using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using IronText.Framework;

namespace IronText.Build
{
    public abstract class AssemblyProviderBase : IExternalResourceProvider<Assembly>
    {
        public AssemblyProviderBase(AssemblyName assemblyName)
        {
            this.AssemblyName = assemblyName;
        }

        public DateTime Timestamp
        { 
            get { return GetAssemblyCompileDateTime(AssemblyName); }  
        }

        public AssemblyName AssemblyName { get; private set; }

        public virtual ReadOnlyCollection<IExternalResource> Sources
        {
            get { return Array.AsReadOnly(new IExternalResource[0]); }
        }

        public Assembly Resource { get; private set; }

        public bool Exists
        {
            get
            {
                var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                foreach (var assembly in assemblies)
                {
                    if (assembly.FullName == AssemblyName.FullName)
                    {
                        return true;
                    }
                }

                if (!string.IsNullOrEmpty(AssemblyName.CodeBase))
                {
                    string filePath = new Uri(AssemblyName.CodeBase).LocalPath;
                    return File.Exists(filePath);
                }

                return false;
            }
        }

        public bool Rebuild(ILogging logging)
        {
            Assembly resource = null;
            if (!DoRebuild(logging, ref resource))
            {
                return false;
            }

            if (resource == null)
            {
                if (!DoLoad(logging, ref resource))
                {
                    return false;
                }
            }

            this.Resource = resource;
            return true;
        }

        public bool Load(ILogging logging) 
        {
            if (Resource == null)
            {
                Assembly resource = null;
                if (!DoLoad(logging, ref resource))
                {
                    return false;
                }

                this.Resource = resource;
            }

            return true;
        }

        /// <summary>
        /// Rebuild and optinally set resource.
        /// </summary>
        /// <param name="resource"></param>
        protected abstract bool DoRebuild(ILogging logging, ref Assembly resource);

        /// <summary>
        /// Load resource if it was not loaded yet.
        /// </summary>
        /// <param name="resource"></param>
        protected virtual bool DoLoad(ILogging logging, ref Assembly resource)
        {
            try
            {
                resource = Assembly.Load(AssemblyName);
            }
            catch (Exception)
            {
                logging.Write(
                    new LogEntry
                    {
                        Severity = Severity.Error,
                        Message = string.Format("Unable to load assembly '{0}'.", AssemblyName.FullName)
                    });

                return false;
            }

            return true;
        }

        private static DateTime GetAssemblyCompileDateTime(AssemblyName name)
        {
            if (name.CodeBase == null)
            {
                name = AssemblyName.GetAssemblyName(name.Name + ".dll");
            }

            string filePath = name.CodeBase;
            if (filePath.StartsWith(Uri.UriSchemeFile))
            {
                filePath = filePath.Substring(Uri.UriSchemeFile.Length).TrimStart('/', ':');
            }

            return RetrieveLinkerTimestamp(filePath);
        }

        public override string ToString()
        {
            return Path.GetFileName(new Uri(AssemblyName.CodeBase).LocalPath);
        }

        /// <summary>
        /// Retrieves the linker timestamp.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <returns></returns>
        /// <remarks>http://www.codinghorror.com/blog/2005/04/determining-build-date-the-hard-way.html</remarks>
        private static DateTime RetrieveLinkerTimestamp(string filePath)
        {
            const int peHeaderOffset = 60;
            const int linkerTimestampOffset = 8;
            var b = new byte[2048];
            FileStream s = null;
            try
            {
                s = new System.IO.FileStream(filePath, System.IO.FileMode.Open, System.IO.FileAccess.Read);
                s.Read(b, 0, 2048);
            }
            finally
            {
                if(s != null)
                    s.Close();
            }
            var dt = new System.DateTime(1970, 1, 1, 0, 0, 0).AddSeconds(System.BitConverter.ToInt32(b, System.BitConverter.ToInt32(b, peHeaderOffset) + linkerTimestampOffset));
            return dt.AddHours(System.TimeZone.CurrentTimeZone.GetUtcOffset(dt).Hours);
        }
    }
}
