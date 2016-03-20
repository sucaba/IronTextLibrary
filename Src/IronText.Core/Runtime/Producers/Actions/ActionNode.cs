using IronText.Collections;
using IronText.Logging;

namespace IronText.Runtime
{
    public class ActionNode
    {
        public readonly int    Token;
        public object Value;
        public readonly Loc    Location;
        public readonly HLoc   HLocation;

        /// <summary>
        /// Inherited properties of a state (union of inherited properties of all tokens following this state).
        /// </summary>
        public PropertyValueNode InheritedStateProperties;

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
            this.InheritedStateProperties = stateProperties;
        }

        public object GetSynthesizedProperty(int synthIndex)
        {
            object result;
            TokenProperties.TryGetValue(synthIndex, out result);
            return result;
        }

        public void SetSynthesizedProperty(int synthIndex, object value)
        {
            TokenProperties = new PropertyValueNode(synthIndex, value).SetNext(TokenProperties);
        }

        public object GetInheritedStateProperty(int inhIndex)
        {
            object result;
            InheritedStateProperties.TryGetValue(inhIndex, out result);
            return result;
        }

        public void SetInheritedStateProperty(int inhIndex, object value)
        {
            InheritedStateProperties = new PropertyValueNode(inhIndex, value).SetNext(InheritedStateProperties);
        }
    }

    public class PropertyValueNode : SListNode<PropertyValueNode>
    {
        public PropertyValueNode(int index, object value)
        {
            this.Index  = index;
            this.Value = value;
        }

        public int    Index { get; private set; }

        public object Value { get; private set; }
    }

    public static class PropertyValueNodeExtensions
    {
        public static bool TryGetValue(this PropertyValueNode self, int inhIndex, out object value)
        {
            PropertyValueNode node = self;
            while (node != null)
            {
                if (node.Index == inhIndex)
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