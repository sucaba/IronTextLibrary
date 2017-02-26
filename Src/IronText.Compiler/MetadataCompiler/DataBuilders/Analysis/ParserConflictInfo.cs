using IronText.Runtime;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace IronText.MetadataCompiler.Analysis
{
    [Serializable]
    public class ParserConflictInfo
    {
        private readonly List<ParserInstruction> actions = new List<ParserInstruction>();

        internal ParserConflictInfo(int state, int token)
        {
            this.State = state;
            this.Token = token;
            this.Actions = new ReadOnlyCollection<ParserInstruction>(this.actions);
        }

        public int State { get; private set; }

        public int Token { get; private set; }

        public ReadOnlyCollection<ParserInstruction> Actions { get; private set; }

        internal void AddAction(ParserInstruction action)
        {
            actions.Add(action);
        }
    }
}
