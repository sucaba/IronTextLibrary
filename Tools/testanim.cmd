set DOT="C:\Program Files (x86)\Graphviz 2.28\bin\dot.exe"
set TESTDIR=%~dp0..\Src\IronText.Core.Tests\bin\Debug

for %%i in (%TESTDIR%\*.gv) do (
    %DOT% -Tpng -O %%i)

