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
    class VarsStack
    {
        private const string SlotLocalPrefix = "slot";

        private readonly Fluent<EmitSyntax> coder;
        private readonly List<Ref<Locals>>  slots     = new List<Ref<Locals>>();
        private readonly Stack<Ref<Locals>> freeSlots = new Stack<Ref<Locals>>();
        private int slotSeed = 0;

        public VarsStack(Fluent<EmitSyntax> coder)
        {
            this.coder = coder;
        }

        public int Count { get { return slots.Count; } }

        public void Pop(int count)
        {
            RemoveRange(Count - count, count);
        }

        /// <summary>
        /// Pop <paramref name="count"/> elements at <paramref name="index"/>.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="count"></param>
        public void RemoveRange(int index, int count)
        {
            int first = index;
            int last  = first + count;

            for (int i = first; i != last; ++i)
            {
                freeSlots.Push(slots[i]);
            }

            slots.RemoveRange(first, count);
        }

        /// <summary>
        /// And push value from .net stack into slot at <paramref name="index"/>.
        /// </summary>
        public void Insert(int index)
        {
            var slot = NewSlot();

            coder(il => il.Stloc(slot));

            slots.Insert(index, slot);
        }

        public void Push()
        {
            Insert(Count);
        }

        public void LdSlot(int index)
        {
            coder(il => il.Ldloc(slots[index]));
        }

        private Ref<Locals> NewSlot()
        {
            if (freeSlots.Count == 0)
            {
                // Add one more local variable for storing argument.
                coder(
                    il =>
                    {
                        var l = il.Locals.Generate(SlotLocalPrefix + slotSeed);
                        freeSlots.Push(l.GetRef());
                        return il.Local(l, il.Types.Object);
                    });

                ++slotSeed;
            }

            return freeSlots.Pop();
        }
    }
}
