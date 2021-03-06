﻿using IronText.Framework;

namespace IronText.Reflection.Managed
{
    public class CilMerger
    {
        /// <summary>
        /// Symbol being merged
        /// </summary>
        public CilSymbolRef      Symbol        { get; set; }

        public CilContextRef     Context       { get; set; }

        public Pipe<IActionCode> ActionBuilder { get; set; }
    }
}
