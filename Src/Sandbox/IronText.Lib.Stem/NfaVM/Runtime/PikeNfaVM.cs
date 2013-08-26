using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using IronText.Framework;

namespace IronText.Lib.NfaVM.Runtime
{
    enum OpCode
    {
        Fetch,
        IsA,
        Match,
        Save,
        Jmp = LabelOperandFlag | 1,
        Fork = LabelOperandFlag | 2,
        LabelOperandFlag = 0x10
    }

    [StructLayout(LayoutKind.Explicit)]
    struct Inst
    {
        [FieldOffset(0)]
        public OpCode Op;

        [FieldOffset(sizeof(OpCode))]
        public int    IntArg;

        [FieldOffset(sizeof(OpCode))]
        public int    Id;

        [FieldOffset(sizeof(OpCode))]
        public int    LabelPos;

        [FieldOffset(sizeof(OpCode))]
        public int    Slot;

        public override string ToString()
        {
            return string.Format("{0} {1}", this.Op, this.IntArg);
        }
    }

    public struct Thread
    {
        public int LabelIndex;
        public int[] Slots;
    }

    class PikeNfaVM : IReceiver<int>
    {
        private readonly Inst[] program;
        // threads waiting for the next input
        private Stack<Thread> suspended;
        private List<Thread> matched;
        // Current location + 1. Before any input is equal to 0.
        private int nextLocation;
        private int currentValue;
        private readonly int slotCount;
        // Label object index
        private int[] labelIndexToPos;
        // Keep track of location per label to prevent creation of the already existing threads (avoids backtracking).
        private int[] labelIndexToLocation;

        public PikeNfaVM(Inst[] program)
        {
            this.program = program;
            var maxSlot = program.Where(inst => inst.Op == OpCode.Save).Max(inst => inst.Slot);
            this.slotCount = maxSlot == int.MinValue ? 0 : maxSlot + 1;

            BuildLabelIndex(program);
            suspended = new Stack<Thread>(labelIndexToPos.Length);
            matched = new List<Thread>();

            var startLabelIndex = PosToLabelIndex(0);
            suspended.Push(new Thread { LabelIndex = startLabelIndex, Slots = new int[slotCount] });
            
            this.labelIndexToLocation = new int[labelIndexToPos.Length];
            for (int i = 0; i != labelIndexToLocation.Length; ++i)
            {
                if (i != startLabelIndex)
                {
                    labelIndexToLocation[i] = int.MaxValue;
                }
            }

            Run();
        }

        private void BuildLabelIndex(Inst[] program)
        {
            var labels = new List<int>();
            int nextPos = 0;
            foreach (var inst in program)
            {
                ++nextPos;
                int label;

                if ((inst.Op & OpCode.LabelOperandFlag) == OpCode.LabelOperandFlag)
                {
                    label = inst.LabelPos;
                }
                else if (inst.Op == OpCode.Fetch)
                {
                    label = nextPos;
                }
                else
                {
                    continue;
                }

                if (!labels.Contains(label))
                {
                    labels.Add(label);
                }
            }

            const int ProgramStartPos = 0;
            if (!labels.Contains(ProgramStartPos))
            {
                labels.Add(ProgramStartPos);
            }

            this.labelIndexToPos = labels.ToArray();
        }

        public bool HasMatch { get { return matched.Count != 0; } }

        public bool Failure { get { return matched.Count == 0 && suspended.Count == 0; } }

        public IReceiver<int> Next(int input)
        {
            currentValue = input;

            matched.Clear();
            ++nextLocation;
            Run();

            return this;
        }

        public IReceiver<int> Done() { return this; }

        private static T[] CloneArray<T>(T[] items)
        {
            T[] result = new T[items.Length];
            Array.Copy(items, result, items.Length);
            return result;
        }

        private int PosToLabelIndex(int pos)
        {
            int result = Array.IndexOf(labelIndexToPos, pos);
            if (result < 0)
            {
                throw new ArgumentException("Label '" + pos + "' not found in labelPosToLabelIndex array", "pos");
            }

            return result;
        }

        private int LabelIndexToPos(int labelIndex)
        {
            return labelIndexToPos[labelIndex];
        }

        /// <summary>
        /// Run all suspended threads
        /// </summary>
        private void Run()
        {
            // Unblock or terminate currently blocked threads
            var running = suspended;
            suspended = new Stack<Thread>();

            // Run current threads until running list is empty.
            // Fill blocked threads list to continue with next input.
            while (running.Count != 0)
            {
                var thread = running.Pop();
                
                int pos = LabelIndexToPos(thread.LabelIndex);
                var slots = thread.Slots;

                bool slotsModified = false;
                bool alive = true;
                do
                {
                    var instruction = program[pos];
                    switch (instruction.Op)
                    {
                        default:
                            throw new InvalidOperationException("Unsupported instruction:" + instruction.Op);
                        case OpCode.Fetch:
                            alive = false;
                            int labelIndex = PosToLabelIndex(pos + 1); // label index of the next instruction
                            if (labelIndexToLocation[labelIndex] != nextLocation) // position not in thread list yet
                            {
                                labelIndexToLocation[labelIndex] = nextLocation;
                                suspended.Push(new Thread { LabelIndex = labelIndex, Slots = slots });
                            }

                            break;
                        case OpCode.Jmp:
                            pos = instruction.LabelPos;
                            var labelIndex3 = PosToLabelIndex(pos);
                            Assert(labelIndexToLocation[labelIndex3] != nextLocation);
                            break;
                        case OpCode.IsA:
                            alive = nextLocation != 0 && (instruction.Id == currentValue);
                            ++pos;
                            break;
                        case OpCode.Fork:
                            var labelIndex2 = PosToLabelIndex(instruction.LabelPos);
                            Assert(labelIndexToLocation[labelIndex2] != nextLocation);
                            running.Push(new Thread { LabelIndex = labelIndex2, Slots = slots });
                            ++pos;
                            break;
                        case OpCode.Save:
                            if (!slotsModified)
                            {
                                slotsModified = true;
                                slots = CloneArray(slots);
                            }

                            slots[instruction.Slot] = nextLocation - 1;
                            ++pos;
                            break;
                        case OpCode.Match:
                            matched.Add(thread);
                            alive = false;
                            break;
                    }
                }
                while (alive);
            }

            running.Clear();
        }

        [Conditional("DEBUG")]
        private void Assert(bool condition)
        {
            if (!condition)
            {
                throw new InvalidOperationException();
            }
        }
    }
}
