using System;
using System.Reflection.Emit;
using IronText.Lib.IL.Backend.ReflectionEmit;
using IronText.Lib.Shared;
using IronText.Misc;

namespace IronText.Lib.IL
{
    public class DynamicMethod2<TDelegate> where TDelegate : class
    {
        private Type[] argTypes;
        private string[] argNames;
        private Type resultType;

        public DynamicMethod2(
            string assemblyName,
            Action<EmitSyntax, Ref<Args>[]> codeBuilder)
        {
            ExtractArgumentAndResultTypes(typeof(TDelegate));

            var dynamicMethod = new DynamicMethod(assemblyName + "_Invoke", resultType, argTypes);
            var ilGenerator = dynamicMethod.GetILGenerator(256);
            EmitSyntax emit = new ILGeneratorBackend(ilGenerator);

            var inputArgs = new Ref<Args>[argTypes.Length];
            for (int i = 0; i != inputArgs.Length; ++i)
            {
                var def = emit.Args.Generate(argNames[i]);
                def.Value = i;
                inputArgs[i] = def.GetRef();
            }

            codeBuilder(emit, inputArgs);

            this.Delegate = (TDelegate)(object)dynamicMethod.CreateDelegate(typeof(TDelegate));
        }

        public TDelegate Delegate { get; private set; }

        private void ExtractArgumentAndResultTypes(Type delegateType)
        {
            ReflectionUtils.GetDelegateSignature(delegateType, out resultType, out argTypes, out argNames);
        }
    }

}
