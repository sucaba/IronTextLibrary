using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using IronText.Framework;
using IronText.Logging;
using IronText.Runtime;

namespace Samples
{
    [Language]
    [GrammarDocument("DynamicLinq.gram")]
    [Precedence(">", 1)]
    [Precedence(".", 10)]
    public class DynamicLinqCompiler
    {
        private readonly Dictionary<string, Expression> scope = new Dictionary<string, Expression>();

        public static DelegateT Compile<DelegateT>(string expr, params object[] args)
        {
            var c = new DynamicLinqCompiler(args);
            using (var interp = new Interpreter<DynamicLinqCompiler>(c))
            {
                interp.LoggingKind = LoggingKind.ThrowOnError;
                interp.Parse(expr);
                return Expression.Lambda<DelegateT>(c.Result, null).Compile();
            }
        }

        public DynamicLinqCompiler(object[] args)
        {
            for (int i = 0; i != args.Length; ++i)
            {
                var expr = (args[i] as Expression);
                if (expr != null)
                { }
                else if (expr is IQueryable)
                {
                    expr = ((IQueryable)expr).Expression;
                }
                else
                {
                    expr = Expression.Constant(args[i]);
                }

                scope["$" + i] = expr;
            }
        }

        [Outcome]
        public Expression Result { get; set; }

        [Produce("from", null, "in", null)]
        public Pipeline From(string variable, Expression source)
        {
            var elementType = GetElementType(source.Type);


            var result = new Pipeline
                            {
                                Variable = variable,
                                QueryableExpr = source,
                                Parameter = Expression.Parameter(elementType, variable)
                            };

            // TODO: Universal search for an extension method container
            if (typeof(IQueryable<>).MakeGenericType(elementType).IsAssignableFrom(source.Type))
            {
                result.MethodContainerType = typeof(Queryable);
            }
            else if (typeof(IEnumerable<>).MakeGenericType(elementType).IsAssignableFrom(source.Type))
            {
                result.MethodContainerType = typeof(Enumerable);
            }
            else
            {
                throw new InvalidOperationException(
                    "Unable to find extension method container for type '" + source.Type.FullName + "'");
            }

            scope[variable] = result.Parameter;
            return result;
        }

        [Produce(null, "where", null)]
        public Pipeline Where(Pipeline pipeline, Expression condExpr)
        {
            var predicateType = typeof(Func<,>).MakeGenericType(pipeline.ElementType, typeof(bool));

            var predicate = Expression.Lambda(
                                predicateType,
                                condExpr,
                                pipeline.Parameter);

            return new Pipeline
            {
                QueryableExpr =
                    Expression.Call(
                        pipeline.MethodContainerType,
                        "Where",
                        new Type[] { pipeline.ElementType },
                        pipeline.QueryableExpr,
                        predicate),
                Variable = pipeline.Variable,
                Parameter = pipeline.Parameter,
                MethodContainerType = pipeline.MethodContainerType
            };
        }

        [Produce(null, "select", null)]
        public Expression FromSelect(Pipeline pipeline, Expression expression)
        {
            var inElementType = pipeline.ElementType;
            var outElementType = expression.Type;
            var selectorDelegateType = typeof(Func<,>).MakeGenericType(inElementType, outElementType);

            var selectorExpr = Expression.Lambda(
                            selectorDelegateType,
                            expression,
                            pipeline.Parameter);

            return
                Expression.Call(
                    pipeline.MethodContainerType,
                    "Select",
                    new Type[] 
                        { 
                            inElementType,
                            outElementType
                        },
                    pipeline.QueryableExpr,
                    selectorExpr);
        }

        [Produce(null, ".", null)]
        public Expression FieldExpression(Expression instance, string fieldName)
        {
            return Expression.Field(instance, fieldName);
        }

        [Produce(null, ">", null)]
        public Expression GreaterThan(Expression x, Expression y)
        {
            return Expression.GreaterThan(x, y);
        }

        [Produce]
        public Expression Constant(int value)
        {
            return Expression.Constant(value, typeof(int));
        }

        [Produce]
        public Expression IdentifierExpression(string name) { return scope[name]; }

        [Match("alpha alnum*")]
        [Match("'$' digit+")]
        public string Identifier(string text) { return text; }

        [Match("digit+")]
        public int Integer(string text)
        {
            return int.Parse(text);
        }

        [Match("blank+")]
        public void Blank() { }

        private static Type GetElementType(Type seqType)
        {
            Type ienum = FindIEnumerable(seqType);
            if (ienum == null) return seqType;
            return ienum.GetGenericArguments()[0];
        }

        private static Type FindIEnumerable(Type seqType)
        {
            if (seqType == null || seqType == typeof(string))
            {
                return null;
            }

            if (seqType.IsArray)
            {
                return typeof(IEnumerable<>).MakeGenericType(seqType.GetElementType());
            }

            if (seqType.IsGenericType)
            {
                foreach (Type arg in seqType.GetGenericArguments())
                {
                    Type ienum = typeof(IEnumerable<>).MakeGenericType(arg);
                    if (ienum.IsAssignableFrom(seqType))
                    {
                        return ienum;
                    }
                }
            }

            Type[] ifaces = seqType.GetInterfaces();
            if (ifaces != null && ifaces.Length > 0)
            {
                foreach (Type iface in ifaces)
                {
                    Type ienum = FindIEnumerable(iface);
                    if (ienum != null) return ienum;
                }
            }

            if (seqType.BaseType != null && seqType.BaseType != typeof(object))
            {
                return FindIEnumerable(seqType.BaseType);
            }

            return null;
        }

        public class Pipeline
        {
            public string Variable;
            public Expression QueryableExpr;
            public ParameterExpression Parameter;
            public Type MethodContainerType;

            public Type ElementType { get { return GetElementType(QueryableExpr.Type); } }
        }
    }
}
