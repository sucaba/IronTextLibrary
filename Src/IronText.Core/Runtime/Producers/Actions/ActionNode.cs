using IronText.Collections;
using IronText.Logging;

namespace IronText.Runtime
{
    public delegate void DataAction(IDataContext dataContext);

    public interface IDataContext
    {
        object GetOutcomeProperty(string name);
        void SetOutcomeProperty(string name, object value);

        object GetInputProperty(int position, string name);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="position">
        /// Production input position (before the corresponding input token).
        /// <c>Production.InputLength</c> position corresponds to the position after the last token.</param>
        /// <param name="name">Inherited attribute name</param>
        /// <returns></returns>
        object GetInherited(int position, string name);

        /// <summary>
        /// Set inherited attribute value which will be used after the pending production.
        /// </summary>
        /// <param name="name">Attribute name</param>
        /// <param name="value">Attribute value</param>
        void SetInherited(string name, object value);
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