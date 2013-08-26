using System;
using System.Collections.Generic;
using IronText.Algorithm;
using IronText.Framework;
using IronText.Lib.Shared;

namespace IronText.Lib.IL.Generators
{
    public delegate void SwitchGeneratorAction(EmitSyntax emit, int value);

    public abstract class SwitchGenerator
    {
        protected Def<Labels>[] valueLabels;
        protected KeyValuePair<int, Action<EmitSyntax>>[] valueCodeBuilders;
        protected Def<Labels> END;

        public static SwitchGenerator Contiguous(int[] map, int defaultValue) 
        { 
            return new ContiguousSwitchGenerator(map, defaultValue); 
        }

        public static SwitchGenerator Sparse(
            IIntMap<int>  arrows,
            IntInterval   possibleBounds,
            IIntFrequency frequency = null) 
        {
            var result = new SparseSwitchGenerator(arrows, possibleBounds, frequency);
            return result;
        }

        public static SwitchGenerator Sparse(
            IntArrow<int>[] intervalToValue,
            int defaultValue,
            IntInterval   possibleBounds,
            IIntFrequency frequency = null) 
        { 
            var result = new SparseSwitchGenerator(intervalToValue, defaultValue, possibleBounds, frequency);
            return result;
        }

        public void Build(EmitSyntax emit, Ref<Args>[] args, SwitchGeneratorAction action)
        {
            Build(emit, il => il.Ldarg(args[0]), action);
        }

        public void Build(EmitSyntax emit, Pipe<EmitSyntax> ldvalue, SwitchGeneratorAction action)
        {
            this.END = emit.Labels.Generate();
            DoBuild(emit, ldvalue, action);
        }

        protected abstract void DoBuild(EmitSyntax emit, Pipe<EmitSyntax> ldvalue, SwitchGeneratorAction action);

        public static void RetValueAction(EmitSyntax emit, int value)
        {
            emit
                .Ldc_I4(value)
                .Ret();
        }

        public static SwitchGeneratorAction LdValueAction(Ref<Labels> END)
        {
            return (EmitSyntax emit, int value) =>
                {
                    emit
                        .Ldc_I4(value)
                        .Br(END);
                };
        }

        public static SwitchGeneratorAction LabelJumpAction(Ref<Labels>[] valueLabels)
        {
            return (EmitSyntax emit, int value) =>
                {
                    emit.Br(valueLabels[value]);
                };
        }
    }
}
