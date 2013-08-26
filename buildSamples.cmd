%~dp0deploy\nuget install IronText -Source %~dp0install  -OutputDirectory %~dp0Samples\packages

"%windir%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe" /p:Configuration=Debug;Platform="Any CPU" %~dp0Samples\Samples.sln %*
