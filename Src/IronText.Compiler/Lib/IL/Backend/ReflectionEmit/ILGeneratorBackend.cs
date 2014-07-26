using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using IronText.Lib.Ctem;
using IronText.Lib.Shared;
using IronText.Logging;

namespace IronText.Lib.IL.Backend.ReflectionEmit
{
    public class ILGeneratorBackend : EmitSyntax
    {
        private readonly ILGenerator il;
        private readonly Dictionary<object, Label> labelByCell;

        public ILGeneratorBackend(ILGenerator ilGenerator)
        {
            this.il = ilGenerator;
            this.Labels = new OnDemandNs<Labels>();
            this.Locals = new DefFirstNs<Locals>();
            this.Args   = new DefFirstNs<Args>();
            this.labelByCell = new Dictionary<object, Label>();
        }

        IEmitSyntaxPluginManager EmitSyntax.Plugins
        {
            get {  throw new NotImplementedException(); }
        }

        public Loc Location { get; set; }

        public DefFirstNs<Locals> Locals { get; private set; }

        public DefFirstNs<Args> Args { get; private set; }

        public OnDemandNs<Labels> Labels { get; set; }

        public IResolutionScopeNs ResolutionScopeNs
        {
            get { throw new NotImplementedException(); }
        }

        public ITypeNs Types
        {
            get { throw new NotImplementedException(); }
        }

        public IMethodNs Methods
        {
            get { throw new NotImplementedException(); }
        }

        public ClassSyntax EndBody() { return null; }

        public EmitSyntax Label(Def<Labels> label)
        {
            this.il.MarkLabel(GetLabelValue(label));
            return this;
        }

        public EmitSyntax Override(Ref<Methods> methodRef)
        {
            throw new NotSupportedException();
        }

        public EmitSyntax Local(Def<Locals> def, Ref<Types> typeName)
        {
            var localBuilder = il.DeclareLocal(GetTypeValue(typeName));
            def.Value = localBuilder;
            return this;
        }

        public EmitSyntax Br_S(Ref<Labels> label)
        {
            il.Emit(OpCodes.Br_S, GetLabelValue(label.Def));
            return this;
        }

        public EmitSyntax Break()
        {
            il.Emit(OpCodes.Break);
            return this;
        }

        public EmitSyntax Brtrue_S(Ref<Labels> label)
        {
            il.Emit(OpCodes.Brtrue_S, GetLabelValue(label));
            return this;
        }

        public EmitSyntax Brtrue(Ref<Labels> label)
        {
            il.Emit(OpCodes.Brtrue, GetLabelValue(label));
            return this;
        }

        public EmitSyntax Brfalse_S(Ref<Labels> label)
        {
            il.Emit(OpCodes.Brfalse_S, GetLabelValue(label));
            return this;
        }

        public EmitSyntax Brfalse(Ref<Labels> label)
        {
            il.Emit(OpCodes.Brfalse, GetLabelValue(label));
            return this;
        }

        public EmitSyntax Br(Ref<Labels> label)
        {
            il.Emit(OpCodes.Br, GetLabelValue(label));
            return this;
        }

        public EmitSyntax Bne_Un(Ref<Labels> label)
        {
            il.Emit(OpCodes.Bne_Un, GetLabelValue(label));
            return this;
        }

        public EmitSyntax Beq(Ref<Labels> label)
        {
            il.Emit(OpCodes.Beq, GetLabelValue(label));
            return this;
        }

        public EmitSyntax Bgt(Ref<Labels> label)
        {
            il.Emit(OpCodes.Bgt, GetLabelValue(label));
            return this;
        }

        public EmitSyntax Blt(Ref<Labels> label)
        {
            il.Emit(OpCodes.Blt, GetLabelValue(label));
            return this;
        }

        public EmitSyntax Bge(Ref<Labels> label)
        {
            il.Emit(OpCodes.Bge, GetLabelValue(label));
            return this;
        }

        public EmitSyntax Ble(Ref<Labels> label)
        {
            il.Emit(OpCodes.Ble, GetLabelValue(label));
            return this;
        }

        public EmitSyntax Jmp(Ref<Methods> method)
        {
            il.Emit(OpCodes.Jmp, GetMethodValue(method));
            return this;
        }

        public EmitSyntax Switch(params Ref<Labels>[] labels)
        {
            il.Emit(OpCodes.Switch, Array.ConvertAll(labels, GetLabelValue));
            return this;
        }

