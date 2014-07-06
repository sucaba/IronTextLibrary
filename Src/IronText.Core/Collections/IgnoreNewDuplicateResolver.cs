using System;

namespace IronText.Collections
{
    sealed class IgnoreNewDuplicateResolver<T> : IDuplicateResolver<T>
    {
        private static IgnoreNewDuplicateResolver<T> _instance;

        public static IgnoreNewDuplicateResolver<T> Instance
        {
            get 
            {  
                return _instance 
                    ?? (_instance = new IgnoreNewDuplicateResolver<T>());
            }
        }

        public T Resolve(T existing, T newProduction)
        {
            return existing;
        }
    }
}
