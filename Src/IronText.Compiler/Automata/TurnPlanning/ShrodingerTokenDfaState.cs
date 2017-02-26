﻿using IronText.Common;
using System.Collections.Generic;
using System;

namespace IronText.Automata.TurnPlanning
{
    class ShrodingerTokenDfaState
    {
        public static ShrodingerTokenDfaState FailState { get; }
            = new ShrodingerTokenDfaState();

        public Dictionary<int, ShrodingerTokenDecision> Transitions { get; private set; }
        
        public ShrodingerTokenDfaState()
        {
        }

        public ShrodingerTokenDfaState(Dictionary<int, ShrodingerTokenDecision> transitions)
        {
            this.Transitions = transitions;
        }

        public void Init(Dictionary<int, ShrodingerTokenDecision> transitions)
        {
            this.Transitions = transitions;
        }

        public void AddDecision(int token, ShrodingerTokenDecision decision)
        {
            if (decision != ShrodingerTokenDecision.NoAlternatives)
            {
                Transitions.Add(token, decision);
            }
        }

        public ShrodingerTokenDecision GetDecision(int token) =>
            Transitions.GetOrDefault(token);

        public ShrodingerTokenDfaState GetNext(int token) =>
            Transitions.GetOrDefault(token)?.NextState ?? FailState;
    }
}