        public EmitSyntax Call(Ref<Methods> method)
        {
            il.Emit(OpCodes.Call, GetMethodValue(method));
            return this;
        }

        public EmitSyntax Callvirt(Ref<Methods> method)
        {
            il.Emit(OpCodes.Callvirt, GetMethodValue(method));
            return this;
        }

        public EmitSyntax EntryPoint()
        {
            throw new NotImplementedException();
        }

        public EmitSyntax Ldarg(Ref<Args> arg)
        {
            il.Emit(OpCodes.Ldarg, GetArgValue(arg));
            return this;
        }

        public EmitSyntax Ldarga(Ref<Args> arg)
        {
            il.Emit(OpCodes.Ldarga, GetArgValue(arg));
            return this;
        }

        public EmitSyntax Ldarg(int argIndex)
        {
            il.Emit(OpCodes.Ldarg, argIndex);
            return this;
        }

        public EmitSyntax Ldarga(int argIndex)
        {
            il.Emit(OpCodes.Ldarga, argIndex);
            return this;
        }

        public EmitSyntax Starg(int argIndex)
        {
            il.Emit(OpCodes.Starg, argIndex);
            return this;
        }

        public EmitSyntax Ldc_I4(int constant)
        {
            il.Emit(OpCodes.Ldc_I4, constant);
            return this;
        }

        public EmitSyntax Ldc_I4_0()
        {
            il.Emit(OpCodes.Ldc_I4_0);
            return this;
        }

        public EmitSyntax Ldc_I4_1()
        {
            il.Emit(OpCodes.Ldc_I4_1);
            return this;
        }

        public EmitSyntax Ldloc(Ref<Locals> local)
        {
            il.Emit(OpCodes.Ldloc, GetLocalValue(local));
            return this;
        }

        public EmitSyntax Ldloca(Ref<Locals> local)
        {
            il.Emit(OpCodes.Ldloca, GetLocalValue(local));
            return this;
        }

        public EmitSyntax Ldfld(FieldInfo fieldInfo)
        {
            il.Emit(OpCodes.Ldfld, fieldInfo);
            return this;
        }

        public EmitSyntax Ldsfld(FieldInfo fieldInfo)
        {
            il.Emit(OpCodes.Ldsfld, fieldInfo);
            return this;
        }

        public EmitSyntax Ldsfld(FieldSpec field)
        {
            throw new InvalidOperationException();
        }

        public EmitSyntax Stfld(FieldInfo fieldInfo)
        {
            il.Emit(OpCodes.Stfld, fieldInfo);
            return this;
        }

        public EmitSyntax Stsfld(FieldInfo fieldInfo)
        {
            il.Emit(OpCodes.Stsfld, fieldInfo);
            return this;
        }

        public EmitSyntax Stsfld(FieldSpec field)
        {
            throw new InvalidOperationException();
        }

        public EmitSyntax Ldloc0()
        {
            il.Emit(OpCodes.Ldloc_0);
            return this;
        }

        public EmitSyntax Ldind_I4()
        {
            il.Emit(OpCodes.Ldind_I4);
            return this;
        }

        public EmitSyntax Ldelem_Ref()
        {
            il.Emit(OpCodes.Ldelem_Ref);
            return this;
        }

        public EmitSyntax Ldelem(Ref<Types> type)
        {
            il.Emit(OpCodes.Ldelem, GetTypeValue(type));
            return this;
        }

        public EmitSyntax Ldelema(Ref<Types> type)
        {
            il.Emit(OpCodes.Ldelema, GetTypeValue(type));
            return this;
        }

        public EmitSyntax Ldelem_I4()
        {
            il.Emit(OpCodes.Ldelem_I4);
            return this;
        }

        public EmitSyntax Ldelem_U2()
        {
            il.Emit(OpCodes.Ldelem_U2);
            return this;
        }

        public EmitSyntax Ldlen()
        {
            il.Emit(OpCodes.Ldlen);
            return this;
        }

        public EmitSyntax Ldnull()
        {
            il.Emit(OpCodes.Ldnull);
            return this;
        }

        public EmitSyntax Ldstr(QStr str)
        {
            il.Emit(OpCodes.Ldstr, str.Text);
            return this;
        }

