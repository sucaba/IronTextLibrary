using System;

namespace IronText.Collections
{
    sealed class FailDuplicateResolver<T> : IDuplicateResolver<T>
    {
        private static FailDuplicateResolver<T> _instance;

        public static FailDuplicateResolver<T> Instance
        {
            get 
            {  
                return _instance 
                    ?? (_instance = new FailDuplicateResolver<T>());
            }
        }

        public T Resolve(T existing, T newProduction)
        {
            var msg = string.Format("Attempt to add duplicate '{0}' production.", existing);
            throw new InvalidOperationException(msg);
        }
    }
}
