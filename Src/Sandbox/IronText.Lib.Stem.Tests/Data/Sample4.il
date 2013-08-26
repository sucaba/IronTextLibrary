// Array creation test

.assembly Sample4 { }

.assembly extern mscorlib
{
    .ver 4:0:0:0
    .publickeytoken = (B7 7A 5C 56 19 34 E0 89)
}

.module "Sample4.exe"

.class public MainClass
{
  .method public static void Main ()
  {
    .entrypoint
    ldc.i4.1
    newarr [mscorlib]System.String
    pop
    ldc.i4.1
    newarr [mscorlib]System.Double
    pop
    ldc.i4.1
    newarr MainClass
    pop
    ldstr "success!"
    call void [mscorlib]System.Console::WriteLine (string text)
    ret
  }
}
