using System.Collections.Generic;
using System.Collections.ObjectModel;
using IronText.Lib.NfaVM.Runtime;
using IronText.Lib.RegularAst;
using IronText.Lib.Shared;
using IronText.Lib.Stem;

namespace IronText.Lib.NfaVM.Backend.BytecodeCompiler
{
    public class NfaVMBytecodeBackend : INfaVM
    {
        private const int DefaultProgramCapacity = 200;
        private readonly List<Inst> program;

        public NfaVMBytecodeBackend(AstNode root)
        {
            this.Scanner = new StemScanner();
            this.Labels = new OnDemandNs<Labels>();
            this.program = new List<Inst>(DefaultProgramCapacity);

            new RegularNfaVMCompiler().Compile(root, this);
        }

        public StemScanner Scanner { get; private set; }

        internal ReadOnlyCollection<Inst> Code { get { return new ReadOnlyCollection<Inst>(this.program); } }

        public OnDemandNs<Labels> Labels { get; private set; }

        public void Program(Zom_<INfaVM> entries) { }

        // Update instructions with referencing this label
        public INfaVM Label(Def<Labels> def)
        {
            def.Value = program.Count;
            foreach (var labelRef in Labels.RefsBefore(def))
            {
                int instructionPos = labelRef.Tag;
                Inst inst = program[instructionPos];
                inst.LabelPos = program.Count;
                program[instructionPos] = inst;
            }

            return this;
        }

        public INfaVM Fetch()
        {
            program.Add(new Inst { Op = OpCode.Fetch });
            return this;
        }

        public INfaVM IsA(int expected)
        {
            program.Add(new Inst { Op = OpCode.IsA, IntArg = expected });
            return this;
        }

        public INfaVM Match()
        {
            program.Add(new Inst { Op = OpCode.Match });
            return this;
        }

        public INfaVM Jmp(Ref<Labels> label)
        {
            program.Add(new Inst { Op = OpCode.Jmp, LabelPos = LabelRefToPos(label) });
            return this;
        }

        public INfaVM Fork(Ref<Labels> label)
        {
            program.Add(new Inst { Op = OpCode.Fork, LabelPos = LabelRefToPos(label) });
            return this;
        }

        public INfaVM Save(int slotIndex)
        {
            program.Add(new Inst { Op = OpCode.Save, LabelPos = slotIndex });
            return this;
        }

        public int IntNum(Num num) { return int.Parse(num.Text); }

        private int LabelRefToPos(Ref<Labels> label)
        {
            int result;

            var value = label.Value;
            if (value == null)
            {
                // mark yet unknown value
                result = -1;
                label.Tag = program.Count;
            }
            else
            {
                // get value
                result = (int)value;
            }

            return result;
        }
    }
}
