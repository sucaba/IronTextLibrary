
namespace IronText.Framework
{
    interface IUndoable
    {
        void BeginEdit();

        void EndEdit();

        void Undo(int undoCount);
    }
}
