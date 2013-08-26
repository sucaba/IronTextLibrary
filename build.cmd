call %~dp0clean.cmd

"%windir%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe" /p:Configuration=Deploy;Platform="Any CPU" %~dp0Src\IronText.sln %*

%~dp0deploy\tools\net40\MergeDerived.exe %~dp0deploy\lib\net40\IronText.Compiler.dll
