using System;
using System.Collections.Generic;
using System.Linq;
using IronText.Algorithm;
using IronText.Framework;
using IronText.Lib.Shared;

namespace IronText.Lib.IL.Generators
{
    class SparseSwitchGenerator : SwitchGenerator, IDecisionVisitor
    {
        private readonly DecisionTreePlatformInfo platformInfo 
                                                        = new DecisionTreePlatformInfo(
                                                                branchCost:            3,
                                                                switchCost:            10,
                                                                maxSwitchElementCount: 256,
                                                                minSwitchDensity:      0.5);

        private SwitchGeneratorAction action;
        private List<Ref<Labels>>     labels;
        private readonly IDecisionTreeGenerationStrategy strategy;

        public int                    MaxLinearCount = 3;
        private EmitSyntax emit;
        private Pipe<EmitSyntax> ldvalue;
        private readonly IIntMap<int>  intMap;
        private readonly IntInterval   possibleBounds;
        private readonly IIntFrequency frequency;
        private DecisionTreeBuilder builder;

        public SparseSwitchGenerator(
            IntArrow<int>[] intArrows,
            int defaultValue,
            IntInterval possibleBounds,
            IIntFrequency frequency = null)
            : this(
                new MutableIntMap<int>(intArrows, defaultValue), 
                possibleBounds,
                frequency)
        {
        }

        public SparseSwitchGenerator(IIntMap<int> intMap, IntInterval possibleBounds)
            : this(intMap, possibleBounds, null)
        {
        }

        public SparseSwitchGenerator(
            IIntMap<int>  intMap,
            IntInterval   possibleBounds,
            IIntFrequency frequency)
        {
            this.strategy = new InlineFirstDTStrategy(this);
            this.intMap = intMap;
            this.possibleBounds = possibleBounds;
            this.frequency = frequency ?? new UniformIntFrequency(possibleBounds);
        }

        protected override void DoBuild(
            EmitSyntax            emit,
            Pipe<EmitSyntax>      ldvalue,
            SwitchGeneratorAction action)
        {
            this.action = action;

#if false
            var decisionTree = new BinaryDecisionTreeBuilder(intMap.DefaultValue, platformInfo);
            var node = decisionTree.Build(intMap.Enumerate().ToArray());
#else
            this.builder = new DecisionTreeBuilder(platformInfo);
            var node = builder.Build(
                    intMap,
                    possibleBounds,
                    frequency);
#endif
            this.emit = emit;
            this.ldvalue = ldvalue;
            this.labels = new List<Ref<Labels>>();

            strategy.PlanCode(node);
            strategy.GenerateCode();

            // Debug.Write(node);
        }

        void IDecisionVisitor.Visit(ActionDecision decision)
        {
            emit.Label(GetNodeLabel(decision).Def);
            this.action(emit, decision.Action);
        }

        void IDecisionVisitor.Visit(RelationalBranchDecision decision)
        {
            emit
                .Label(GetNodeLabel(decision).Def)
                .Do(ldvalue)
                .Ldc_I4(decision.Operand)
                ;

            var label = GetNodeLabel(decision.Right);
            switch (decision.Operator.Negate())
            {
                case RelationalOperator.Equal:          emit.Beq(label);    break;
                case RelationalOperator.NotEqual:       emit.Bne_Un(label); break;
                case RelationalOperator.Less:           emit.Blt(label);    break;
                case RelationalOperator.Greater:        emit.Bgt(label);    break;
                case RelationalOperator.LessOrEqual:    emit.Ble(label);    break;
                case RelationalOperator.GreaterOrEqual: emit.Bge(label);    break;
                default:
                    throw new InvalidOperationException("Not supported operator");
            }

            GenerateCodeOrJump(decision.Left);

            strategy.IntermediateGenerateCode();
        }

        void IDecisionVisitor.Visit(JumpTableDecision decision)
        {
            emit
                .Label(GetNodeLabel(decision).Def)
                .Do(ldvalue)
                ;
            if (decision.StartElement != 0)
            {
                emit
                    .Ldc_I4(decision.StartElement)
                    .Sub();
            }

            emit
                .Switch(decision.ElementToAction.Select(GetNodeLabel).ToArray());
            // default case:
            GenerateCodeOrJump(builder.DefaultActionDecision);

            foreach (var leaf in decision.LeafDecisions)
            {
                strategy.PlanCode(leaf);
            }

            strategy.IntermediateGenerateCode();
        }

        private Ref<Labels> GetNodeLabel(Decision decision)
        {
            if (!decision.Label.HasValue)
            {
                decision.Label = labels.Count;
                labels.Add(emit.Labels.Generate().GetRef());

                strategy.PlanCode(decision);
            }

            return labels[decision.Label.Value];
        }

        private void GenerateCodeOrJump(Decision decision)
        {
            if (!strategy.TryInlineCode(decision))
            {
                emit.Br(GetNodeLabel(decision));
            }
        }

    }
}
