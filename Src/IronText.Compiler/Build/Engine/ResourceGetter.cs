using System;
using System.Collections.ObjectModel;
using IronText.Logging;

namespace IronText.Build
{
    public delegate bool BuildDelegate<T>(ILogging logging, out T output);

    public class ResourceGetter<T> 
        : IExternalResourceProvider<T>
        where T : class
    {
        private bool isLoaded;
        private T resource;

        protected ResourceGetter() { }

        public ResourceGetter(BuildDelegate<T> getter)
        {
            this.Getter = getter;
        }

        protected BuildDelegate<T> Getter;

        public T Resource { get { return resource; } }

        public bool Exists { get { return isLoaded; } }

        public DateTime Timestamp { get { return DateTime.MinValue; } }

        public ReadOnlyCollection<IExternalResource> Sources
        {
            get { return new ReadOnlyCollection<IExternalResource>(new IExternalResource[0]); }
        }

        public bool Rebuild(ILogging logging) { return Load(logging); }

        public bool Load(ILogging logging)
        {
            if (!isLoaded)
            {
                isLoaded = true;
                return Getter(logging, out resource);
            }

            return true;
        }
    }
}
