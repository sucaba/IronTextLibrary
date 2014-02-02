using System;
using System.Reflection;

namespace IronText.Misc
{
    internal class TypePattern
    {
        private readonly MethodInfo matcher;
        private readonly Type patternType;
        private readonly Type[] placeholders;

        public TypePattern(MethodInfo matcher)
        {
            this.matcher = matcher;
            this.patternType = matcher.ReturnType;

            if (matcher.IsGenericMethodDefinition)
            {
                this.placeholders = matcher.GetGenericArguments();
            }
            else
            {
                this.placeholders = Type.EmptyTypes;
            }
        }

        public int PlaceholderCount { get { return placeholders.Length; } }

        public Type[] Match(Type type)
        {
            Type[] result = placeholders.Length == 0 ? Type.EmptyTypes : null;
            if (!Match(patternType, type, ref result))
            {
                return null;
            }

            return result;
        }

        public MethodInfo MakeProducer(Type resultType)
        {
            var types = Match(resultType);
            if (types == null)
            {
                return null;
            }

            if (placeholders.Length == 0)
            {
                return matcher;
            }

            return matcher.MakeGenericMethod(types);
        }

        private bool Match(Type patternType, Type instanceType, ref Type[] output)
        {
            if (patternType.IsArray)
            {
                if (!instanceType.IsArray || patternType.GetArrayRank() != instanceType.GetArrayRank())
                {
                    return false;
                }

                return Match(patternType.GetElementType(), instanceType.GetElementType(), ref output);
            }

            if (patternType.IsPointer)
            {
                if (!instanceType.IsPointer)
                {
                    return false;
                }

                return Match(patternType.GetElementType(), instanceType.GetElementType(), ref output);
            }

            if (patternType.IsByRef)
            {
                if (!instanceType.IsByRef)
                {
                    return false;
                }

                return Match(patternType.GetElementType(), instanceType.GetElementType(), ref output);
            }


            if (patternType.IsGenericType)
            {
                if (!instanceType.IsGenericType)
                {
                    return false;
                }

                var patternGenericDef = patternType.GetGenericTypeDefinition();
                var instanceGenericDef = instanceType.GetGenericTypeDefinition();
                if (!patternGenericDef.Equals(instanceGenericDef))
                {
                    return false;
                }

                var patternTypeArgs = patternType.GetGenericArguments();
                var instanceTypeArgs = instanceType.GetGenericArguments();
                for (int i = 0; i != instanceTypeArgs.Length; ++i)
                {
                    if (!Match(patternTypeArgs[i], instanceTypeArgs[i], ref output))
                    {
                        return false;
                    }
                }

                return true;
            }

            if (patternType.IsGenericParameter)
            {
                int index = Array.IndexOf(placeholders, patternType);
                if (index < 0)
                {
                    throw new InvalidOperationException("Internal error");
                }

                if (output == null)
                {
                    output = new Type[placeholders.Length];
                }
                
                if (output[index] == null)
                {
                    output[index] = instanceType;
                }
                else if (!output[index].Equals(instanceType))
                {
                    // different types for single type-placeholder
                    return false;
                }

                return true;
            }

            return patternType.Equals(instanceType);
        }
    }
}
