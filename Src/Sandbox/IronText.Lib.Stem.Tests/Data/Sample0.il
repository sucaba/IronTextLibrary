// Entry point and extern method call test

.assembly Sample0 { }

.assembly extern mscorlib
{
    .ver 4:0:0:0
    .publickeytoken = (B7 7A 5C 56 19 34 E0 89)
}

.module "Sample0.exe"

.class public MainClass
{
  .method public static void Main ()
  {
    .entrypoint
    ldstr "I'm from the IL Assembly Language..."
    call void [mscorlib]System.Console::WriteLine (string text)
    ret
  }
}
