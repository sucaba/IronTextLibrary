using IronText.Logging;
using System;

namespace IronText.Runtime
{
    /// <summary>
    /// Represents error caused by particular datum.
    /// </summary>
    public class SyntaxException : ApplicationException
    {
        private readonly string message;
        private readonly Loc location;

        public SyntaxException(Loc location, string message)
        {
            this.location = location;
            this.message = message;
        }

        /// <summary>
        /// Error message containing location, datum text and error explanation.
        /// </summary>
        public override string Message
        {
            get
            {
                string locationText;
                if (location.IsUnknown)
                {
                    locationText = "";
                }
                else
                {
                    locationText = location.ToString() + ": ";
                }

                return string.Format("{0}{1}", locationText, message);
            }
        }
    }
}
