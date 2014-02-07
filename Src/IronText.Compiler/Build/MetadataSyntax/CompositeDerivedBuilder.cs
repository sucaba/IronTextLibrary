using System.Collections.Generic;
using System.Linq;
using IronText.Logging;

namespace IronText.Build.MetadataSyntax
{
    public class CompositeDerivedBuilder<TContext> : IDerivedBuilder<TContext>
        where TContext : class
    {
        private readonly IDerivedBuilder<TContext>[] builders;

        public CompositeDerivedBuilder(IEnumerable<IDerivedBuilder<TContext>> builders)
        {
            this.builders = builders.ToArray();
        }

        public TContext Build(ILogging logging, TContext context)
        {
            foreach (var builder in builders)
            {
                context = builder.Build(logging, context);
                if (context == null)
                {
                    return null;
                }
            }

            return context;
        }
    }
}
