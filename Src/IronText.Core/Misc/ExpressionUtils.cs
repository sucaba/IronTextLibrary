using System;
using System.Linq.Expressions;
using System.Reflection;

namespace IronText.Misc
{
    static class ExpressionUtils
    {
        public static FieldInfo GetField<T,R>(Expression<Func<T,R>> expr)
        {
            var memberExpr = expr.Body as MemberExpression;
            if (memberExpr == null)
            {
                throw new ArgumentException("Expected MemberExpression");
            }

            var field = memberExpr.Member as FieldInfo;
            if (field != null)
            {
                return field;
            }

            throw new ArgumentException("Expected MemberExpression with field reference");
        }

        public static MethodInfo GetMethod<T>(Expression<Action<T>> expr)
        {
            return GetMethodFromLambda(expr);
        }

        public static MethodInfo GetMethod<T1,T2>(Expression<Action<T1,T2>> expr)
        {
            return GetMethodFromLambda(expr);
        }

        public static MethodInfo GetMethod<T,R>(Expression<Func<T,R>> expr)
        {
            return GetMethodFromLambda(expr);
        }

        public static MethodInfo GetMethod<T1, T2, R>(Expression<Func<T1, T2, R>> expr)
        {
            return GetMethodFromLambda(expr);
        }

        public static MethodInfo GetMethodFromLambda(LambdaExpression expr)
        {
            var memberExpr = expr.Body as MethodCallExpression;
            if (memberExpr == null)
            {
                throw new ArgumentException("Expected MemberExpression");
            }

            return memberExpr.Method;
        }

        public static ConstructorInfo GetConstructorFromLambda(LambdaExpression expr)
        {
            var memberExpr = expr.Body as NewExpression;
            if (memberExpr == null)
            {
                throw new ArgumentException("Expected MemberExpression");
            }

            return memberExpr.Constructor;
        }
    }
}
