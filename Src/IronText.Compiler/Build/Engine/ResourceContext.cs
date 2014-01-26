using System.Collections.Generic;
using System.Linq;
using IronText.Framework;
using IronText.Logging;

namespace IronText.Build
{
    public class ResourceContext
    {
        public static readonly ResourceContext Instance = new ResourceContext();

        private readonly Stack<IExternalResource> buildStack = new Stack<IExternalResource>();
        private readonly List<IExternalResource> failedResources = new List<IExternalResource>();

        public ILogging Logging { get; set; }

        public ResourceContext()
        {
            this.Logging = new BuildLogging();
        }

        public bool LoadOrBuild(IExternalResource resource)
        {
            if (failedResources.Contains(resource))
            {
                return false;
            }

            if (resource.Exists && IsUpToDate(resource))
            {
                Verbose("Resource '{0}' is up-to-date.", resource);
                if (!resource.Load(Logging))
                {
                    failedResources.Add(resource);
                    Verbose("Failed to load '{0}'.", resource);
                    return false;
                }
            }
            else
            {
                if (!resource.Exists)
                {
                    Verbose("Resource '{0}' does not exist.", resource);
                }

                Verbose("Started building resource '{0}' ...", resource);

                foreach (var source in resource.Sources)
                {
                    if (!LoadOrBuild(source))
                    {
                        return false;
                    }
                }

                buildStack.Push(resource);
                try
                {
                    if (!resource.Rebuild(Logging))
                    {
                        failedResources.Add(resource);
                        Verbose("Failed to build {0}.", resource);
                        return false;
                    }
                }
                finally
                {
                    buildStack.Pop();
                }

                Verbose("Done building resource '{0}'", resource);
            }

            return true;
        }

        public bool LoadOrBuild<T>(IExternalResourceProvider<T> resource, out T output) where T : class
        {
            bool result = LoadOrBuild((IExternalResource)resource);
            if (result)
            {
                output = resource.Resource;
            }
            else
            {
                output = null;
            }

            return result;
        }

        private bool IsUpToDate(IExternalResource resource)
        {
            bool result = resource.Sources.All(src => src.Timestamp <= resource.Timestamp);
            if (!result)
            {
                Verbose("'{0}' is older than source.", resource);
            }
            else
            {
                result = resource.Sources.All(IsUpToDate);
                if (!result)
                {
                    Verbose("Some of '{0}' dependencies are not up-to-date.", resource);
                }
            }

            return result;
        }

        public bool CanLoadOrBuild(IExternalResource resource)
        {
            return !buildStack.Contains(resource)
                && resource.Sources.All(CanLoadOrBuild);
        }

        private void Verbose(string format, params object[] args)
        {
            Logging.Write(
                new LogEntry
                {
                    Severity = Severity.Verbose,
                    Message = string.Format(format, args),
                });
        }

        private void Message(string format, params object[] args)
        {
            Logging.Write(
                new LogEntry
                {
                    Severity = Severity.Message,
                    Message = string.Format(format, args),
                });
        }

        private void Error(string format, params object[] args)
        {
            Logging.Write(
                new LogEntry
                {
                    Severity = Severity.Error,
                    Message = string.Format(format, args),
                });
        }
    }
}
