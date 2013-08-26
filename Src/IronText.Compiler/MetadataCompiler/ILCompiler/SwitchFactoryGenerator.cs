using System.Linq;
using IronText.Extensibility;
using IronText.Framework;
using IronText.Lib.IL;
using IronText.Lib.IL.Generators;

namespace IronText.MetadataCompiler
{
    internal class SwitchFactoryGenerator
    {
        private readonly SwitchRule[] switchRules;
        private readonly ITokenRefResolver tokenRefResolver;

        public SwitchFactoryGenerator(
                SwitchRule[]      switchRules,
                ITokenRefResolver tokenRefResolver)
        {
            this.switchRules = switchRules;
            this.tokenRefResolver = tokenRefResolver;
        }

        public void Build(
            EmitSyntax emit,
            IContextResolverCode contextResolver,
            Pipe<EmitSyntax> ldThisId,
            Pipe<EmitSyntax> ldExit,
            Pipe<EmitSyntax> ldLanguage)
        {
                // Enumerable.Range(0, switchRules.Length).ToArray();

            if (switchRules.Length == 0)
            {
                emit
                    .Ldnull()
                    .Ret();
                return;
            }

            var idToRule = switchRules.ToDictionary(
                switchRule => tokenRefResolver.GetId(switchRule.Tid),
                switchRule => switchRule);
            int count = switchRules.Max(r => tokenRefResolver.GetId(r.Tid)) + 1;
            int[] map = new int[count];
            for (int i = 0; i != count; ++i)
            {
                map[i] = -1;
            }

            for (int i = 0; i != switchRules.Length; ++i)
            {
                map[tokenRefResolver.GetId(switchRules[i].Tid)] = i;
            }

            ISwitchActionCode code = new SwitchActionCode(
                    contextResolver,
                    emit,
                    ldExit,
                    ldLanguage);

            var switchEmitter = SwitchGenerator.Contiguous(map, -1);
            switchEmitter.Build(
                emit,
                ldThisId,
                (il, switchRuleIndex) =>
                {
                    if (switchRuleIndex >= 0)
                    {
                        switchRules[switchRuleIndex].ActionBuilder(code);
                    }
                    else
                    {
                        il
                            .Ldnull()
                            .Ret();
                    }
                });
        }
    }
}
