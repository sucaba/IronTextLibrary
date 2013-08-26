using System;
using IronText.Build;
using IronText.Lib.Shared;

namespace IronText.Lib.IL
{
    public class CachedMethod<TDelegate> where TDelegate : class
    {
        public CachedMethod(
            string assemblyName,
            Func<EmitSyntax,Ref<Args>[],EmitSyntax> codeBuilder)
        {
            const string typeName = "CachedMethod";
            const string methodName = "Invoke";

            var assemblyProvider = new CachedMethodAssemblyProvider(
                                        assemblyName,
                                        typeName,
                                        methodName,
                                        typeof(TDelegate),
                                        codeBuilder);
            var delegateProvider = new DelegateProvider<TDelegate>(
                                        assemblyProvider,
                                        typeName,
                                        methodName);

            ResourceContext.Instance.LoadOrBuild(delegateProvider);

            this.Delegate = delegateProvider.Resource;
        }

        public TDelegate Delegate { get; private set; }
    }

}
