using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronText.Reporting
{
    /// <summary>
    /// Custom reporting action
    /// </summary>
    /// <param name="data">Language data for building various reports</param>
    public delegate void ReportBuilder(IReportData data);
}
