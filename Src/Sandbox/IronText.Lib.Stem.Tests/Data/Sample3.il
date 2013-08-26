// Local variables test

.assembly Sample3 { }

.assembly extern mscorlib
{
    .ver 4:0:0:0
    .publickeytoken = (B7 7A 5C 56 19 34 E0 89)
}



.module "Sample3.exe"

.class public MainClass
{
  .method public static void Main ()
  {
    .entrypoint
    .locals s string
    ldstr "I'm from the IL Assembly Language..."
    stloc s
    ldloc s
    call void [mscorlib]System.Console::WriteLine (string text)
    ret
  }
}
  
