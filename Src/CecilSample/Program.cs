using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CecilSample
{
    class Program
    {
        private static byte[] data = new byte[] { 65, 65, 65, 65, 65, 65, 65, 65, 65, 65 };

        static void Main(string[] args)
        {
            var assm = AssemblyDefinition.CreateAssembly(
                            new AssemblyNameDefinition(
                                "myassem",
                                new Version("1.0.0.0")), "mainmodule", ModuleKind.Console);
            ModuleDefinition mod = assm.Modules[0];
            TypeDefinition type = new TypeDefinition(
                                        "ghoori", "chai", TypeAttributes.Class|TypeAttributes.Public)
            {
                BaseType = mod.TypeSystem.Object
            };
            mod.Types.Add(type);


            var dataFild = AddAssemblyData(mod, type);

            var fild = new FieldDefinition(
                            "arrField", 
                            FieldAttributes.Private | FieldAttributes.Static,
                            mod.Import(typeof(byte[])));
            type.Fields.Add(fild);

            // Static constructor
            {
                var cctor = new MethodDefinition(
                                ".cctor",
                                MethodAttributes.Static | MethodAttributes.HideBySig | MethodAttributes.RTSpecialName | MethodAttributes.SpecialName,
                                mod.Import(typeof(void)));
                var il = cctor.Body.GetILProcessor();
                LdAssemblyDataBytes(mod, dataFild, il);
                il.Emit(OpCodes.Stsfld, fild);
                il.Emit(OpCodes.Ret);

                type.Methods.Add(cctor);
            }

            {
                var mainMethod = new MethodDefinition(
                                    "Main",
                                    MethodAttributes.Public | MethodAttributes.Static,
                                    mod.TypeSystem.Void);
                var il = mainMethod.Body.GetILProcessor();
                il.Emit(OpCodes.Ldstr, "Hello world");
                var print = mod.Import(typeof(Console).GetMethod("WriteLine", new[] { typeof(string) }));
                il.Emit(OpCodes.Call, print);

                il.Emit(OpCodes.Ldsfld, fild);
                il.Emit(OpCodes.Ldlen);
                il.Emit(OpCodes.Box, mod.TypeSystem.Int32);
                il.Emit(OpCodes.Callvirt, mod.Import(typeof(object).GetMethod("ToString")));
                il.Emit(OpCodes.Call, print);

                type.Methods.Add(mainMethod);
                mod.EntryPoint = mainMethod;
            }

            // will appear as a sequence of 'A' in binary view of output file. 
            assm.Write("ass.exe");
        }

        private static void LdAssemblyDataBytes(ModuleDefinition mod, FieldDefinition dataFild, ILProcessor il)
        {
            var initArray = mod.Import(typeof(System.Runtime.CompilerServices.RuntimeHelpers).GetMethod("InitializeArray"));
            il.Emit(OpCodes.Ldc_I4, data.Length);
            il.Emit(OpCodes.Newarr, mod.TypeSystem.Byte);
            il.Emit(OpCodes.Dup);
            il.Emit(OpCodes.Ldtoken, dataFild);
            il.Emit(OpCodes.Call, initArray);
        }

        private static FieldDefinition AddAssemblyData(ModuleDefinition mod, TypeDefinition type)
        {
            var dataFildTypeName = "Arraytype" + data.Length;
            var dataFildType = new TypeDefinition(
                                "", dataFildTypeName,
                                TypeAttributes.AnsiClass | TypeAttributes.ExplicitLayout | TypeAttributes.Sealed)
            {
                BaseType = mod.Import(typeof(ValueType)),
                PackingSize = 1,
                ClassSize = data.Length
            };
            mod.Types.Add(dataFildType);

            var dataFild = new FieldDefinition(
                            "dataField",
                            FieldAttributes.Private | FieldAttributes.Static | FieldAttributes.Assembly | FieldAttributes.HasFieldRVA,
                            dataFildType);
            dataFild.InitialValue = data;
            type.Fields.Add(dataFild);
            return dataFild;
        }
    }
}