        public EmitSyntax Throw()
        {
            il.Emit(OpCodes.Throw);
            return this;
        }

        public EmitSyntax Mul()
        {
            il.Emit(OpCodes.Mul);
            return this;
        }

        public EmitSyntax Newarr(TypeSpec itemType)
        {
            il.Emit(OpCodes.Newarr, GetTypeValue(itemType.Type));
            return this;
        }

        public EmitSyntax Newobj(Ref<Methods> constructor)
        {
            il.Emit(OpCodes.Newobj, GetMethodValue(constructor));
            return this;
        }

        public EmitSyntax Initobj(Ref<Types> type)
        {
            il.Emit(OpCodes.Initobj, GetTypeValue(type));
            return this;
        }

        public EmitSyntax Nop()
        {
            il.Emit(OpCodes.Nop);
            return this;
        }

        public EmitSyntax Dup()
        {
            il.Emit(OpCodes.Dup);
            return this;
        }

        public EmitSyntax Pop()
        {
            il.Emit(OpCodes.Pop);
            return this;
        }

        public EmitSyntax Ret()
        {
            il.Emit(OpCodes.Ret);
            return this;
        }

        public EmitSyntax Stelem_Ref()
        {
            il.Emit(OpCodes.Stelem_Ref);
            return this;
        }

        public EmitSyntax Stelem_I4()
        {
            il.Emit(OpCodes.Stelem_I4);
            return this;
        }

        public EmitSyntax Stelem(Ref<Types> type)
        {
            il.Emit(OpCodes.Stelem, GetTypeValue(type));
            return this;
        }

        public EmitSyntax Stind_I4()
        {
            il.Emit(OpCodes.Stind_I4);
            return this;
        }

        public EmitSyntax Stind_Ref()
        {
            il.Emit(OpCodes.Stind_Ref);
            return this;
        }

        public EmitSyntax Stloc(Ref<Locals> id)
        {
            il.Emit(OpCodes.Stloc, GetLocalValue(id));
            return this;
        }

        public EmitSyntax Stloc0()
        {
            il.Emit(OpCodes.Stloc_0);
            return this;
        }

        public EmitSyntax Add()
        {
            il.Emit(OpCodes.Add);
            return this;
        }

        public EmitSyntax Sub()
        {
            il.Emit(OpCodes.Sub);
            return this;
        }

        public EmitSyntax Box(Ref<Types> typeRef)
        {
            il.Emit(OpCodes.Box, GetTypeValue(typeRef));
            return this;
        }

        public EmitSyntax Unbox_Any(Ref<Types> typeRef)
        {
            il.Emit(OpCodes.Unbox_Any, GetTypeValue(typeRef));
            return this;
        }

        public EmitSyntax Ldtoken(FieldInfo token)
        {
            il.Emit(OpCodes.Ldtoken, token);
            return this;
        }

        public EmitSyntax Ldtoken(FieldSpec fieldSpec)
        {
            throw new NotImplementedException();
        }

        public EmitSyntax Ldtoken(Ref<Types> token)
        {
            il.Emit(OpCodes.Ldtoken, GetTypeValue(token));
            return this;
        }

        public EmitSyntax Ldftn(Ref<Methods> method)
        {
            il.Emit(OpCodes.Ldftn, GetMethodValue(method));
            return this;
        }

        private int GetArgValue(Ref<Args> arg) 
        { 
            return (int)(arg.Value == null ? -1 : arg.Value);
        }

        private void SetArgValue(Def<Args> arg, int value)
        {
            arg.Value = value;
        }

        private Label GetLabelValue(Ref<Labels> label) { return GetLabelValue(label.Def); }

        private Label GetLabelValue(Def<Labels> label)
        {
            Label result;

            if (label.Value == null)
            {
                result = il.DefineLabel();
                label.Value = result;
            }
            else
            {
                result = (Label)label.Value;
            }

            return result;
        }

        private static LocalBuilder GetLocalValue(Ref<Locals> local) { return (LocalBuilder)local.Value; }

        private static Type GetTypeValue(Ref<Types> itemType) { return (Type)itemType.Value; }

        private static MethodInfo GetMethodValue(Ref<Methods> methodRef) { return (MethodInfo)methodRef.Value; }

        private static ConstructorInfo GetConstructorValue(Ref<Methods> constructor) { return (ConstructorInfo)constructor.Value; }

    }
}
