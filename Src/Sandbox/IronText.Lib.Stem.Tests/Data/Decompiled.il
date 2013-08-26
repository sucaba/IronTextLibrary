.assembly extern mscorlib {}

.assembly FactorialSample
{
    .ver 1:0:1:0
}
.module FactorialSample.exe

.namespace ILSample
{
    .class public auto ansi beforefieldinit Program
        extends [mscorlib]System.Object
    {
        .method public hidebysig specialname rtspecialname instance void .ctor() cil managed
		{
		    .maxstack 8
		    L_0000: ldarg.0 
		    L_0001: call instance void [mscorlib]System.Object::.ctor()
		    L_0006: ret 
		}

        .method public hidebysig static int32 Factorial(int32 n) cil managed
		{
		    .maxstack 3
		    .locals init (
		        [0] int32 result,
		        [1] int32 CS$1$0000,
		        [2] bool CS$4$0001)
		    L_0000: nop 
		    L_0001: ldarg.0 
		    L_0002: ldc.i4.0 
		    L_0003: ceq 
		    L_0005: ldc.i4.0 
		    L_0006: ceq 
		    L_0008: stloc.2 
		    L_0009: ldloc.2 
		    L_000a: brtrue.s L_0012
		    L_000c: nop 
		    L_000d: ldc.i4.1 
		    L_000e: stloc.0 
		    L_000f: nop 
		    L_0010: br.s L_001f
		    L_0012: nop 
		    L_0013: ldarg.0 
		    L_0014: ldarg.0 
		    L_0015: ldc.i4.1 
		    L_0016: sub 
		    L_0017: call int32 ILSample.Program::Factorial(int32)
		    L_001c: mul 
		    L_001d: stloc.0 
		    L_001e: nop 
		    L_001f: ldloc.0 
		    L_0020: stloc.1 
		    L_0021: br.s L_0023
		    L_0023: ldloc.1 
		    L_0024: ret 
		}

		.method public hidebysig static void Main() cil managed
		{
		    .entrypoint
		    .maxstack 8
		    L_0000: nop 
		    L_0001: ldstr "Factorial(5)={0}"
		    L_0006: ldc.i4.5 
		    L_0007: call int32 ILSample.Program::Factorial(int32)
		    L_000c: box int32
		    L_0011: call void [mscorlib]System.Console::WriteLine(string, object)
		    L_0016: nop 
		    L_0017: ret 
		}

    }
}

 
