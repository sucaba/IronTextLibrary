set MSBuildExtensionsDir=%ProgramFiles(x86)%\MSBuild\
set MSBuildCommonDir=%MSBuildExtensionsDir%4.0\Microsoft.Common.Targets\
set IronTextDir=%MSBuildExtensionsDir%IronText\

rmdir /q /s "%IronTextDir%"

del "%MSBuildCommonDir%ImportBefore\IronText.ImportBefore.Common.props"
del "%MSBuildCommonDir%ImportAfter\IronText.ImportAfter.Common.targets"
