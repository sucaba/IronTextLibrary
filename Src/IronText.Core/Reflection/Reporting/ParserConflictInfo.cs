﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using IronText.Runtime;

namespace IronText.Reflection.Reporting
{
    public class ParserConflictInfo
    {
        private readonly List<ParserAction> actions = new List<ParserAction>();

        internal ParserConflictInfo(int state, int token)
        {
            this.State = state;
            this.Token = token;
            this.Actions = new ReadOnlyCollection<ParserAction>(this.actions);
        }

        public int State { get; private set; }

        public int Token { get; private set; }

        public ReadOnlyCollection<ParserAction> Actions { get; private set; }

        internal void AddAction(ParserAction action)
        {
            actions.Add(action);
        }

        internal void AddAction(int actionCell)
        {
            actions.Add(ParserAction.Decode(actionCell));
        }
    }
}
