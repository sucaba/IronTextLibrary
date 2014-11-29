using IronText.Collections;
using IronText.Logging;

namespace IronText.Runtime
{
    public class ActionNode
    {
        public readonly int    Token;
        public readonly object Value;
        public readonly Loc    Location;
        public readonly HLoc   HLocation;

        /// <summary>
        /// Inherited properties of a state (union of inherited properties of all tokens following this state).
        /// </summary>
        public readonly PropertyValueNode StateProperties;

        /// <summary>
        /// Synth. Properties of a token
        /// </summary>
        public PropertyValueNode TokenProperties { get; private set; }

        public ActionNode(int token, object value, Loc loc, HLoc hLoc, PropertyValueNode stateProperties = null)
        {
            this.Token           = token;
            this.Value           = value;
            this.Location        = loc;
            this.HLocation       = hLoc;
            this.StateProperties = stateProperties;
        }

        public void SetTokenProperty(int propertyIndex, object value)
        {
            TokenProperties = new PropertyValueNode(propertyIndex, value).SetNext(TokenProperties);
        }
    }

    public class PropertyValueNode : SListNode<PropertyValueNode>
    {
        public PropertyValueNode(int propIndex, object value)
        {
            this.PropertyIndex = propIndex;
            this.Value = value;
        }

        public int    PropertyIndex { get; private set; }

        public object Value         { get; private set; }
    }

}