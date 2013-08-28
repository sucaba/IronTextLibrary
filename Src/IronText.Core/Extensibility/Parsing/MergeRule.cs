
namespace IronText.Extensibility
{
    public class MergeRule
    {
        public int                TokenId;
        /// <summary>
        /// Token reference which corresponds to the merged token type.
        /// </summary>
        public TokenRef           Token;
        public MergeActionBuilder ActionBuilder;
    }
}
