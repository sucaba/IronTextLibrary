using IronText.Collections;

namespace IronText.Runtime
{
    public class MessageData : Ambiguous<MessageData>
    {
        public readonly int    Token;
        public readonly int    Action;
        public readonly string Text;
        public object          ExternalValue;

        public MessageData(int token, string text, int action)
        {
            Token  = token;
            Action = action;
            Text   = text;
        }

        public MessageData(int token, string text, object externalValue)
        {
            Token = token;
            Action = -1;
            Text = text;
            ExternalValue = externalValue;
        }
    }
}
