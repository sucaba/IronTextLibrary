using System;
using System.Collections.Generic;
using System.Reflection;
using IronText.Build.MetadataSyntax;
using IronText.Misc;

namespace IronText.Build
{
    public class TypesMetadataSyntax<TContext> : IMetadataSyntax<TContext>
        where TContext : class
    {
        public IDerivedBuilder<TContext> GetBuilder(Assembly assembly)
        {
            return new CompositeDerivedBuilder<TContext>(GetBuilders(assembly));
        }

        private IDerivedBuilder<TContext>[] GetBuilders(Assembly sourceAssembly)
        {
            var result = new List<IDerivedBuilder<TContext>>();

            foreach (var type in sourceAssembly.GetTypes())
            {
                var attr = Attributes.First<IDerivedBuilderMetadata>(type);
                if (attr != null && attr.IsIncludedInBuild(type))
                {
                    result.Add(CreateBuilder(attr.BuilderType, type));
                }
            }

            return result.ToArray();
        }
        
        private IDerivedBuilder<TContext> CreateBuilder(Type type, Type attributeDefiningType)
        {
            try
            {
                return (IDerivedBuilder<TContext>)Activator.CreateInstance(type, attributeDefiningType);
            }
            catch (Exception)
            {
#if false
                Console.WriteLine("**Unable to create instance of {0} with arg {1}", type, attributeDefiningType);
                Exception error = e;
                while (error.InnerException != null)
                {
                    error = error.InnerException;
                }

                Console.WriteLine("Exception {0}", error);
#endif
                throw;
            }
        }
    }
}
