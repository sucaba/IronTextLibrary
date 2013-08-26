using IronText.Framework;
using IronText.Lib.Shared;

namespace IronText.Lib.IL.Generators
{
    class ContiguousSwitchGenerator : SwitchGenerator
    {
        private readonly int[] map;
        private readonly int defaultValue;

        public ContiguousSwitchGenerator(int[] map, int defaultValue)
        {
            this.map = map;
            this.defaultValue = defaultValue;
        }

        protected override void DoBuild(EmitSyntax emit, Pipe<EmitSyntax> ldvalue, SwitchGeneratorAction action)
        {
            var labels = new Ref<Labels>[map.Length];
            for (int i = 0; i != labels.Length; ++i)
            {
                labels[i] = emit.Labels.Generate().GetRef();
            }

            emit
                .Do(ldvalue)
                .Switch(labels)
                ;

            action(emit, defaultValue);

            for (int i = 0; i != labels.Length; ++i)
            {
                emit.Label(labels[i].Def);
                action(emit, map[i]);
            }
        }
    }
}
