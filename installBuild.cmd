set MSBuildExtensionsDir=%ProgramFiles(x86)%\MSBuild\
set MSBuildCommonDir=%MSBuildExtensionsDir%4.0\Microsoft.Common.Targets\
set IronTextDir=%MSBuildExtensionsDir%IronText\

rmdir /q /s "%IronTextDir%"

del "%MSBuildCommonDir%ImportBefore\IronText.ImportBefore.Common.props"
del "%MSBuildCommonDir%ImportAfter\IronText.ImportAfter.Common.targets"

del "%IronTextDir%IronText.MsBuild.dll"
del "%IronTextDir%IronText.MsBuild.pdb"
del "%IronTextDir%IronText.Build.dll" 
del "%IronTextDir%IronText.Build.pdb"

"C:\Windows\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe" %~dp0Build.sln

mkdir "%MSBuildCommonDir%ImportBefore\"
mkdir "%MSBuildCommonDir%ImportAfter\"
mkdir "%IronTextDir%"

copy %~dp0MsBuild\IronText.ImportBefore.Common.targets "%MSBuildCommonDir%ImportBefore\"
copy %~dp0MsBuild\IronText.ImportAfter.Common.targets "%MSBuildCommonDir%ImportAfter\"
copy %~dp0MsBuild\IronText.MsBuild.dll "%IronTextDir%"
copy %~dp0MsBuild\IronText.MsBuild.pdb "%IronTextDir%"
copy %~dp0MsBuild\IronText.Build.dll "%IronTextDir%"
copy %~dp0MsBuild\IronText.Build.pdb "%IronTextDir%"
