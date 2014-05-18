using IronText.Framework;
using IronText.Lib.IL;
using IronText.Lib.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronText.Compilation
{
    /// <summary>
    /// Compile-time stack based on local variables.
    /// </summary>
    class LocalsStack
    {
        private const string SlotLocalPrefix = "slot";

        private readonly Fluent<EmitSyntax> coder;
        private readonly List<Ref<Locals>> slotLocals = new List<Ref<Locals>>();
        private readonly Stack<Ref<Locals>> freeSlotLocals = new Stack<Ref<Locals>>();
        private int currentSlotCount = 0;

        public LocalsStack(Fluent<EmitSyntax> coder)
        {
            this.coder = coder;
        }

        public int Count { get { return slotLocals.Count; } }

        public void Pop(int count)
        {
            int first = slotLocals.Count - count;
            int last  = first + count;

            for (int i = first; i != last; ++i)
            {
                freeSlotLocals.Push(slotLocals[i]);
            }

            slotLocals.RemoveRange(first, count);
        }

        public void Push()
        {
            if (freeSlotLocals.Count == 0)
            {
                // Add one more local variable for storing argument.
                coder(
                    il =>
                    {
                        var l = il.Locals.Generate(SlotLocalPrefix + currentSlotCount);
                        freeSlotLocals.Push(l.GetRef());
                        return il.Local(l, il.Types.Object);
                    });

                ++currentSlotCount;
            }

            var slot = freeSlotLocals.Pop();
            coder(il => il.Stloc(slot));
            slotLocals.Add(slot);
        }

        public void LdSlot(int index)
        {
            coder(il => il.Ldloc(slotLocals[index]));
        }
    }
}
