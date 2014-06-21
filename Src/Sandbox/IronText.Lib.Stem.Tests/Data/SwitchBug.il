// Static method definition and call

.assembly SwitchBug { }

.assembly extern mscorlib
{
    .ver 4:0:0:0
    .publickeytoken = (B7 7A 5C 56 19 34 E0 89)
}

.module "SwitchBug.exe"

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
    ldc.i4 3
    // BUG: Because array production has no scope context info
    switch (L_Fail, L_Fail, L_Fail, L_A)
    br L_Fail
L_A: 
    ldstr "Success"    
    call void MainClass::print(string text)
    ret
L_Fail:    
    ldstr "Fail"    
    call void MainClass::print(string text)
    ret
  }
}
