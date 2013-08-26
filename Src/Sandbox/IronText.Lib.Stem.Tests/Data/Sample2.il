// Static method definition and recursive call

.assembly Sample2 { }

.assembly extern mscorlib
{
    .ver 4:0:0:0
    .publickeytoken = (B7 7A 5C 56 19 34 E0 89)
}

.module "Sample2.exe"

.class public MainClass
{
  .method private static int32 factorial (int32 n)
  {
    ldarg n
    brtrue.s NON_ZERO
    ldc.i4 1
    br.s END
NON_ZERO:
    ldarg n
    ldc.i4 1
    sub
    call int32 MainClass::factorial(int32 n)
    ldarg n
    mul
END:
    ret
  }

  .method public static void Main ()
  {
    .entrypoint
    ldc.i4 10
    call int32 MainClass::factorial(int32 n)
    call void [mscorlib]System.Console::WriteLine (int32 text)
    ret
  }
}
