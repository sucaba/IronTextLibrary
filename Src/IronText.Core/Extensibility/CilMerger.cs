
namespace IronText.Extensibility
{
    public class CilMerger
    {
        /// <summary>
        /// Symbol being merged
        /// </summary>
        public CilSymbolRef           Symbol { get; set; }

        public CilMergerActionBuilder ActionBuilder { get; set; }
    }
}
