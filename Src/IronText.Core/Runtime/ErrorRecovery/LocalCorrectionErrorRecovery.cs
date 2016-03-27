using IronText.Logging;
// #define SPELLING_FEATURE
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace IronText.Runtime
{
    class LocalCorrectionErrorRecovery : IReceiver<Msg>
    {
        /// <summary>
        /// Token count that mast be accepted by a parser after correction.
        /// </summary>
        private const int VerificationLength = 2;
        private static int FailurePatternSize = 5; // including preceding

        private readonly List<Msg> failedInput = new List<Msg>();
        private readonly List<Msg> correction = new List<Msg>();
        private readonly IPushParser exit;
        private ParserCorrectionModel currentModel;
        private bool useViolatingRules;

        private const int Spelling = ParserCorrectionModel.Spelling;
        private const int Insertion = ParserCorrectionModel.Insertion;

        static LocalCorrectionErrorRecovery()
        {
            FailurePatternSize = CorrectionModels.Max(m => m.GetRequiredInputSize()) 
                               + VerificationLength;
        }

        private static readonly ParserCorrectionModel[] CorrectionModels = {
#if SPELLING_FEATURE
            new ParserCorrectionModel
                ("Misspelling of \"$1\" which is replaced by the keyword \"%1\".") 
                { 0, Spelling, 2 },
            new ParserCorrectionModel
                ("Misspelling of \"$0\" before \"$1\" which is replaced by the keyword \"%0\".") 
                { Spelling, 1 },
#endif
            new ParserCorrectionModel
                ("%1 is missing after $0.")  // ("%1 is inserted before $1.") 
                { 0, Insertion, 1, }, 
            new ParserCorrectionModel
                ("Expected %1 but got $1.") // ("$1 is replaced by %1.") 
                { 0, Insertion, 2, }, 
            new ParserCorrectionModel
                ("Unexpected $1.") //("$1 is deleted.") 
                { 0, 2, },
            new ParserCorrectionModel
                ("%1 %2 are missing before $1.") //("%1 %2 is inserted before $1.") 
                { 0, Insertion, Insertion, 1, }, 
            new ParserCorrectionModel
                ("%0 is missing before $0.") //("%0 is inserted before $0 $1.") 
                { Insertion, 0, }, 
            new ParserCorrectionModel
                ("Expected %0 before $1 but got $0.") //("$0 before $1 is replaced by %0.") 
                { Insertion, 1, }, 
            new ParserCorrectionModel
                ("Unexpected $0 before $1.") // ("$0 before $1 is deleted.") 
                { 1, },
            new ParserCorrectionModel
                ("Expected %0 but got $0 $1.") // ("$0 $1 is replaced by %0.") 
                { Insertion, 2, },
            new ParserCorrectionModel
                ("Expected %0 %1 before $1 but got $0.") // ("$0 before $1 is replaced by %0 %1." ) 
                { Insertion, Insertion, 1, }, 
        };
        private readonly RuntimeGrammar grammar;
        private readonly int[] terms;
        private readonly ILogging logging;

        public LocalCorrectionErrorRecovery(
            RuntimeGrammar  grammar,
            IPushParser exit,
            ILogging    logging)
        {
            this.grammar  = grammar;
            this.exit     = exit;
            this.logging  = logging;

            this.terms = grammar
                            .EnumerateTokens()
                            .Where(grammar.IsTerminal)
                            .ToArray();
        }

        public IReceiver<Msg> Next(Msg item)
        {
            failedInput.Add(item);

            if (failedInput.Count == FailurePatternSize || item.AmbToken == PredefinedTokens.Eoi)
            {
                return Recover(false);
            }

            return this;
        }

        public IReceiver<Msg> Done()
        {
            return Recover(true);
        }

        private bool ValidateCorrection()
        {
            IReceiver<Msg> r = exit.CloneVerifier();
            int pos = 0;
            int minLength = currentModel.GetMinimalLength();

            foreach (var msg in correction)
            {
                r = r.Next(msg);

                if (r == null)
                {
                    return false;
                }

                if (pos >= minLength && (PredefinedTokens.Eoi == msg.AmbToken || grammar.IsBeacon(msg.AmbToken)))
                {
                    return true;
                }

                ++pos;
            }

            return true;
        }

        private IReceiver<Msg> Recover(bool done)
        {
            IReceiver<Msg> result;

            // Don't insert tokens in category "don't insert"
            // Don't delete tokens in category "don't delete" 
            useViolatingRules = false;
            if (FindCorrection())
            {
                result = exit.Feed(correction);
            }
            else
            {
                // Last chance to correct error using
                //  insertions of tokens in category "don't insert"
                //  and deletion of tokens in category "don't delete" 
                useViolatingRules = true;
                if (FindCorrection())
                {
                    result = exit.Feed(correction);
                }
                else
                {
                    result = new PanicModeErrorRecovery(grammar, exit, logging)
                             .Feed(failedInput);
                }
            }

            correction.Clear();
            if (done)
            {
                result = result.Done();
            }

            return result;
        }

        private bool FindCorrection()
        {
            foreach (var model in CorrectionModels)
            {
                if (failedInput.Count < model.GetRequiredInputSize())
                {
                    continue;
                }

                var deletedTokens = model.GetDeletedIndexes().Select(i => failedInput[i].AmbToken);
                bool violatesDontDelete = deletedTokens.Any(grammar.IsDontDelete);
                if (!useViolatingRules && violatesDontDelete)
                {
                    continue;
                }
                if (useViolatingRules && !violatesDontDelete)
                {
                    continue;
                }

                correction.Clear();
                int count = model.Count;
                for (int i = 0; i != count; ++i)
                {
                    if (model[i] == Insertion)
                    {
                        correction.Add(default(Msg));
                    }
                    else if (model[i] == Spelling)
                    {
                        throw new NotImplementedException();
                    }
                    else if (model[i] < failedInput.Count)
                    {
                        correction.Add(failedInput[model[i]]);
                    }
                    else
                    {
                        break;
                    }
                }

                // Complete correction with the correction-verifiction suffix
                CompleteCorrectionWithVerificationSuffix(model);

                this.currentModel = model;

                if (Match(0, model))
                {
                    // Complete correction with remaining collected failedInput
                    CompleteCorrectedInput(model);

                    logging.Write(
                        new LogEntry
                        {
                            Severity = Severity.Error,
                            Location = model.GetHiglightLocation(failedInput),
                            Message  = model.FormatMessage(grammar, failedInput, correction)
                        });
                    return true;
                }
            }

            correction.Clear();
            return false;
        }

        private void CompleteCorrectedInput(ParserCorrectionModel model)
        {
            int i = model.GetRequiredInputSize() + VerificationLength;
            int end = failedInput.Count;
            for (; i < end; ++i)
            {
                correction.Add(failedInput[i]);
            }
        }

        private void CompleteCorrectionWithVerificationSuffix(ParserCorrectionModel model)
        {
            int i = model.GetRequiredInputSize();
            int end = i + VerificationLength;
            for (; i != end; ++i)
            {
                if (i >= failedInput.Count)
                {
                    break;
                }

                correction.Add(failedInput[i]);
            }
        }

        private bool Match(int start, ParserCorrectionModel m)
        {
            if (start == m.Count)
            {
                return ValidateCorrection();
            }

            foreach (var candidate in Following(m, start))
            {
                correction[start] = candidate;
                if (Match(start + 1, m))
                {
                    return true;
                }
            }

            return false;
        }

        private IEnumerable<Msg> Following(ParserCorrectionModel m, int k)
        {
            Debug.Assert(k != m.Count);

            if (m[k] == Insertion)
            {
                if (!useViolatingRules)
                {
                    foreach (int term in terms)
                    {
                        var categories = grammar.GetTokenCategories(term);
                        if ((categories & SymbolCategory.DoNotInsert) == 0)
                        {
                            yield return new Msg(term, null, null, Loc.Unknown);
                        }
                    }
                }
                else
                {
                    foreach (int term in terms)
                    {
                        var categories = grammar.GetTokenCategories(term);
                        if ((categories & SymbolCategory.DoNotInsert) != 0)
                        {
                            yield return new Msg(term, null, null, Loc.Unknown); // TODO: Location and value
                        }
                    }
                }
            }
            else if (m[k] == Spelling)
            {
                throw new NotImplementedException();
            }
            else
            {
                yield return failedInput[m[k]];
            }
        }

        private IReceiver<Msg> ParseTransite(IReceiver<Msg> parser, Msg candidate)
        {
            return parser.Next(candidate);
        }
    }
}
