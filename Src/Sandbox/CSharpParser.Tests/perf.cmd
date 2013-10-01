@echo off
set command=

set TOOLSDIR=C:\Program Files (x86)\Microsoft Visual Studio 11.0\Team Tools\Performance Tools
set VSInstr=%TOOLSDIR%\vsinstr.exe
set VSPerfClrEnv=%TOOLSDIR%\VSPerfCLREnv.cmd
set VSPerfCmd=%TOOLSDIR%\VSPerfCmd.exe
set Nunit=C:\Program Files (x86)\TestDriven.NET 3\NUnit\2.5\nunit-console-x86.exe

set TestName=CSharpParser.Tests.BasicTest.ProfilableRecognizeTest
set OUTPUTPATH=CSharpParsingTest.vsp

:: set TestName=CSharpParser.Tests.BasicTest.BigTest
:: set OUTPUTPATH=CSharpParsingTest_Big.vsp

pushd %~dp0

:: Run test to build derived DLLs
%command% "%Nunit%" CSharpParser.Tests.dll /run:CSharpParser.Tests.BasicTest.ProfilableRecognizeTest

:: Use the VSInstr tool to generate an instrumented version of the target application.
%command% "%VSInstr%" IronText.Core.dll 
%command% "%VSInstr%" CSharpParser.Tests.dll 

:: Initialize the .NET Framework profiling environment variables.
%command% call "%VSPerfClrEnv%" /tracegclife

:: Start the profiler. 
%command% "%VSPerfCmd%" /start:trace /output:%OUTPUTPATH%

:: Starts data collection for all processes.
%command% "%VSPerfCmd%" /globalon

%command% "%Nunit%" CSharpParser.Tests.dll /run:%TestName%

:: Stops data collection for all processes.
%command% "%VSPerfCmd%" /globalon

:: Shut down the profiler. Type:
%command% "%VSPerfCmd%" /shutdown

:: (Optional) Clear the profiling environment variables.
%command% call "%VSPerfClrEnv%" /off

%command% start %OUTPUTPATH%

popd
