using System.Linq;
using IronText.Algorithm;
using IronText.Automata.Regular;
using IronText.Extensibility;
using IronText.Framework;
using IronText.Lib.IL;
using IronText.Lib.IL.Generators;
using IronText.Lib.Shared;

namespace IronText.MetadataCompiler
{
    class ScannerGenerator
    {
        public const int Sentinel = 0;

        private readonly IDfaSerialization dfa;

        public ScannerGenerator(IDfaSerialization dfa)
        {
            this.dfa = dfa;
        }

        public void Build(EmitSyntax emit, IContextResolverCode contextResolverCode, int startRuleId)
        {
            // Debug.WriteLine("DFA for " + descriptor.Name + ":");
            // Debug.WriteLine(dfa);
            int failState  = dfa.StateCount;

            var locals = emit.Locals;
            var labels = emit.Labels;

            var FIN     = labels.Generate();
            var current = locals.Generate("ch");

            var Lgoto = new Ref<Labels>[dfa.StateCount + 1]; // label for incrementing ch position
            var Mgoto = new Ref<Labels>[dfa.StateCount];     // assing ch using current position
            var Tgoto = new Ref<Labels>[dfa.StateCount];     // transition dispatching switch-code

            for (int i = 0; i != dfa.StateCount; ++i)
            {
                Lgoto[i] = labels.Generate().GetRef();
                Mgoto[i] = labels.Generate().GetRef();
                Tgoto[i] = labels.Generate().GetRef();
            }

            Lgoto[failState] = FIN.GetRef();

            emit
                .Local(current, emit.Types.Char)
                .Ldarg(0) .Ldfld((ScanCursor c) => c.InnerState)
                .Switch(Mgoto)
                .Br(Lgoto[0])
                ;

            foreach (var S in dfa.EnumerateStates())
            {
                int state = S.Index;

                emit
                    .Label(Lgoto[state].Def);

                if (state != dfa.Start)
                {
                    // ++Cursor
                    emit
                        .Ldarg(0) // for Stfld
                        .Ldarg(0).Ldfld((ScanCursor c) => c.Cursor)
                        .Ldc_I4_1()
                        .Add()
                        .Stfld((ScanCursor c) => c.Cursor)
                    ;
                }
                
                // If state is accepring then remember position 
                // in Marker and save corresponding ActionId.
                if (S.IsAccepting)
                {
                    int i = 0;
                    foreach (var action in S.Actions)
                    {
                        emit
                            .Ldarg(0)
                            .Ldfld((ScanCursor c) => c.Actions)
                            .Ldc_I4(i)
                            .Ldc_I4(action + startRuleId) 
                            .Stelem_I4()
                            ;
                    }

                    emit
                        .Ldarg(0)
                        .Ldc_I4(S.Actions.Count)
                        .Stfld((ScanCursor c) => c.ActionCount)
                        ;

                    emit
                        .Ldarg(0)
                        .Ldarg(0) .Ldfld((ScanCursor c) => c.Cursor)
                        .Stfld((ScanCursor c) => c.Marker);

                    // Save line/column information

                    emit
                        // cursor.MarkerLine = cursor.CursorLine;
                        .Ldarg(0)
                        .Ldarg(0)
                        .Ldfld((ScanCursor c) => c.CursorLine)
                        .Stfld((ScanCursor c) => c.MarkerLine)
                        // cursor.MarkerLineStart = cursor.CursorLineStart;
                        .Ldarg(0)
                        .Ldarg(0)
                        .Ldfld((ScanCursor c) => c.CursorLineStart)
                        .Stfld((ScanCursor c) => c.MarkerLineStart)
                        ;
                }

                if (S.IsNewline)
                {
                    emit
                        // ++cursor.CursorLine;
                        .Ldarg(0)
                        .Ldarg(0)
                        .Ldfld((ScanCursor c) => c.CursorLine)
                        .Ldc_I4_1()
                        .Add()
                        .Stfld((ScanCursor c) => c.CursorLine)
                        // cursor.CursorLineStart = cursor.Cursor;
                        .Ldarg(0)
                        .Ldarg(0)
                        .Ldfld((ScanCursor c) => c.Cursor)
                        .Stfld((ScanCursor c) => c.CursorLineStart)
                        ;
                }

                emit
                    .Label(Mgoto[state].Def)
                // Get current input symbol
                    .Ldarg(0) .Ldfld((ScanCursor c) => c.Buffer)
                    .Ldarg(0) .Ldfld((ScanCursor c) => c.Cursor)
                    .Ldelem_U2()
                    .Stloc(current.GetRef())
                // Label for tunneling
                    .Label(Tgoto[state].Def)
                    ;

                if (S.IsFinal && state != 0)
                {
                    emit
                        .Br(FIN.GetRef());
                    continue; 
                }

                int checkSentinelState = failState + 1;

                var stateRealTransitions = dfa.EnumerateRealTransitions(S).ToList();


                // Find custom sentinel transtion
                int customSentinelToState = -1;
                var customSentinelTransitionIndex = 
                    stateRealTransitions.FindIndex(pair => pair.Key.Contains(Sentinel));
                if (customSentinelTransitionIndex >= 0)
                {
                    customSentinelToState = stateRealTransitions[customSentinelTransitionIndex].Value;
                }

                // Sentinel check transiiton
                var sentinelTransition = new IntArrow<int>(Sentinel, checkSentinelState);
                stateRealTransitions.Insert(0, sentinelTransition);

                var generator = SwitchGenerator.Sparse(
                                    stateRealTransitions.ToArray(), 
                                    failState,
                                    UnicodeIntSetType.UnicodeInterval,
                                    frequency: UnicodeFrequency.Default);
                generator.Build(
                    emit,
                    il => il.Ldloc(current.GetRef()),
                    (EmitSyntax il, int value) =>
                    {
                        if (value == checkSentinelState)
                        {
                            var handleSentinelCharLabel = 
                                    (customSentinelToState >= 0) 
                                    ? Lgoto[customSentinelToState] 
                                    : FIN.GetRef();

                            // return for buffer filling if Cursor == Limit, 
                            // otherwise transite on char
                            il
                                .Ldarg(0) .Ldfld((ScanCursor c) => c.Limit)
                                .Ldarg(0) .Ldfld((ScanCursor c) => c.Cursor)
                                .Bne_Un(handleSentinelCharLabel)
                                
                                // Return for buffer filling
                                .Ldarg(0)
                                .Ldc_I4(state)
                                .Stfld((ScanCursor c) => c.InnerState)
                                .Ldc_I4(1)
                                .Ret()
                                ;
                        }
                        else if (value == failState && S.Tunnel >= 0)
                        {
                            // Tunnel-jump to other state and verify current
                            // input again in that state.
                            il.Br(Tgoto[S.Tunnel]);
                        }
                        else
                        {
                            // Transite to the valid or failed state
                            il.Br(Lgoto[value]);
                        }
                    });
            }

            // Return to the last accepted position and report accept stage
            emit
                .Label(FIN)
                .Ldc_I4(0)
                .Ret()
                ;
        }
    }
}
