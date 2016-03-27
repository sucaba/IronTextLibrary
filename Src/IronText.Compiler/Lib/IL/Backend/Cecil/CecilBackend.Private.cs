using System;
using System.Linq;
using System.Collections.Generic;
using IronText.Lib.Shared;
using IronText.Logging;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace IronText.Lib.IL.Backend.Cecil
{
    partial class CecilBackend
    {
        public Document               document;
        public AssemblyDefinition     assembly;
        public ModuleDefinition       module;
        public TypeDefinition         type;
        public MethodDefinition       method;
        public ILProcessor            body;
        private readonly Dictionary<object, Instruction> instructionByLabel = new Dictionary<object, Instruction>();
        private List<Def<Labels>> pendingLabelMarks = new List<Def<Labels>>();

        private Instruction MakeLabel(Ref<Labels> label)
        {
            var result = this.body.Create(OpCodes.Nop);
            result.Operand = label.Def;
            return result;
        }

        private Def<Labels> GetLabel(object labelObject)
        {
            return (Def<Labels>)((Instruction)labelObject).Operand;
        }
        
        private Def<Labels>[] GetLabels(object labelsObject)
        {
            var labelsOperand = (Instruction[])labelsObject;
            return Array.ConvertAll(labelsOperand, GetLabel);
        }

        private bool HasLabelOperand(Instruction instruction)
        {
            return instruction.OpCode.OperandType == OperandType.InlineBrTarget
                || instruction.OpCode.OperandType == OperandType.ShortInlineBrTarget;
        }

        private bool HasMultipleLabelOperand(Instruction instruction)
        {
            return instruction.OpCode.OperandType == OperandType.InlineSwitch;
        }

        private SequencePoint CreateSequencePoint(Loc location)
        {
            var result = new SequencePoint(this.document)
            {
                StartLine   = location.FirstLine   - 1,
                StartColumn = location.FirstColumn - 1,
                EndLine     = location.LastLine    - 1,
                EndColumn   = location.LastColumn  - 1
            };
            return result;
        }

        private TypeReference GetTypeValue(Ref<Types> typeRef) { return (TypeReference)typeRef.Value; }

        private MethodReference GetMethodValue(Ref<Methods> methodRef) { return (MethodReference)methodRef.Value; }

        private ParameterDefinition GetArgValue(Ref<Args> arg) { return (ParameterDefinition)arg.Value; }

        private VariableDefinition GetLocalValue(Ref<Locals> local) { return (VariableDefinition)local.Value; }

        private FieldReference GetFieldValue(FieldSpec fieldSpec)
        {
            var declType  = GetTypeValue(fieldSpec.DeclType);
            var fieldType = module.Import(GetTypeValue(fieldSpec.FieldType));
            var result = new FieldReference(fieldSpec.FieldName, fieldType, declType);
            return result;
        }
    }
}
