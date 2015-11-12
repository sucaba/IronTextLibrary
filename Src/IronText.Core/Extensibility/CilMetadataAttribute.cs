using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using IronText.Logging;
using IronText.Misc;
using IronText.Reflection;
using IronText.Reflection.Managed;
using IronText.Reflection.Reporting;
using IronText.Runtime;

namespace IronText.Extensibility
{
    public abstract class LanguageMetadataAttribute : Attribute, ICilMetadata
    {
        public virtual void Bind(ICilMetadata parent, MemberInfo member)
        {
            this.Parent = parent;
            this.Member = member;
        }

        public virtual bool Validate(ILogging logging) { return true; }

        public ICilMetadata Parent { get; private set; }

        public MemberInfo Member { get; set; }

        public virtual IEnumerable<CilSymbolRef> GetSymbolsInCategory(SymbolCategory category)
        {
            return Enumerable.Empty<CilSymbolRef>();
        }

        public virtual IEnumerable<CilSymbolFeature<Precedence>> GetSymbolPrecedence()
        {
            return Enumerable.Empty<CilSymbolFeature<Precedence>>();
        }

        public virtual IEnumerable<CilSymbolFeature<CilSemanticScope>> GetLocalContextProviders()
        {
            return Enumerable.Empty<CilSymbolFeature<CilSemanticScope>>();
        }

        public virtual IEnumerable<ICilMetadata> GetChildren()
        {
            return Enumerable.Empty<ICilMetadata>();
        }

        public virtual IEnumerable<CilProduction> GetProductions(IEnumerable<CilSymbolRef> leftSides)
        {
            return Enumerable.Empty<CilProduction>();
        }

        public virtual IEnumerable<CilMerger> GetMergers(IEnumerable<CilSymbolRef> leftSides)
        {
            return Enumerable.Empty<CilMerger>();
        }

        public virtual IEnumerable<CilMatcher> GetMatchers()
        {
            return Enumerable.Empty<CilMatcher>();
        }

        public virtual IEnumerable<IReport> GetReports()
        {
            return Enumerable.Empty<IReport>();
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as LanguageMetadataAttribute);
        }

        public bool Equals(LanguageMetadataAttribute other)
        {
            return PropertyComparer<LanguageMetadataAttribute>.Default.Equals(this, other);
        }

        public override int GetHashCode()
        {
            return PropertyComparer<LanguageMetadataAttribute>.Default.GetHashCode(this);
        }

        public override string ToString()
        {
            return string.Format("{{{0} {1}}}", GetType().Name, Member.Name);
        }

        protected Type GetContextType()
        {
            var method = Member as MethodInfo;
            if (!(Member is MethodInfo) && !(Member is PropertyInfo))
            {
                throw new InvalidOperationException("Unable to get context from metadata not bound to method or property.");
            }

            if (Parent == null)
            {
                return Member.DeclaringType;
            }

            var parentType = Parent.Member as Type;
            if (parentType == null)
            {
                return Member.DeclaringType;
            }

            return parentType;
        }
    }
}
