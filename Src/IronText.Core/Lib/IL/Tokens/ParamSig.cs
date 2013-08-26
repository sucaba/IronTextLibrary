using System;
using System.Reflection;

namespace IronText.Lib.IL
{
    public class ParamSig
    {
        public const string InDirection  = "[in] ";
        public const string OutDirection = "[out] ";

        public TypeSig            ParameterType;
        public ParamAttr Direction;

        public static ParamSig Parse(string text)
        {
            var result = new ParamSig();
            if (text.StartsWith(OutDirection))
            {
                result.Direction |= ParamAttr.Out;
                text = text.Substring(OutDirection.Length);
            }
            if (text.StartsWith(InDirection))
            {
                result.Direction |= ParamAttr.In;
                text = text.Substring(InDirection.Length);
            }

            result.ParameterType = TypeSig.Parse(text);
            return result;
        }

        public static ParamSig FromParameterInfo(ParameterInfo parameter)
        {
            var result = ParamSig.FromType(parameter.ParameterType);

            if (parameter.Attributes == ParameterAttributes.None)
            {
                result.Direction = ParamAttr.None;
            }
            else if (parameter.Attributes == ParameterAttributes.In)
            {
                result.Direction =  ParamAttr.In;
            }
            else if (parameter.Attributes == ParameterAttributes.Out)
            {
                result.Direction = ParamAttr.Out;
            }
            else
            {
                throw new NotImplementedException(
                    "Not supported parameter attribute." 
                    + Enum.GetName(typeof(ParameterAttributes), parameter.Attributes));
            }

            return result;
        }

        public static ParamSig FromType(Type type)
        {
            return new ParamSig
            {
                Direction = ParamAttr.In,
                ParameterType = TypeSig.FromType(type)
            };
        }
    }
}
