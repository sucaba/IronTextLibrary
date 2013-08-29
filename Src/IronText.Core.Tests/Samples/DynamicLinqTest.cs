using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using IronText.Framework;
using NUnit.Framework;

namespace IronText.Tests.Samples
{
    [TestFixture]
    public class DynamicLinqTest
    {
        [Test]
        public void LinqTest()
        {
            var employees = new Employee[]
            {
                new Employee { Id = 10, FirstName = "John",     LastName = "Doe",     Age = 25 },
                new Employee { Id = 20, FirstName = "Samantha", LastName = "Wells",   Age = 35 },
                new Employee { Id = 30, FirstName = "Michel",   LastName = "Jackson", Age = 45 },
            };

            var query = DynamicLinq.Compile<Func<IQueryable<string>>>(
                            "from e in $0 where e.Age > $1 select e.FirstName",
                            employees.AsQueryable(), //.AsQueryable() is optional in this case
                            30);
            // Should produce:
            //   Samantha
            //   Michel

            foreach (var name in query())
            {
                Debug.WriteLine(name);
            }
        }

        public class Employee
        {
            public int    Id;
            public string FirstName;
            public string LastName;
            public int    Age;
        }

        [Language]
        [GrammarDocument("DynamicLinq.gram")]
        [ScannerDocument("DynamicLinq.scan")]
        [DescribeParserStateMachine("DynamicLinq.info")]
        [Precedence(">", 1)]
        [Precedence(".", 10)]
        public class DynamicLinq
        {
            private readonly Dictionary<string, Expression> scope = new Dictionary<string, Expression>();

            public static DelegateT Compile<DelegateT>(string expr, params object[] args)
            {
                var c = new DynamicLinq(args);
                Language.Parse(c, expr);
                return Expression.Lambda<DelegateT>(c.Result, null).Compile();
            }

            public DynamicLinq(object[] args)
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

            [ParseResult]
            public Expression Result { get; set; } 

            [Parse("from", null, "in", null)]
            public QueryablePipeline From(string variable, Expression source)
            {
                var elementType = GetElementType(source.Type);


                var result = new QueryablePipeline 
                                {
                                    Variable  = variable,
                                    QueryableExpr = source,
                                    Parameter = Expression.Parameter(elementType, variable)
                                };

                // TODO: Unversal search for extension method container
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

            [Parse(null, "where", null)]
            public QueryablePipeline Where(QueryablePipeline pipeline, Expression condExpr)
            {
                var predicateType = typeof(Func<,>).MakeGenericType(pipeline.ElementType, typeof(bool));

                var predicate = Expression.Lambda(
                                    predicateType,
                                    condExpr,
                                    pipeline.Parameter);

                return new QueryablePipeline
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

            [Parse(null, "select", null)]
            public Expression FromSelect(QueryablePipeline pipeline, Expression expression)
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

            [Parse(null, ".", null)]
            public Expression FieldExpression(Expression instance, string fieldName)
            {
                return Expression.Field(instance, fieldName);
            }

            [Parse(null, ">", null)]
            public Expression GreaterThan(Expression x, Expression y)
            {
                return Expression.GreaterThan(x, y);
            }

            [Parse]
            public Expression Constant(int value)
            {
                return Expression.Constant(value, typeof(int));
            }

            [Parse]
            public Expression IdentifierExpression(string name) { return scope[name]; }

            [Scan("alpha alnum*")]
            [Scan("'$' digit+")]
            public string Identifier(string text) { return text; }

            [Scan("digit+")]
            public int Integer(string text)
            {
                return int.Parse(text);
            }

            [Scan("blank+")]
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
                    foreach (Type arg in seqType.GetGenericArguments()) {
                        Type ienum = typeof(IEnumerable<>).MakeGenericType(arg);
                        if (ienum.IsAssignableFrom(seqType)) {
                            return ienum;
                        }
                    }
                }

                Type[] ifaces = seqType.GetInterfaces();
                if (ifaces != null && ifaces.Length > 0)
                {
                    foreach (Type iface in ifaces) {
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

            public class QueryablePipeline
            {
                public string     Variable;
                public Expression QueryableExpr;
                public ParameterExpression Parameter;
                public Type MethodContainerType;

                public Type ElementType { get { return GetElementType(QueryableExpr.Type); } }
            }
        }


        public class SelectClause
        {
            public Expression Expression;
        }
    }


}
