using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using IronText.Framework;
using IronText.Lib.Ctem;
using IronText.Lib.Shared;
using IronText.Logging;
using Mono.Cecil;
using Mono.Cecil.Cil;
using IronText.Runtime;

namespace IronText.Lib.IL.Backend.Cecil
{
    partial class CecilBackend 
        : CilSyntax
        , ICilDocumentInfo
        , CilDocumentSyntax
        , AssemblyInfoSyntax
        , AssemblyRefSyntax
        , ClassSyntax
        , ClassAttrSyntax
        , ClassIdSyntax
        , ClassExtendsSyntax
        , ClassImplementsSyntax
        , WantMethAttr
        , WantCallConv
        , WantCallKind
        , PInvAttrSyntax
        , WantReturnType
        , WantName
        , WantImplAttr
        , WantOpenArgs
        , WantArgs
        , WantField
        , WantFieldAttr
        , WantFieldName
        , WantFieldAt
        , WantFieldInit
        , ParamAttrSyntax1<WantArgs>
        , ParamAttrSyntax1<WantMoreArgs>
        , WantMoreArgs
        , WantMethodBody
        , EmitSyntax
        , IEmitSyntaxPluginManager
        , IAssemblyReader
        , IAssemblyWriter
        , IAssemblyResolverParameters
    {
        private DefaultAssemblyResolver assemblyResolver;
        private ParameterAttributes currentParamAttr;
        private TypeAttributes currentClassAttributes;
        private AssemblyNameReference currentAssemblyRef;
        private FieldDefinition field;
        private readonly Dictionary<Type, IEmitSyntaxPlugin> emitSyntaxPlugins = new Dictionary<Type, IEmitSyntaxPlugin>();

        public static CilSyntax Create(string filePath)
        {
            var result = new CecilBackend();
            result.document = new Document(filePath);
            return result;
        }

        public CecilBackend()
        {
            this.document = new Document("none");

            this.Scanner = new CtemScanner();
            this.Locals  = new DefFirstNs<Locals>();
            this.Labels  = new OnDemandNs<Labels>();
            this.Args    = new DefFirstNs<Args>();

            this.assemblyResolver = new DefaultAssemblyResolver();
       }

        IEmitSyntaxPluginManager EmitSyntax.Plugins { get { return this; } }

        bool IEmitSyntaxPluginManager.TryGetPlugin(Type pluginType, out IEmitSyntaxPlugin plugin)
        {
            return emitSyntaxPlugins.TryGetValue(pluginType, out plugin);
        }

        void IEmitSyntaxPluginManager.Add(Type contract, IEmitSyntaxPlugin plugin)
        {
            emitSyntaxPlugins.Add(contract, plugin);
        }


        public CilSyntax Result { get; set; }

        public CtemScanner Scanner { get; private set; }

        public ILogging Logging { get; set; }

        public IParsing Parsing { get; set; }

        public Guid Mvid { get {  return module.Mvid; } }

        public OnDemandNs<Labels> Labels { get; set; }

        public DefFirstNs<Locals> Locals { get; private set; }

        public DefFirstNs<Args> Args { get; private set; }

        public IResolutionScopeNs ResolutionScopeNs { get; private set; }

        public ITypeNs Types { get; private set; }

        public IMethodNs Methods { get; private set; }

        public ClassSyntax Size(int value)
        {
            this.type.ClassSize = value;
            return this;
        }

        public ClassSyntax Pack(int value)
        {
            this.type.PackingSize = (short)value;
            return this;
        }

        public CilDocumentSyntax BeginDocument() { return this; }

        public AssemblyInfoSyntax AssemblyRewrite(string fromFilePath)
        {
            (this as IAssemblyReader).Read(fromFilePath);

            var assemblyId = Path.GetFileNameWithoutExtension(fromFilePath);

            var assemblyNameDef = new AssemblyNameDefinition(
                assemblyId,
                new Version(1, 0, 0, 0));

            var result = AssemblyInit(assemblyNameDef, addCorLib:false);
            module.ScopeResolver = new RewriteScopeResolver();
            return result;
        }

        public AssemblyRefSyntax AssemblyExtern(Def<ResolutionScopes> assemblyScope)
        {
            this.currentAssemblyRef = assemblyScope.Value as AssemblyNameReference;
            return this;
        }

        public AssemblyInfoSyntax Assembly(string name)
        {
            var assemblyId = name;

            var assemblyNameDef = new AssemblyNameDefinition(
                assemblyId,
                new Version(1, 0, 0, 0));

            assembly = AssemblyDefinition.CreateAssembly(
                        assemblyNameDef,
                        assemblyNameDef.Name,
                        new ModuleParameters 
                        {
                            Kind = ModuleKind.Console,
                            AssemblyResolver = assemblyResolver
                        });

            return AssemblyInit(assemblyNameDef, addCorLib:false);
        }

        public CilDocumentSyntax EndAssembly() 
        { 
            return this;
        }

        private AssemblyInfoSyntax AssemblyInit(AssemblyNameDefinition assemblyNameDef, bool addCorLib)
        {
            this.assembly.Name = assemblyNameDef;
            this.assembly.MainModule.Name = assemblyNameDef.Name;

            this.module = assembly.MainModule;

            this.ResolutionScopeNs = new CecilResolutionScopeNs(module);
            this.Types = new CecilTypeNs(module);
            this.Methods = new CecilMethodNs(module);

            return this;
        }

        public void EndDocument()
        {
            CilDocumentSyntax syntax = this;
            foreach (var plugin in emitSyntaxPlugins.Values)
            {
                syntax = plugin.BeforeEndDocument(syntax);
            }

            this.type = null;
            this.method = null;
            this.body = null;
            this.instructionByLabel.Clear();
            this.pendingLabelMarks.Clear();
        }

        public CilDocumentSyntax Module(QStr moduleName)
        {
            if (moduleName != null)
            {
                this.module.Name = moduleName.Text;
            }

            return this; 
        }

        AssemblyInfoSyntax CustomAttributesSyntax<AssemblyInfoSyntax>.CustomAttribute(Ref<Types> customType)
        {
            TypeReference type = GetTypeValue(customType);
            TypeDefinition typeDef = type.Resolve();
            MethodReference defaultConstructor = typeDef.Methods.Where(m => m.IsConstructor && m.Parameters.Count == 0).First();
            defaultConstructor = this.module.Import(defaultConstructor);
            assembly.CustomAttributes.Add(new CustomAttribute(defaultConstructor));
            return this;
        }

        public ClassAttrSyntax Class_() { return this; }

        ClassExtendsSyntax ClassIdSyntax.Named(string className)
        {
            string @namespace, name;
            SignatureUtils.SplitFullName(className, out @namespace, out name);

            this.type = module.MetadataResolver.Resolve(new TypeReference(@namespace, name, module, module))
                      ?? new TypeDefinition(
                                @namespace,
                                name,
                                currentClassAttributes,
                                module.TypeSystem.Object);

            this.currentClassAttributes = default(TypeAttributes);

            this.module.Types.Add(this.type);

            return this;
        }

        public CilDocumentSyntax EndClass() { return this; }

        ClassImplementsSyntax ClassExtendsSyntax.Extends(Ref<Types> baseClass)
        {
            this.type.BaseType = GetTypeValue(baseClass);
            return this;
        }

        ClassImplementsSyntax ClassImplementsSyntax.Implements(Ref<Types> @interface)
        {
            this.type.Interfaces.Add(GetTypeValue(@interface));
            return this;
        }

        internal TypeDefinition GetTypeData(int size)
        {
            string strTypeName = "$ArrayType$" + size;
            TypeDefinition type = null;
            foreach (TypeDefinition td in module.Types)
                if (td.Name == strTypeName)
                {
                    type = td;
                    break;
                }
 
            if (type == null)
            {
                TypeAttributes attr = TypeAttributes.Sealed | TypeAttributes.ExplicitLayout | TypeAttributes.Public;
                module.Types.Add(type = new TypeDefinition(null, strTypeName, attr, module.Import(typeof(System.ValueType))));
                type.PackingSize = 1;
                type.ClassSize = size;
            }

            return type;
        }

        WantMethAttr ClassSyntax.Method() 
        {
            this.method = new MethodDefinition(
                            "Undefined",
                            MethodAttributes.CompilerControlled,
                            GetTypeValue(this.Types.Void));
            this.method.HasThis = false;
            this.method.ExplicitThis = false;

            type.Methods.Add(this.method);
            return this;
        }

        WantName WantReturnTypeThen<WantName>.Returning(Ref<Types> resultTypeRef)
        {
            this.method.ReturnType = GetTypeValue(resultTypeRef);
            return this; 
        }
            
        WantOpenArgs WantNameThen<WantOpenArgs>.Named(string methodName)
        {
            this.method.Name = methodName;

            if (methodName == ".ctor" || methodName == ".cctor")
            {
                method.IsRuntimeSpecialName = true;
                method.IsSpecialName = true;
            }

            if (methodName == ".ctor")
            {
                method.HasThis = true;
            }
            else if (methodName == ".cctor")
            {
                method.HasThis = false;
            }

            this.body = this.method.Body.GetILProcessor();

            this.Locals.Frame = new ListFrame<Locals,VariableDefinition> { Items = method.Body.Variables };

            Args.PushFrame();

            return this;
        }

        public ClassSyntax EndBody()
        { 
            Args.PopFrame();

            if (pendingLabelMarks.Count != 0)
            {
                throw new InvalidOperationException("One ore more labels placed after the last instruction.");
            }


            foreach (var instruction in this.method.Body.Instructions)
            {
                if (HasLabelOperand(instruction))
                {
                    var label = GetLabel(instruction.Operand);
                    instruction.Operand = this.instructionByLabel[label];
                }
                else if (HasMultipleLabelOperand(instruction))
                {
                    var branches = GetLabels(instruction.Operand);
                    var labelInstructions = new List<Instruction>();
                    foreach (Def<Labels> branch in branches)
                    {
                         labelInstructions.Add(this.instructionByLabel[branch]);
                    }

                    instruction.Operand = labelInstructions.ToArray();
                }
            }

            return this;
        }

        public WantImplAttr EndArgs() 
        {
            return this; 
        }

        WantMoreArgs ParamAttrSyntax1<WantMoreArgs>.In
        {
            get
            {
                this.currentParamAttr |= ParameterAttributes.In;
                return this;
            }
        }

        WantMoreArgs ParamAttrSyntax1<WantMoreArgs>.Out
        {
            get
            {
                this.currentParamAttr |= ParameterAttributes.Out;
                return this;
            }
        }

        WantMoreArgs ParamAttrSyntax1<WantMoreArgs>.Opt
        {
            get
            {
                this.currentParamAttr |= ParameterAttributes.Optional;
                return this;
            }
        }

        WantArgs ParamAttrSyntax1<WantArgs>.In
        {
            get
            {
                this.currentParamAttr |= ParameterAttributes.In;
                return this;
            }
        }

        WantArgs ParamAttrSyntax1<WantArgs>.Out
        {
            get
            {
                this.currentParamAttr |= ParameterAttributes.Out;
                return this;
            }
        }

        WantArgs ParamAttrSyntax1<WantArgs>.Opt
        {
            get
            {
                this.currentParamAttr |= ParameterAttributes.Optional;
                return this;
            }
        }

        WantArgs WantOpenArgs.BeginArgs()
        {
            return this;
        }

        public WantMoreArgs Argument(Ref<Types> typeRef, Def<Args> arg)
        {
            string name;
            if (arg.Name == null)
            {
                name = "undefined";
            }
            else
            {
                name = arg.Name;
            }

            var argument = new ParameterDefinition(name, currentParamAttr, GetTypeValue(typeRef));
            currentParamAttr = ParameterAttributes.None;

            this.method.Parameters.Add(argument);
            arg.Value = argument;

            return this;
        }

        #region Instructions

        EmitSyntax WantMethodBody.BeginBody()
        {
            return this;
        }

        public EmitSyntax Label(Def<Labels> label)
        {
            this.pendingLabelMarks.Add(label);
            return this;
        }

        public EmitSyntax Override(Ref<Methods> methodRef)
        {
            this.method.Overrides.Add(GetMethodValue(methodRef));
            return this;
        }

        public EmitSyntax Local(Def<Locals> def, Ref<Types> typeName)
        {
            if (def.Name != null)
            {
                def.Value = new VariableDefinition(def.Name, GetTypeValue(typeName));
            }
            else
            {
                def.Value = new VariableDefinition(GetTypeValue(typeName));
            }

            return this;
        }

        public EmitSyntax Stloc(Ref<Locals> local)
        {
            Emit(OpCodes.Stloc, local);
            return this;
        }

        public EmitSyntax Stloc0()
        {
            Emit(OpCodes.Stloc_0);
            return this;
        }

        public EmitSyntax Ldloc(Ref<Locals> local)
        {
            Emit(OpCodes.Ldloc, local);
            return this;
        }

        public EmitSyntax Ldloca(Ref<Locals> local)
        {
            Emit(OpCodes.Ldloca, local);
            return this;
        }

        public EmitSyntax Ldfld(System.Reflection.FieldInfo fieldInfo)
        {
            Emit(OpCodes.Ldfld, module.Import(fieldInfo));
            return this;
        }

        public EmitSyntax Ldsfld(FieldSpec fieldSpec)
        {
            FieldReference field = GetFieldValue(fieldSpec);
            Emit(OpCodes.Ldsfld, field);
            return this;
        }

        public EmitSyntax Ldsfld(System.Reflection.FieldInfo fieldInfo)
        {
            Emit(OpCodes.Ldsfld, module.Import(fieldInfo));
            return this;
        }

        public EmitSyntax Stfld(System.Reflection.FieldInfo fieldInfo)
        {
            Emit(OpCodes.Stfld, module.Import(fieldInfo));
            return this;
        }

        public EmitSyntax Stsfld(System.Reflection.FieldInfo fieldInfo)
        {
            Emit(OpCodes.Stsfld, module.Import(fieldInfo));
            return this;
        }

        public EmitSyntax Stsfld(FieldSpec fieldSpec)
        {
            FieldReference field = GetFieldValue(fieldSpec);
            Emit(OpCodes.Stsfld, field);
            return this;
        }

        public EmitSyntax Ldloc0()
        {
            Emit(OpCodes.Ldloc_0);
            return this;
        }

        public EmitSyntax Ldind_I4()
        {
            Emit(OpCodes.Ldind_I4);
            return this;
        }

        public EmitSyntax Ldelem(Ref<Types> type)
        {
            Emit(OpCodes.Ldelem_Any, type);
            return this;
        }

        public EmitSyntax Ldelema(Ref<Types> type)
        {
            Emit(OpCodes.Ldelema, type);
            return this;
        }

        public EmitSyntax Ldelem_Ref()
        {
            Emit(OpCodes.Ldelem_Ref);
            return this;
        }

        public EmitSyntax Ldelem_I4()
        {
            Emit(OpCodes.Ldelem_I4);
            return this;
        }

        public EmitSyntax Ldelem_U2()
        {
            Emit(OpCodes.Ldelem_U2);
            return this;
        }

        public EmitSyntax Ldstr(QStr str)
        {
            Emit(OpCodes.Ldstr, str.Text);
            return this;
        }

        public EmitSyntax Throw()
        {
            Emit(OpCodes.Throw);
            return this;
        }

        public EmitSyntax Ldlen()
        {
            Emit(OpCodes.Ldlen);
            return this;
        }

        public EmitSyntax Ldnull()
        {
            Emit(OpCodes.Ldnull);
            return this;
        }

        public EmitSyntax Ldarg(Ref<Args> arg)
        {
            var value = GetArgValue(arg);
            Emit(OpCodes.Ldarg, value.Index);
            return this;
        }

        public EmitSyntax Ldarga(Ref<Args> arg)
        {
            var value = GetArgValue(arg);
            Emit(OpCodes.Ldarga, value.Index);
            return this;
        }

        public EmitSyntax Ldarg(int argIndex)
        {
            Emit(OpCodes.Ldarg, argIndex);
            return this;
        }

        public EmitSyntax Ldarga(int argIndex)
        {
            Emit(OpCodes.Ldarga, argIndex);
            return this;
        }

        public EmitSyntax Starg(int argIndex)
        {
            Emit(OpCodes.Starg, argIndex);
            return this;
        }

        public EmitSyntax Ldc_I4(int constant)
        {
            Emit(OpCodes.Ldc_I4, constant);
            return this;
        }

        public EmitSyntax Ldc_I4_0()
        {
            Emit(OpCodes.Ldc_I4_0);
            return this;
        }

        public EmitSyntax Ldc_I4_1()
        {
            Emit(OpCodes.Ldc_I4_1);
            return this;
        }

        public EmitSyntax Call(Ref<Methods> method)
        {
            Emit(OpCodes.Call, method);
            return this;
        }

        public EmitSyntax Callvirt(Ref<Methods> method)
        {
            Emit(OpCodes.Callvirt, method);
            return this;
        }

        public EmitSyntax Brtrue_S(Ref<Labels> label)
        {
            Emit(OpCodes.Brtrue_S, label);
            return this;
        }

        public EmitSyntax Brtrue(Ref<Labels> label)
        {
            Emit(OpCodes.Brtrue, label);
            return this;
        }

        public EmitSyntax Brfalse_S(Ref<Labels> label)
        {
            Emit(OpCodes.Brfalse_S, label);
            return this;
        }

        public EmitSyntax Brfalse(Ref<Labels> label)
        {
            Emit(OpCodes.Brfalse, label);
            return this;
        }

        public EmitSyntax Br(Ref<Labels> label)
        {
            Emit(OpCodes.Br, label);
            return this;
        }

        public EmitSyntax Br_S(Ref<Labels> label)
        {
            Emit(OpCodes.Br_S, label);
            return this;
        }

        public EmitSyntax Jmp(Ref<Methods> method)
        {
            Emit(OpCodes.Jmp, method);
            return this;
        }

        public EmitSyntax Bne_Un(Ref<Labels> label)
        {
            Emit(OpCodes.Bne_Un, label);
            return this;
        }

        public EmitSyntax Beq(Ref<Labels> label)
        {
            Emit(OpCodes.Beq, label);
            return this;
        }

        public EmitSyntax Bgt(Ref<Labels> label)
        {
            Emit(OpCodes.Bgt, label);
            return this;
        }

        public EmitSyntax Blt(Ref<Labels> label)
        {
            Emit(OpCodes.Blt, label);
            return this;
        }

        public EmitSyntax Bge(Ref<Labels> label)
        {
            Emit(OpCodes.Bge, label);
            return this;
        }

        public EmitSyntax Ble(Ref<Labels> label)
        {
            Emit(OpCodes.Ble, label);
            return this;
        }

        public EmitSyntax Switch(params Ref<Labels>[] labels)
        {
            Emit(OpCodes.Switch, labels);
            return this;
        }

        public EmitSyntax Add()
        {
            Emit(OpCodes.Add);
            return this;
        }

        public EmitSyntax Sub()
        {
            Emit(OpCodes.Sub);
            return this;
        }

        public EmitSyntax Mul()
        {
            Emit(OpCodes.Mul);
            return this;
        }

        public EmitSyntax Break()
        {
            Emit(OpCodes.Break);
            return this;
        }

        public EmitSyntax Nop()
        {
            Emit(OpCodes.Nop);
            return this;
        }

        public EmitSyntax Dup()
        {
            Emit(OpCodes.Dup);
            return this;
        }

        public EmitSyntax Ret()
        {
            Emit(OpCodes.Ret);
            return this;
        }

        public EmitSyntax Newobj(Ref<Methods> constructor)
        {
            Emit(OpCodes.Newobj, constructor);
            return this;
        }

        public EmitSyntax Initobj(Ref<Types> value)
        {
            Emit(OpCodes.Initobj, value);
            return this;
        }

        public EmitSyntax Newarr(TypeSpec itemType)
        {
            Emit(OpCodes.Newarr, itemType.Type);
            return this;
        }

        public EmitSyntax Stelem_Ref()
        {
            Emit(OpCodes.Stelem_Ref);
            return this;
        }

        public EmitSyntax Stelem_I4()
        {
            Emit(OpCodes.Stelem_I4);
            return this;
        }

        public EmitSyntax Stelem(Ref<Types> type)
        {
            Emit(OpCodes.Stelem_Any, type);
            return this;
        }

        public EmitSyntax Stind_I4()
        {
            Emit(OpCodes.Stind_I4);
            return this;
        }

        public EmitSyntax Stind_Ref()
        {
            Emit(OpCodes.Stind_Ref);
            return this;
        }

        public EmitSyntax Pop()
        {
            Emit(OpCodes.Pop);
            return this;
        }

        public EmitSyntax Box(Ref<Types> typeRef)
        {
            Emit(OpCodes.Box, typeRef);
            return this;
        }

        public EmitSyntax Unbox_Any(Ref<Types> typeRef)
        {
            Emit(OpCodes.Unbox_Any, typeRef);
            return this;
        }

        public EmitSyntax EntryPoint()
        {
            this.module.EntryPoint = this.method;
            return this;
        }

        public EmitSyntax Ldtoken(System.Reflection.FieldInfo token)
        {
            Emit(OpCodes.Ldtoken, module.Import(token));
            return this;
        }

        public EmitSyntax Ldtoken(FieldSpec fieldSpec)
        {
            Emit(OpCodes.Ldtoken, GetFieldValue(fieldSpec));
            return this;
        }

        public EmitSyntax Ldtoken(Ref<Types> token)
        {
            Emit(OpCodes.Ldtoken, GetTypeValue(token));
            return this;
        }

        public EmitSyntax Ldftn(Ref<Methods> method)
        {
            Emit(OpCodes.Ldftn, method);
            return this;
        }

        #endregion Instructions

        private void Emit(OpCode opCode)
        {
            Labelize(body.Create(opCode));
        }

        private void Emit(OpCode opCode, string text)
        {
            if (text == null)
            {
                Ldnull();
                return;
            }

            Labelize(body.Create(opCode, text));
        }

        private void Emit(OpCode opCode, Ref<Types> typeRef)
        {
            Labelize(body.Create(opCode, GetTypeValue(typeRef)));
        }

        private void Emit(OpCode opCode, Ref<Methods> method)
        {
            Labelize(body.Create(opCode, GetMethodValue(method)));
        }

        private void Emit(OpCode opCode, FieldReference fieldReference)
        {
            Labelize(body.Create(opCode, fieldReference));
        }

        private void Emit(OpCode opCode, TypeReference typeReference)
        {
            Labelize(body.Create(opCode, typeReference));
        }

        private void Emit(OpCode opCode, Ref<Labels> label)
        {
            Labelize(body.Create(opCode, MakeLabel(label)));
        }

        private void Emit(OpCode opCode, Ref<Locals> local)
        {
            Labelize(body.Create(opCode, GetLocalValue(local)));
        }

        private void Emit(OpCode opCode, int constant)
        {
            Labelize(body.Create(opCode, constant));
        }

        private void Emit(OpCode opCode, Ref<Labels>[] labels)
        {
            Labelize(body.Create(opCode, Array.ConvertAll(labels, MakeLabel)));
        }

        private void Labelize(Instruction instruction)
        {
            instruction.SequencePoint = this.CreateSequencePoint(
                Parsing != null ? Parsing.HLocation : HLoc.Unknown);

            foreach (var label in pendingLabelMarks)
            {
                this.instructionByLabel[label] = instruction;
            }

            pendingLabelMarks.Clear();

            body.Append(instruction);
        }

        void IAssemblyReader.Read(string path)
        {
            this.assembly = AssemblyDefinition.ReadAssembly(path);
        }

        void IAssemblyWriter.Write(string path)
        {
            WriteAssembly(path, null);
        }

        void IAssemblyWriter.Write(Stream stream)
        {
            WriteAssembly(null, stream);
        }

        private void WriteAssembly(string path, Stream stream)
        {
            if (stream == null)
            {
                string filePath = path;
                string ext = Path.GetExtension(filePath);

                // Add extension when there is no valid one
                if (ext != ".dll" && ext != ".exe")
                {
                    ext = this.module.EntryPoint == null ? ".dll" : ".exe";
                    filePath += ext;
                }

                this.assembly.Write(
                    filePath/*,
                    new WriterParameters 
                    { 
                        WriteSymbols = this.document != null 
                                    && !string.IsNullOrEmpty(this.document.Url) 
                    }*/);
            }
            else
            {
                this.assembly.Write(stream);
            }

            this.module = null;
            this.assembly = null;
        }

        void IAssemblyResolverParameters.AddSearchDirectory(string directory)
        {
            this.assemblyResolver.AddSearchDirectory(directory);
        }

        ClassAttrSyntax ClassAttrSyntax.Public
        {
            get 
            { 
                currentClassAttributes |= TypeAttributes.Public;
                return this; 
            }
        }

        ClassAttrSyntax ClassAttrSyntax.Private
        {
            get 
            { 
                currentClassAttributes |= TypeAttributes.NotPublic;
                return this; 
            }
        }

        ClassAttrSyntax ClassAttrSyntax.Value
        {
            get 
            {
                throw new InvalidOperationException("Extend System.ValueType to create custom value type.");
            }
        }

        ClassAttrSyntax ClassAttrSyntax.Enum
        {
            get 
            { 
                throw new InvalidOperationException("Extend System.Enum to create custom value type.");
            }
        }

        ClassAttrSyntax ClassAttrSyntax.Interface
        {
            get 
            { 
                currentClassAttributes |= TypeAttributes.Interface;
                return this; 
            }
        }

        ClassAttrSyntax ClassAttrSyntax.Sealed
        {
            get 
            { 
                currentClassAttributes |= TypeAttributes.Sealed;
                return this; 
            }
        }

        ClassAttrSyntax ClassAttrSyntax.Abstract
        {
            get 
            { 
                currentClassAttributes |= TypeAttributes.Abstract;
                return this; 
            }
        }

        ClassAttrSyntax ClassAttrSyntax.Auto
        {
            get 
            { 
                currentClassAttributes |= TypeAttributes.AutoLayout;
                return this; 
            }
        }

        ClassAttrSyntax ClassAttrSyntax.Sequential
        {
            get 
            { 
                currentClassAttributes |= TypeAttributes.SequentialLayout;
                return this; 
            }
        }

        ClassAttrSyntax ClassAttrSyntax.Explicit
        {
            get 
            { 
                currentClassAttributes |= TypeAttributes.ExplicitLayout;
                return this; 
            }
        }

        ClassAttrSyntax ClassAttrSyntax.Ansi
        {
            get 
            { 
                currentClassAttributes |= TypeAttributes.AnsiClass;
                return this; 
            }
        }

        ClassAttrSyntax ClassAttrSyntax.Unicode
        {
            get 
            { 
                currentClassAttributes |= TypeAttributes.UnicodeClass;
                return this; 
            }
        }

        ClassAttrSyntax ClassAttrSyntax.Autochar
        {
            get 
            { 
                currentClassAttributes |= TypeAttributes.AutoClass;
                return this; 
            }
        }

        ClassAttrSyntax ClassAttrSyntax.Import
        {
            get 
            { 
                currentClassAttributes |= TypeAttributes.Import;
                return this; 
            }
        }

        ClassAttrSyntax ClassAttrSyntax.Serializable
        {
            get 
            { 
                currentClassAttributes |= TypeAttributes.Serializable;
                return this; 
            }
        }

        ClassAttrSyntax ClassAttrSyntax.NestedPublic
        {
            get 
            { 
                currentClassAttributes |= TypeAttributes.NestedPublic;
                return this; 
            }
        }

        ClassAttrSyntax ClassAttrSyntax.NestedPrivate
        {
            get 
            { 
                currentClassAttributes |= TypeAttributes.NestedPrivate;
                return this; 
            }
        }

        ClassAttrSyntax ClassAttrSyntax.NestedFamily
        {
            get 
            { 
                currentClassAttributes |= TypeAttributes.NestedFamily;
                return this; 
            }
        }

        ClassAttrSyntax ClassAttrSyntax.NestedAssembly
        {
            get 
            { 
                currentClassAttributes |= TypeAttributes.NestedAssembly;
                return this; 
            }
        }

        ClassAttrSyntax ClassAttrSyntax.NestedFamANDAssem
        {
            get 
            { 
                currentClassAttributes |= TypeAttributes.NestedFamANDAssem;
                return this; 
            }
        }

        ClassAttrSyntax ClassAttrSyntax.NestedFamORAssem
        {
            get 
            { 
                currentClassAttributes |= TypeAttributes.NestedFamORAssem;
                return this; 
            }
        }

        ClassAttrSyntax ClassAttrSyntax.BeforeFieldInit
        {
            get 
            { 
                currentClassAttributes |= TypeAttributes.BeforeFieldInit;
                return this; 
            }
        }

        ClassAttrSyntax ClassAttrSyntax.SpecialName
        {
            get 
            { 
                currentClassAttributes |= TypeAttributes.SpecialName;
                return this; 
            }
        }

        ClassAttrSyntax ClassAttrSyntax.RTSpecialName
        {
            get 
            { 
                currentClassAttributes |= TypeAttributes.RTSpecialName;
                return this; 
            }
        }

        WantMethAttr WantMethAttrThen<WantMethAttr>.Static
        {
            get
            {
                method.Attributes |= MethodAttributes.Static;
                return this;
            }
        }

        WantMethAttr WantMethAttrThen<WantMethAttr>.Public
        {
            get
            {
                this.method.Attributes |= MethodAttributes.Public;
                return this;
            }
        }

        WantMethAttr WantMethAttrThen<WantMethAttr>.Private
        {
            get
            {
                this.method.Attributes |= MethodAttributes.Private;
                return this;
            }
        }

        WantMethAttr WantMethAttrThen<WantMethAttr>.Family
        {
            get
            {
                this.method.Attributes |= MethodAttributes.Family;
                return this;
            }
        }

        WantMethAttr WantMethAttrThen<WantMethAttr>.Final
        {
            get
            {
                this.method.Attributes |= MethodAttributes.Final;
                return this;
            }
        }

        WantMethAttr WantMethAttrThen<WantMethAttr>.Specialname
        {
            get
            {
                this.method.Attributes |= MethodAttributes.SpecialName;
                return this;
            }
        }

        WantMethAttr WantMethAttrThen<WantMethAttr>.Virtual
        {
            get
            {
                this.method.Attributes |= MethodAttributes.Virtual;
                return this;
            }
        }

        WantMethAttr WantMethAttrThen<WantMethAttr>.Abstract
        {
            get
            {
                this.method.Attributes |= MethodAttributes.Abstract;
                return this;
            }
        }

        WantMethAttr WantMethAttrThen<WantMethAttr>.Assembly
        {
            get
            {
                this.method.Attributes |= MethodAttributes.Assembly;
                return this;
            }
        }

        WantMethAttr WantMethAttrThen<WantMethAttr>.Famandassem
        {
            get
            {
                this.method.Attributes |= MethodAttributes.FamANDAssem;
                return this;
            }
        }

        WantMethAttr WantMethAttrThen<WantMethAttr>.Famorassem
        {
            get
            {
                this.method.Attributes |= MethodAttributes.FamORAssem;
                return this;
            }
        }

        WantMethAttr WantMethAttrThen<WantMethAttr>.Privatescope
        {
            get
            {
                this.method.Attributes |= MethodAttributes.Private;
                return this;
            }
        }

        WantMethAttr WantMethAttrThen<WantMethAttr>.Hidebysig
        {
            get
            {
                this.method.Attributes |= MethodAttributes.HideBySig;
                return this;
            }
        }

        WantMethAttr WantMethAttrThen<WantMethAttr>.Newslot
        {
            get
            {
                this.method.Attributes |= MethodAttributes.NewSlot;
                return this;
            }
        }

        WantMethAttr WantMethAttrThen<WantMethAttr>.Rtspecialname
        {
            get
            {
                this.method.Attributes |= MethodAttributes.RTSpecialName;
                return this;
            }
        }

        WantMethAttr WantMethAttrThen<WantMethAttr>.Unmanagedexp
        {
            get
            {
                this.method.Attributes |= MethodAttributes.UnmanagedExport;
                return this;
            }
        }

        WantMethAttr WantMethAttrThen<WantMethAttr>.Reqsecobj
        {
            get
            {
                this.method.Attributes |= MethodAttributes.RequireSecObject;
                return this;
            }
        }

        PInvAttrSyntax WantMethAttrThen<WantMethAttr>.BeginPinvokeimpl(QStr s1, QStr s2)
        {
            throw new NotImplementedException();
        }

        PInvAttrSyntax WantMethAttrThen<WantMethAttr>.BeginPinvokeimpl(QStr s)
        {
            throw new NotImplementedException();
        }

        PInvAttrSyntax WantMethAttrThen<WantMethAttr>.BeginPinvokeimpl()
        {
            this.method.Attributes |= MethodAttributes.PInvokeImpl;
            return this;
        }

        WantMethAttr PInvAttrSyntax.EndPinvokeimpl()
        {
            throw new NotImplementedException();
        }

        PInvAttrSyntax PInvAttrSyntax.Nomangle
        {
            get { throw new NotImplementedException(); }
        }

        PInvAttrSyntax PInvAttrSyntax.Ansi
        {
            get { throw new NotImplementedException(); }
        }

        PInvAttrSyntax PInvAttrSyntax.Unicode
        {
            get { throw new NotImplementedException(); }
        }

        PInvAttrSyntax PInvAttrSyntax.Autochar
        {
            get { throw new NotImplementedException(); }
        }

        PInvAttrSyntax PInvAttrSyntax.Lasterr
        {
            get { throw new NotImplementedException(); }
        }

        PInvAttrSyntax PInvAttrSyntax.Winapi
        {
            get { throw new NotImplementedException(); }
        }

        PInvAttrSyntax PInvAttrSyntax.Cdecl
        {
            get { throw new NotImplementedException(); }
        }

        PInvAttrSyntax PInvAttrSyntax.Stdcall
        {
            get { throw new NotImplementedException(); }
        }

        PInvAttrSyntax PInvAttrSyntax.Thiscall
        {
            get { throw new NotImplementedException(); }
        }

        PInvAttrSyntax PInvAttrSyntax.Fastcall
        {
            get { throw new NotImplementedException(); }
        }

        WantCallConv WantCallConvThen<WantCallConv>.Instance
        {
            get 
            {
                this.method.HasThis = true;
                return this; 
            }
        }

        WantCallConv WantCallConvThen<WantCallConv>.Explicit
        {
            get
            {
                this.method.ExplicitThis = true;
                return this;
            }
        }

        WantCallKind WantCallKindThen<WantCallKind>.Default
        {
            get 
            {
                this.method.CallingConvention = MethodCallingConvention.Default;
                return this;
            }
        }

        WantCallKind WantCallKindThen<WantCallKind>.VarArg
        {
            get
            {
                this.method.CallingConvention = MethodCallingConvention.VarArg;
                return this;
            }
        }

        WantCallKind WantCallKindThen<WantCallKind>.Cdecl
        {
            get
            {
                this.method.CallingConvention = MethodCallingConvention.C;
                return this;
            }
        }

        WantCallKind WantCallKindThen<WantCallKind>.StdCall
        {
            get
            {
                this.method.CallingConvention = MethodCallingConvention.StdCall;
                return this;
            }
        }

        WantCallKind WantCallKindThen<WantCallKind>.ThisCall
        {
            get 
            { 
                this.method.CallingConvention = MethodCallingConvention.ThisCall;
                return this;
            }
        }

        WantCallKind WantCallKindThen<WantCallKind>.FastCall
        {
            get 
            { 
                this.method.CallingConvention = MethodCallingConvention.FastCall;
                return this;
            }
        }

        WantImplAttr WantImplAttrThen<WantImplAttr>.Native
        {
            get
            {
                this.method.ImplAttributes |= MethodImplAttributes.Native;
                return this;
            }
        }

        WantImplAttr WantImplAttrThen<WantImplAttr>.Cil
        {
            get
            {
                this.method.ImplAttributes |= MethodImplAttributes.IL;
                return this;
            }
        }

        WantImplAttr WantImplAttrThen<WantImplAttr>.NoOptimization
        {
            get
            {
                this.method.ImplAttributes |= MethodImplAttributes.NoOptimization;
                return this;
            }
        }

        WantImplAttr WantImplAttrThen<WantImplAttr>.Managed
        {
            get
            {
                this.method.ImplAttributes |= MethodImplAttributes.Managed;
                return this;
            }
        }

        WantImplAttr WantImplAttrThen<WantImplAttr>.Unmanaged
        {
            get
            {
                this.method.ImplAttributes |= MethodImplAttributes.Unmanaged;
                return this;
            }
        }

        WantImplAttr WantImplAttrThen<WantImplAttr>.ForwardRef
        {
            get
            {
                this.method.ImplAttributes |= MethodImplAttributes.ForwardRef;
                return this;
            }
        }

        WantImplAttr WantImplAttrThen<WantImplAttr>.Runtime
        {
            get
            {
                this.method.ImplAttributes |= MethodImplAttributes.Runtime;
                return this;
            }
        }

        WantImplAttr WantImplAttrThen<WantImplAttr>.InternalCall
        {
            get
            {
                this.method.ImplAttributes |= MethodImplAttributes.InternalCall;
                return this;
            }
        }

        WantImplAttr WantImplAttrThen<WantImplAttr>.Synchronized
        {
            get
            {
                this.method.ImplAttributes |= MethodImplAttributes.Synchronized;
                return this;
            }
        }

        WantImplAttr WantImplAttrThen<WantImplAttr>.NoInlining
        {
            get
            {
                this.method.ImplAttributes |= MethodImplAttributes.NoInlining;
                return this;
            }
        }

        AssemblyRefSyntax AssemblyRefSyntax.Hash(Bytes hashBytes)
        {
            throw new NotImplementedException();
        }

        AssemblyRefSyntax AssemblyRefSyntax.PublicKeyToken(Bytes keyBytes)
        {
            if (this.currentAssemblyRef != null)
            {
                this.currentAssemblyRef.PublicKeyToken = keyBytes.Data;
            }

            return this;
        }

        AssemblyRefSyntax AssemblyRefSyntax.Version(int major, int minor, int build, int revision)
        {
            if (this.currentAssemblyRef != null)
            {
                currentAssemblyRef.Version = new Version(major, minor, build, revision);
            }

            return this;
        }

        AssemblyRefSyntax AssemblyRefSyntax.Locale(QStr qstr)
        {
            throw new NotImplementedException();
        }

        AssemblyRefSyntax AssemblyRefSyntax.Locale(Bytes localeBytes)
        {
            throw new NotImplementedException();
        }

        CilDocumentSyntax AssemblyRefSyntax.EndAssemblyExtern()
        {
            this.currentAssemblyRef = null;
            return this;
        }

        AssemblyRefSyntax CustomAttributesSyntax<AssemblyRefSyntax>.CustomAttribute(Ref<Types> customType)
        {
            throw new NotImplementedException();
        }

        public WantField Field()
        {
            this.field = new FieldDefinition(null, default(FieldAttributes), module.TypeSystem.Object);
            type.Fields.Add(field);
            return this;
        }

        public WantFieldAttr Repeat(int length)
        {
            throw new NotSupportedException();
        }

        WantFieldAttr WantFieldAttrThen<WantFieldAttr>.Static()
        {
            this.field.Attributes |= FieldAttributes.Static;
            return this;
        }

        WantFieldAttr WantFieldAttrThen<WantFieldAttr>.Public()
        {
            this.field.Attributes |= FieldAttributes.Public;
            return this;
        }

        WantFieldAttr WantFieldAttrThen<WantFieldAttr>.Private()
        {
            this.field.Attributes |= FieldAttributes.Private;
            return this;
        }

        WantFieldAttr WantFieldAttrThen<WantFieldAttr>.Family()
        {
            this.field.Attributes |= FieldAttributes.Family;
            return this;
        }

        public WantFieldAttr Initonly()
        {
            this.field.Attributes |= FieldAttributes.InitOnly;
            return this;
        }

        WantFieldAttr WantFieldAttrThen<WantFieldAttr>.Rtspecialname()
        {
            this.field.Attributes |= FieldAttributes.RTSpecialName;
            return this;
        }

        WantFieldAttr WantFieldAttrThen<WantFieldAttr>.Specialname()
        {
            this.field.Attributes |= FieldAttributes.SpecialName;
            return this;
        }

        WantFieldAttr WantFieldAttrThen<WantFieldAttr>.Assembly()
        {
            this.field.Attributes |= FieldAttributes.Assembly;
            return this;
        }

        WantFieldAttr WantFieldAttrThen<WantFieldAttr>.Famandassem()
        {
            this.field.Attributes |= FieldAttributes.FamANDAssem;
            return this;
        }

        WantFieldAttr WantFieldAttrThen<WantFieldAttr>.Famorassem()
        {
            this.field.Attributes |= FieldAttributes.FamORAssem;
            return this;
        }

        WantFieldAttr WantFieldAttrThen<WantFieldAttr>.Privatescope()
        {
            throw new NotSupportedException();
        }

        public WantFieldAttr Literal()
        {
            this.field.Attributes |= FieldAttributes.Literal;
            return this;
        }

        public WantFieldAttr Notserialized()
        {
            this.field.Attributes |= FieldAttributes.NotSerialized;
            return this;
        }

        public WantFieldName OfType(Ref<Types> type)
        {
            this.field.FieldType = GetTypeValue(type);
            return this;
        }

        WantFieldAt WantFieldName.Named(string fieldName)
        {
            field.Name = fieldName;
            return this;
        }

        public WantFieldInit HasRVA
        {
            get 
            { 
                field.Attributes |= FieldAttributes.HasFieldRVA;
                return this;
            }
        }

        ClassSyntax WantFieldInitThen<ClassSyntax>.Init(Bytes bytes)
        {
            field.InitialValue = bytes.Data;
            return this;
        }
    }
}
