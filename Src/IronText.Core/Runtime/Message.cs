using IronText.Logging;
using System;

namespace IronText.Runtime
{
    public sealed class Message
        : MessageData
        , IEquatable<Message>
    {
        /// <summary>
        /// Envelope Id. It can be either token ID or ambiguous token ID.
        /// </summary>
        public readonly int AmbiguousToken;

        /// <summary>
        /// Line, column based location for a human
        /// </summary>
        public readonly Loc Location;

        public Message(
            int    token,
            string text,
            object value,
            Loc    location)
            : base(token, text, value)
        {
            this.AmbiguousToken = token;
            this.Location       = location;
        }

        public Message(
            int    token,
            string text,
            int    action,
            Loc    location)
            : base(token, text, action)
        {
            this.AmbiguousToken = token;
            this.Location       = location;
        }

        internal Message(
            int     ambiguosToken,
            int     token,
            int     action,
            string  text,
            Loc     location)
            : base(token, text, action)
        {
            this.AmbiguousToken = ambiguosToken;
            this.Location       = location;
        }

        public MessageData FirstData { get { return this; } }

        public override bool Equals(object obj)
        {
            var casted = (Message)obj;
            return Equals(casted);
        }

        public bool Equals(Message other)
        {
            return AmbiguousToken == other.AmbiguousToken
                && Text == other.Text
                && Location == other.Location;
        }

        public override int GetHashCode()
        {
            return AmbiguousToken
                ^ Location.FirstColumn
                ^ Location.LastLine;
        }

        public override string ToString() =>
            $"<Msg Id={AmbiguousToken}, Text={Text}, Loc={Location}>";
    }
}
