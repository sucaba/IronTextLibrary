using System;
using System.Collections.Generic;
using IronText.Extensibility;

namespace IronText.MetadataCompiler
{
    public class TokenCollection : IEnumerable<TokenRef>
    {
        private readonly List<TokenRef> tokens;
        private readonly Dictionary<string, TokenRef> byLiteral = new Dictionary<string,TokenRef>();
        private readonly Dictionary<Type, TokenRef> byType = new Dictionary<Type,TokenRef>();
        private readonly Dictionary<TokenRef, TokenRef> byEquivalent = new Dictionary<TokenRef,TokenRef>();

        public TokenCollection()
        {
            this.tokens = new List<TokenRef>();
        }

        public TokenCollection(int capacity)
        {
            this.tokens = new List<TokenRef>(capacity);
        }

        public TokenCollection(IEnumerable<TokenRef> tokens)
        {
            this.tokens = new List<TokenRef>(tokens);
        }

        public int Count { get { return tokens.Count; } }

        public void Add(TokenRef token)
        {
            tokens.Add(token);

            if (token.IsLiteral)
            {
                byLiteral.Add(token.LiteralText, token);
            }
            else
            {
                byType.Add(token.TokenType, token);
            }
        }

        public TokenRef this[int index] { get { return tokens[index]; } }

        public TokenRef this[string literal] { get { return byLiteral[literal]; } }

        public TokenRef this[Type type] { get { return byType[type]; } }

        public bool TryGet(string literal, out TokenRef output)
        {
            return byLiteral.TryGetValue(literal, out output);
        }

        public bool TryGet(Type tokenType, out TokenRef output)
        {
            return byType.TryGetValue(tokenType, out output);
        }

        public bool Contains(string literal) { return byLiteral.ContainsKey(literal); }

        public bool Contains(Type tokenType) { return byType.ContainsKey(tokenType); }

        public IEnumerator<TokenRef> GetEnumerator()
        {
            return tokens.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
