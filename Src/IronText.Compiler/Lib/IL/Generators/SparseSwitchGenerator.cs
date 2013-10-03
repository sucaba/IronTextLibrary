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
        private SwitchGeneratorAction action;
        private List<Ref<Labels>>     labels;

        public int                    MaxLinearCount = 3;
        private EmitSyntax emit;
        private Pipe<EmitSyntax> ldvalue;
        private readonly IIntMap<int>  intMap;
        private readonly IntInterval   possibleBounds;
        private readonly IIntFrequency frequency;

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
            this.intMap = intMap;
            if (frequency == null)
            {
                this.possibleBounds = new IntInterval(int.MinValue, int.MaxValue);
                this.frequency = new UniformIntFrequency(possibleBounds);
            }
            else
            {
                this.possibleBounds = possibleBounds;
                this.frequency = frequency;
            }
        }

        protected override void DoBuild(
            EmitSyntax            emit,
            Pipe<EmitSyntax>      ldvalue,
            SwitchGeneratorAction action)
        {
            this.action = action;

            var platformInfo = new DecisionTreePlatformInfo(
                                    branchCost:            7,
                                    switchCost:            3,
                                    maxSwitchElementCount: 1024,
                                    minSwitchDensity:      0.5);

#if false
            var decisionTree = new BinaryDecisionTreeBuilder(intMap.DefaultValue, platformInfo);
            var node = decisionTree.Build(intMap.Enumerate().ToArray());
#else
            var decisionTree = new DecisionTreeBuilder(-1, platformInfo);
            var node = decisionTree.Build(
                    intMap,
                    possibleBounds,
                    frequency);
#endif
            this.emit = emit;
            this.ldvalue = ldvalue;
            this.labels = new List<Ref<Labels>>(64);
            node.Accept(this);

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
        }

        public void Visit(JumpTableDecision decision)
        {
            emit
                .Label(GetNodeLabel(decision).Def)
                .Do(ldvalue)
                .Ldc_I4(decision.StartElement)
                .Sub()
                .Switch(decision.ElementToAction.Select(GetNodeLabel).ToArray())
                ;
        }

        private Ref<Labels> GetNodeLabel(Decision node)
        {
            if (!node.Label.HasValue)
            {
                node.Label = labels.Count;
                labels.Add(emit.Labels.Generate().GetRef());
            }

            return labels[node.Label.Value];
        }

    }
}
