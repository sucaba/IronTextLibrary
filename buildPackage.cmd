copy /y %~dp0deploy\uninstall.ps1 %~dp0deploy\tools\net40\uninstall.ps1

pushd %~dp0deploy\lib\net40 

    move IronText.Build.dll ..\..\build\net40
    move IronText.Build.pdb ..\..\build\net40
    move IronText.Compiler.dll ..\..\build\net40
    move IronText.Compiler.pdb ..\..\build\net40
    move Mono.Cecil.dll ..\..\build\net40
    move Mono.Cecil.pdb ..\..\build\net40
    move Mono.Cecil.Pdb.dll ..\..\build\net40
    move Mono.Cecil.Pdb.pdb ..\..\build\net40
    :: Remaining files:
    :: IronText.Core.dll
    :: IronText.Core.pdb
    :: ScannerSyntax.gram
    :: ScannerSyntax.scan

popd

mkdir %~dp0install
%~dp0deploy\NuGet.exe pack %~dp0deploy\Package.nuspec -OutputDirectory install
