
namespace IronText.Algorithm
{
    public interface IDecisionTreeGenerationStrategy
    {
        /// <summary>
        /// Plan codegeneration for <paramref name="decision"/>
        /// </summary>
        /// <param name="decision"></param>
        void PlanCode(Decision decision);
        
        /// <summary>
        /// Generate code in current position for planned decisions
        /// </summary>
        void GenerateCode();

        /// <summary>
        /// Is used during codegeneration to denote 
        /// placeholder for pending planned decisions.
        /// This placeholder can but is not required 
        /// to be used for such codegeneration.
        /// </summary>
        void IntermediateGenerateCode();

        /// <summary>
        /// Is called in case when code for <paramref name="decision "/> 
        /// needs to be generated in a current position.
        /// </summary>
        /// <param name="decision"></param>
        /// <returns>
        /// <c>true</c> when code was generated or <c>false</c> when code
        /// was already generated or strategy does not allow inlining.
        /// </returns>
        bool TryInlineCode(Decision decision);
    }
}
