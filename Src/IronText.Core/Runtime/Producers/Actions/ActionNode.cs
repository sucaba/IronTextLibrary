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
        public readonly PropertyValueNode FollowingStateProperties;

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
            this.FollowingStateProperties = stateProperties;
        }

        public void SetTokenProperty(string name, object value)
        {
            TokenProperties = new PropertyValueNode(name, value).SetNext(TokenProperties);
        }
    }

    public class PropertyValueNode : SListNode<PropertyValueNode>
    {
        public PropertyValueNode(string name, object value)
        {
            this.Name  = name;
            this.Value = value;
        }

        public string Name  { get; private set; }

        public object Value { get; private set; }

    }

    public static class PropertyValueNodeExtensions
    {
        public static bool TryGetValue(this PropertyValueNode self, string name, out object value)
        {
            PropertyValueNode node = self;
            while (node != null)
            {
                if (node.Name == name)
                {
                    value = node.Value;
                    return true;
                }

                node = node.Next;
            }

            value = null;
            return false;
        }
    }

}