using System;
using System.Collections;
using System.Collections.Generic;

namespace IronText.DI
{
    internal class DependencyScope 
        : IDependencyResolver
        , IDisposable
        , IEnumerable<Type>
    {
        private readonly Dictionary<Type, Func<object>> typeToGetter 
            = new Dictionary<Type, Func<object>>();
        private DependencyScope parent;

        public DependencyScope()
            : this(null)
        { }

        public DependencyScope(DependencyScope parent)
        {
            this.parent = parent;
        }

        public bool Has<T>(Func<T,bool> predicate)
            where T : class
        {
            T contract = this.Get<T>();
            return predicate(contract);
        }

        public object Get(Type type)
        {
            var getter = ResolveGetter(type);
            return getter();
        }

        public void Add<TImpl>()
        {
            Add(typeof(TImpl));
        }

        public void Add(Type impl)
        {
            var interfaces = impl.GetInterfaces();
      
            Add(impl, impl);

            foreach (var intf in interfaces)
            {
                Add(intf, impl);
            }
        }

        public void Add<TContract, TImpl>()
        {
            Add(typeof(TContract), typeof(TImpl));
        }

        public void Add(Type contract, Type impl)
        {
            var cs = impl.GetConstructors(); 
            if (cs.Length != 1)
            {
                throw new InvalidOperationException(
                    $"{impl.FullName} should have exactly one public constructor.");
            }

            var c = cs[0];
            var ps = Array.ConvertAll(c.GetParameters(), p => p.ParameterType);

            Add(
                contract,
                () => c.Invoke(Array.ConvertAll(ps, Get)));
        }

        public void Add<T>(T instance)
        {
            if (typeof(T).IsInterface)
            {
                Add(typeof(T), () => instance);
            }
            else
            {
                Register((object)instance);
            }
        }

        public void Register(object instance)
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            var interfaces = instance.GetType().GetInterfaces();
            Add(instance.GetType(), () => instance);
            
            foreach (var intf in interfaces)
            {
                Add(intf, () => instance);
            }
        }

        public void Add<TContract,T1>(Func<T1,TContract> getter)
        {
            Register(typeof(TContract), (Delegate)getter);
        }

        public void Register<TContract,T1,T2>(Func<T1,T2,TContract> getter)
        {
            Register(typeof(TContract), (Delegate)getter);
        }

        public void Register<TContract,T1,T2,T3>(Func<T1,T2,T3,TContract> getter)
        {
            Register(typeof(TContract), (Delegate)getter);
        }
        
        public void Register(Type contract, Delegate parameterizedGetter)
        {
            var invoke = parameterizedGetter.GetType().GetMethod("Invoke");
            var ps = Array.ConvertAll(invoke.GetParameters(), p => p.ParameterType);
            Add(
                contract,
                () => parameterizedGetter.DynamicInvoke(
                        Array.ConvertAll(ps, Get)));
        }

        public DependencyScope Nest()
        {
            return new DependencyScope(this);
        }

        public void Add(Type contract, Func<object> getter)
        {
            if (typeToGetter.ContainsKey(contract))
            {
                throw new InvalidOperationException(
                    $"Dependency {contract.FullName} is already defined");
            }

            if (contract is IHasSideEffects)
            {
                typeToGetter[contract] = getter;
            }
            else
            {
                bool isMemoized = false;
                object memoized = null;
                typeToGetter[contract] = () =>
                {
                    if (!isMemoized)
                    {
                        isMemoized = true;
                        memoized = getter();
                    }

                    return memoized;
                };
            }
        }

        private Func<object> ResolveGetter(Type type)
        {
            Func<object> result;
            if (!typeToGetter.TryGetValue(type, out result)
                && !TryResolveFromParent(type, out result)
                && !AutoAdd(type, out result))
            {
                throw new InvalidOperationException(
                    $"Unable to resolve dependency for type {type.FullName}.");
            }

            return result;
        }

        private bool TryResolveNoAuto(Type type, out Func<object> getter)
        {
            bool result = typeToGetter.TryGetValue(type, out getter)
                        || TryResolveFromParent(type, out getter);
            return result;
        }

        private bool TryResolveFromParent(Type type, out Func<object> getter)
        {
            if (parent == null)
            {
                getter = null;
                return false;
            }

            return parent.TryResolveNoAuto(type, out getter);
        }

        private bool AutoAdd(Type typeToResolve, out Func<object> getter)
        {
            var cs = typeToResolve.GetConstructors(); 
            if (cs.Length != 1)
            {
                getter = null;
                return false;
            }

            Add(typeToResolve, typeToResolve);
            getter = typeToGetter[typeToResolve];
            return true;
        }

        void IDisposable.Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~DependencyScope()
        {
            Dispose(false);
        }

        private void Dispose(bool disposing)
        {
        }

        IEnumerator<Type> IEnumerable<Type>.GetEnumerator()
        {
            return typeToGetter.Keys.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return typeToGetter.Keys.GetEnumerator();
        }
    }
}
