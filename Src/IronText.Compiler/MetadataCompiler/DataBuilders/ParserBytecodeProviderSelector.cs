using IronText.Automata;
using IronText.Automata.TurnPlanning;
using IronText.DI;
using IronText.Runtime;
using System;

namespace IronText.MetadataCompiler
{
    class ParserBytecodeProviderSelector : IDynamicDependency<IParserBytecodeProvider>
    {
        public Type Implementation { get; }

        public ParserBytecodeProviderSelector(ParserRuntime runtime)
        {
            switch (runtime)
            {
                case ParserRuntime.Generic:
                    Implementation = typeof(ParserPlanBytecodeProvider);
                    break;
                default:
                    Implementation = typeof(ParserBytecodeProvider);
                    break;
            }
        }
    }
}
