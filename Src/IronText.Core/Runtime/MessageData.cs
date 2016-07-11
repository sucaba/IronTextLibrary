using IronText.Collections;

namespace IronText.Runtime
{
    public class MessageData : Ambiguous<MessageData>
    {
        public MessageData(int token, string text, int action)
        {
            Token  = token;
            Text   = text;
            Action = action;
        }

        public MessageData(int token, string text, object externalValue)
        {
            Token   = token;
            Text    = text;
            Action  = -1;
            ExternalValue = externalValue;
        }

        public int    Token         { get; }

        public int    Action        { get; }

        public string Text          { get;  }

        public object ExternalValue { get; set; }
    }
}
