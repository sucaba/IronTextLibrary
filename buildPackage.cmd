copy /y %~dp0deploy\uninstall.ps1 %~dp0deploy\tools\net40\uninstall.ps1
%~dp0deploy\NuGet.exe pack %~dp0deploy\Package.nuspec -OutputDirectory install