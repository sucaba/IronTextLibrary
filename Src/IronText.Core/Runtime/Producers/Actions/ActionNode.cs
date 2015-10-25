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

        public object GetSynthesizedProperty(string name)
        {
            object result;
            TokenProperties.TryGetValue(name, out result);
            return result;
        }

        public void SetSynthesizedProperty(string name, object value)
        {
            TokenProperties = new PropertyValueNode(name, value).SetNext(TokenProperties);
        }

        public object GetInheritedStateProperty(string name)
        {
            object result;
            InheritedStateProperties.TryGetValue(name, out result);
            return result;
        }

        public void SetInheritedStateProperty(string name, object value)
        {
            InheritedStateProperties = new PropertyValueNode(name, value).SetNext(InheritedStateProperties);
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