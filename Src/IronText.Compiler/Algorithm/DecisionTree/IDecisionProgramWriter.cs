
namespace IronText.Algorithm
{
    public interface IDecisionProgramWriter
    {
        void Action(Decision labelNode, int action);

        void Jump(Decision labelNode, Decision destination);
        
        void CondJump(Decision labelNode, RelationalOperator op, int operand, Decision destination);

        void JumpTable(Decision labelNode, int startElement, Decision[] elementToAction);
    }
}
