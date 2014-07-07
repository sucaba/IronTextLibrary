using System;

namespace IronText.Collections
{
    static class DuplicateResolver<T>
    {
        private static IDuplicateResolver<T> _fail;
        private static IDuplicateResolver<T> _ignoreNew;

        public static IDuplicateResolver<T> ByName(string name)
        {
            switch (name)
            {
                case "Fail":      return Fail;
                case "IgnoreNew": return IgnoreNew;
                default:
                    var msg = string.Format("Invalid duplicate resolver name '{0}'.", name);
                    throw new ArgumentException(msg, "name");
            }
        }

        public static IDuplicateResolver<T> Fail
        {
            get
            {
                return _fail ?? (_fail = new FailDuplicateResolver());
            }
        }

        public static IDuplicateResolver<T> IgnoreNew
        {
            get 
            {  
                return _ignoreNew ?? (_ignoreNew = new IgnoreNewDuplicateResolver());
            }
        }

        class FailDuplicateResolver : IDuplicateResolver<T>
        {
            public T Resolve(T existing, T newProduction)
            {
                var msg = string.Format("Attempt to add duplicate '{0}' production.", existing);
                throw new InvalidOperationException(msg);
            }
        }

        class IgnoreNewDuplicateResolver : IDuplicateResolver<T>
        {
            public T Resolve(T existing, T newProduction) { return existing; }
        }
    }
}
