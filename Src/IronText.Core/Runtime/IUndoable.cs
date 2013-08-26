
namespace IronText.Runtime
{
    interface IUndoable
    {
        void BeginEdit();

        void EndEdit();

        void Undo(int undoCount);
    }
}
