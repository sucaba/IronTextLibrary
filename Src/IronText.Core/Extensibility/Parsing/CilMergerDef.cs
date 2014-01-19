
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
        public TokenRef Token { get; set; }
        public MergeActionBuilder ActionBuilder { get; set; }
    }
}
