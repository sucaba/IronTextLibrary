using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using IronText.Lib.Shared;

namespace IronText.Lib.IL.Generators
{
    /// <summary>
    /// + Interface implementation
    /// + Interface base interfaces support
    /// + Root abstract class implementation
    /// + Method can return
    ///     + default(value-type)
    ///     + null reference = default(reference-type)
    ///     + this compatible with returned interface
    ///     + this assignable to returned class 
    ///     + new instance assignable to returned class
    /// </summary>
    public class ImplementationGenerator : GeneratedAssemblyProvider
    {
        private readonly List<PlannedClass>    plan;
        private readonly Func<MethodInfo,bool> forceNonNullResult;

        public string[] PlannedClassNames
        {
            get
            {
                return plan
                    .Select(
                        plannedClass =>
                            plannedClass.ClassName)
                    .ToArray();
            }
        }

        public ImplementationGenerator(
           string assemblyName,
            Func<MethodInfo,bool> forceNonNullResult = null)
            : base(assemblyName)
        {
            this.forceNonNullResult = forceNonNullResult ?? DefaultForceNonNullResult;
            this.plan = new List<PlannedClass>();
        }

        public void PlanImplementationOf(Type type)
        {
            int start = plan.Count;

            AddResultType(null, type);

            for (int i = start; i != plan.Count; ++i)
            {
                ExpandEntry(plan[i]);
            }
        }

        private static bool DefaultForceNonNullResult(MethodInfo m) { return false; }

        protected override CilDocumentSyntax DoGenerate(CilDocumentSyntax docCode)
        {
            return Generate(docCode);
        }

        public CilDocumentSyntax Generate(CilDocumentSyntax docCode)
        {
            // Impl placeholder
            foreach (var plannedClass in plan)
            {
                docCode = GenerateImpl(plannedClass, docCode);
            }

            return docCode;
        }

        private CilDocumentSyntax GenerateImpl(PlannedClass entry, CilDocumentSyntax cil)
        {
            ClassExtendsSyntax wantBaseClass =
                    cil.Class_()
                        .Public
                        .Named(entry.ClassName)
                        ;
        
            var baseClassRef = cil.Types.Import(entry.BaseClass);

            ClassImplementsSyntax wantContract =
                    wantBaseClass
                        .Extends(baseClassRef);

            var baseCtor = entry.BaseClass.GetConstructor(Type.EmptyTypes);
            Ref<Methods> baseCtorRef;

            if (baseCtor != null)
            {
                baseCtorRef = cil.Methods.Import(baseCtor);
            }
            else
            {
                baseCtorRef = null;
            }

            foreach (Type c in entry.Contracts)
            {
                wantContract = wantContract.Implements(cil.Types.Import(c));
            }

            ClassSyntax classCode = wantContract;

            classCode = classCode
                .Method()
                        .Public.Instance
                        .Returning(classCode.Types.Void)
                        .Named(".ctor")
                            .BeginArgs().EndArgs()
                            .BeginBody()
                                .Do(il =>
                                    {
                                        if (baseCtorRef != null)
                                        {
                                            il = il
                                                .Ldarg(0)
                                                .Callvirt(baseCtorRef);
                                        }

                                        return il;
                                    })
                                .Ret()
                            .EndBody()
                            ;

            foreach (var method in entry.Methods)
            {
                if (method.IsGenericMethod)
                {
                    throw new InvalidOperationException("Generic methods are not supported.");
                }

                WantArgsBase wantArgs =
                    classCode.Method()
                        .Private.Hidebysig.Newslot
                        .Virtual.Final.Instance
                        .Returning(classCode.Types.Import(method.ReturnType))
                        .Named(method.DeclaringType + "." + method.Name)
                        .BeginArgs();

                foreach (var parameter in method.GetParameters())
                {
                    wantArgs = wantArgs.Argument(
                                classCode.Types.Import(parameter.ParameterType),
                                wantArgs.Args.Generate(parameter.Name));
                }

                var emit = wantArgs.EndArgs().BeginBody();

                emit = emit.Override(emit.Methods.Import(method));

                emit = EmitFactoryCode(
                            emit,
                            entry,
                            method.ReturnType,
                            !forceNonNullResult(method));

                classCode = emit.Ret().EndBody();
            }

            return classCode.EndClass();
        }

