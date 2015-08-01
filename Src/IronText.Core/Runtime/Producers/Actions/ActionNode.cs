using IronText.Collections;
using IronText.Logging;

namespace IronText.Runtime
{
    public delegate void DataAction(IDataContext dataContext);

    public interface IDataContext
    {
        object GetSynthesized(string name);
        void SetSynthesized(string name, object value);

        object GetSynthesized(int position, string name);

        object GetInherited(string name);
    }

    public class ActionNode
    {
        public readonly int    Token;
        public object Value;
        public readonly Loc    Location;
        public readonly HLoc   HLocation;

        /// <summary>
        /// Inherited properties of a state (union of inherited properties of all tokens following this state).
        /// </summary>
        public PropertyValueNode FollowingStateProperties;

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

        public object GetTokenProperty(string name)
        {
            object result;
            TokenProperties.TryGetValue(name, out result);
            return result;
        }

        public void SetTokenProperty(string name, object value)
        {
            TokenProperties = new PropertyValueNode(name, value).SetNext(TokenProperties);
        }

        public object GetFollowingStateProperty(string name)
        {
            object result;
            FollowingStateProperties.TryGetValue(name, out result);
            return result;
        }

        public void SetFollwoingStateProperty(string name, object value)
        {
            FollowingStateProperties = new PropertyValueNode(name, value).SetNext(FollowingStateProperties);
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