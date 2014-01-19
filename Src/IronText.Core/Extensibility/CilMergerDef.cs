
namespace IronText.Extensibility
{
    public class CilMergerDef
    {
        public CilMergerDef()
        {
        }
        
        /// <summary>
        /// Token reference which corresponds to the merged token type.
        /// </summary>
        public CilSymbolRef Token { get; set; }
        public CilMergerActionBuilder ActionBuilder { get; set; }
    }
}
