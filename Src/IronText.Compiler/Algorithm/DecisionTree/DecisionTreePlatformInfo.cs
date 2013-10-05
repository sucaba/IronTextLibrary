using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronText.Algorithm
{
    public class DecisionTreePlatformInfo
    {
        public DecisionTreePlatformInfo(
            int    maxLinearCount           = 3,
            double branchCost               = 3,
            double switchCost               = 8,
            int    maxSwitchElementCount    = 1024,
            double minSwitchDensity         = 0.5)
        {
            MaxLinearCount          = maxLinearCount;
            BranchCost              = branchCost;
            SwitchCost              = switchCost;
            MaxSwitchElementCount   = maxSwitchElementCount;
            MinSwitchDensity        = minSwitchDensity;
        }

        public int MaxLinearCount { get; set; }

        public double BranchCost { get; set; }

        public double SwitchCost { get; set; }

        public int MaxSwitchElementCount { get; set; }

        public double MinSwitchDensity { get; set; }
    }
}
