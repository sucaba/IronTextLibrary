using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IronText.Automata.Regular;
using IronText.Framework;

namespace IronText.MetadataCompiler
{
    interface IScanAmbiguityResolver
    {
        /// <summary>
        /// Register tokens produced by an action
        /// </summary>
        /// <param name="action"></param>
        /// <param name="disambiguation"></param>
        /// <param name="mainToken"></param>
        /// <param name="tokens"></param>
        void RegisterAction(
            int              action,
            Disambiguation   disambiguation,
            int              mainToken,
            IEnumerable<int> tokens);

        /// <summary>
        /// Register actions invoked in state
        /// </summary>
        /// <param name="state"></param>
        /// 
        void RegisterState(TdfaState state);

        /// <summary>
        /// Define ambiguous tokens
        /// </summary>
        /// <param name="grammar"></param>
        void DefineAmbiguities(BnfGrammar grammar);
    }
}
