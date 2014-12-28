using System;
using IronText.Lib.Shared;
using IronText.Misc;

namespace IronText.Lib.IL
{
    class CachedMethodAssemblyProvider : GeneratedAssemblyProvider
    {
        private Type delegateType;
        private Type[] argTypes;
        private string[] argNames;
        private Type resultType;
        private readonly string typeName;
        private readonly string methodName;
        private Ref<Args>[] inputArgs;
        private Func<EmitSyntax, Ref<Args>[], EmitSyntax> codeBuilder;

        public CachedMethodAssemblyProvider(
            string assemblyName,
            string typeName,
            string methodName,
            Type delegateType,
            Func<EmitSyntax,Ref<Args>[],EmitSyntax> codeBuilder,
            bool saveFile = false)
            : base(assemblyName, saveFile)
        {
            this.typeName = typeName;
            this.methodName = methodName;
            this.codeBuilder = codeBuilder;
            this.delegateType = delegateType;

            ExtractArgumentAndResultTypes(delegateType);
        }

        private void ExtractArgumentAndResultTypes(Type delegateType)
        {
            ReflectionUtils.GetDelegateSignature(
                delegateType,
                out resultType,
                out argTypes,
                out argNames);
        }

        private WantArgsBase DefineArgs(WantArgsBase wantArgs)
        {
            this.inputArgs = new Ref<Args>[argTypes.Length];

            for (int i = 0; i != inputArgs.Length; ++i)
            {
                var arg = wantArgs.Args.Generate(argNames[i]).GetRef();
                wantArgs = wantArgs.Argument(wantArgs.Types.Import(argTypes[i]), arg.Def);
                inputArgs[i] = arg;
            }

            return wantArgs;
        }

        protected override CilDocumentSyntax DoGenerate(CilDocumentSyntax docCode)
        {
            return docCode
                .Class_().Public.Named(typeName)
                   .Method()
                       .Public.Static
                       .Returning(docCode.Types.Import(resultType))
                       .Named(methodName)
                       .BeginArgs()
                           .Do(DefineArgs)
                       .EndArgs()
                   .BeginBody()
                       .Do(il => codeBuilder(il, inputArgs))
                   .EndBody()
                .EndClass()
                ;
        }
    }
}