        public EmitSyntax EmitFactoryCode(
            EmitSyntax   emit,
            Type         type)
        {
            if (type.IsAbstract && !plan.Exists(e => e.Implements(type)))
            {
                PlanImplementationOf(type);
            }

            return EmitFactoryCode(emit, null, type, false);
        }

        private EmitSyntax EmitFactoryCode(
            EmitSyntax   emit,
            PlannedClass contextPlannedClass,
            Type         type,
            bool         nullAllowed)
        {
            if (type == typeof(void))
            {
            }
            else if (type == typeof(int))
            {
                emit = emit.Ldc_I4_0();
            }
            else if (type.IsValueType)
            {
                var resultLoc = emit.Locals.Generate("result");

                emit = emit
                    .Local(resultLoc, type)
                    .Ldloca(resultLoc.GetRef())
                    .Initobj(type)
                    .Ldloc(resultLoc.GetRef())
                    ;
            }
            else if (nullAllowed)
            {
                emit.Ldnull();
            }
            else if (!type.IsAbstract && !type.IsInterface)
            {
                emit = emit.Newobj(
                         emit.Types.Import(type));
            }
            else if (contextPlannedClass != null && contextPlannedClass.Implements(type))
            {
                emit = emit.Ldarg(0);
            }
            else if (plan.Exists(e => e.Implements(type)))
            {
                var otherEntry = plan.Find(e => e.Implements(type));
                emit = emit.Newobj(
                         emit.Types.Class_(
                            ClassName.Parse(
                                otherEntry.ClassName)));
            }
            else
            {
                throw new InvalidOperationException(
                    "Internal error: non-planned abstract result type");
            }

            return emit;
        }

        private void AddResultType(PlannedClass contextEntry, Type type)
        {
            if (type.IsClass && type.IsAbstract)
            {
                if (!plan.Exists(e => e.BaseClass == type))
                {
                    plan.Add(
                        new PlannedClass
                        {
                            ClassName = MakeImplName(type),
                            BaseClass = type
                        });
                }
            }
            else if (type.IsInterface)
            {
                if (contextEntry == null)
                {
                    plan.Add(
                        new PlannedClass
                        {
                            ClassName = MakeImplName(type),
                            BaseClass = typeof(object),
                            Contracts = { type }
                        });
                }
                else if (!contextEntry.Contracts.Contains(type))
                {
                    contextEntry.Contracts.Add(type);
                }
            }
        }

        private void ExpandEntry(PlannedClass entry)
        {
            var type = entry.BaseClass;

            const BindingFlags flags = BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance;

            while (type != typeof(object))
            {
                foreach (var method in type.GetMethods(flags))
                {
                    if (!method.IsAbstract)
                    {
                        continue;
                    }

                    entry.Methods.Add(method);

                    if (forceNonNullResult(method))
                    {
                        AddResultType(entry, method.ReturnType);
                    }
                }

                type = type.BaseType;
            }

            for (int i = 0; i != entry.Contracts.Count; ++i)
            {
                var c = entry.Contracts[i];
                entry.Contracts.AddRange(c.GetInterfaces().Except(entry.Contracts));

                foreach (var method in c.GetMethods())
                {
                    if (forceNonNullResult(method))
                    {
                        AddResultType(entry, method.ReturnType);
                    }

                    entry.Methods.Add(method);
                }
            }
        }

        private static string MakeImplName(Type type)
        {
            return type.Name + "_Impl";
        }

        class PlannedClass
        {
            public string                    ClassName;
            public Type                      BaseClass = typeof(object);
            public readonly List<Type>       Contracts = new List<Type>();
            public readonly List<MethodInfo> Methods = new List<MethodInfo>();

            public bool Implements(Type type)
            {
                return Contracts.Contains(type)
                      || type.IsAssignableFrom(BaseClass);
            }
        }
    }
}
