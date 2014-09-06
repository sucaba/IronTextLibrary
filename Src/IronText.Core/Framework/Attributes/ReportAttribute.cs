using IronText.Extensibility;
using IronText.Reflection.Reporting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronText.Framework
{
    [AttributeUsage(AttributeTargets.Class|AttributeTargets.Interface, AllowMultiple=true)]
    public class ReportAttribute : LanguageMetadataAttribute
    {
        private readonly Type type;
        private readonly object[] args;

        public ReportAttribute(Type reportType, params object[] args)
        {
            Validate(reportType);

            this.type = reportType;
            this.args = args;
        }

        public override IEnumerable<IReport> GetReports()
        {
            var result = (IReport)Activator.CreateInstance(type, args);
            return new [] { result };
        }

        public static void Validate(Type type)
        {
            if (!typeof(IReport).IsAssignableFrom(type))
            {
                var msg = string.Format("'{0}' is not  assignable from '{1}'.", typeof(IReport).FullName, type.FullName);
                throw new ArgumentException(msg, "value");
            }
        }
    }
}
