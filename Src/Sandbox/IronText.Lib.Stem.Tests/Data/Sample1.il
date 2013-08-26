// Static method definition and call

.assembly Sample1 { }

.assembly extern mscorlib
{
    .ver 4:0:0:0
    .publickeytoken = (B7 7A 5C 56 19 34 E0 89)
}

.module "Sample1.exe"

.class public MainClass
{
  .method private static void print (string text)
  {
    ldarg text
    call void [mscorlib]System.Console::WriteLine (string text)
    ret
  }

  .method public static void Main ()
  {
    .entrypoint
    ldstr "I'm from the IL Assembly Language..."
    call void MainClass::print(string text)
    ret
  }
}
