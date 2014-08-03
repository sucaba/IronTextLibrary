using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronText.Reflection
{
    public static class ProductionTraits
    {
        public static bool HasEmptyInputTail(this Production prod)
        {
            int len = prod.Input.Length;
            var result = len != 0 && prod.Input[len - 1].HasEmptyProduction();
            return result;
        }

        public static bool HasEmptyInput(Production prod)
        {
            return prod.Input.Length == 0;
        }
    }
}
