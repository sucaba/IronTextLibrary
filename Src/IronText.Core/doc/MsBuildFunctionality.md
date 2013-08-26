I) Building Derived Dlls during build

II) Copying Derived Dlls Through The Project Refences
=====================================================

Goal
----
    1) For every referenced project check if there is corresponding derived dll and copy it locally
    2) For every referenced project check if there is corresponding up-to-date derived dll and copy it locally

Existing functionality in Microsoft.Common.targets
--------------------------------------------------

CoreBuild
    ResolveReferences
        ResolveAssemblyReferences depends on ResolveProjectReferences (it provides _ResolvedProjectReferencePaths)
           provides ReferenceCopyLocalPaths

CoreBuild
    PrepareForRun
        CopyFilesToOutputDirectory
            _CopyFilesMarkedCopyLocal
                uses ReferenceCopyLocalPaths

Main Question
-------------
How xml, pdb files are copied along with references?
Answer: Throught the AllowedReferenceRelatedFileExtensions property used in ResolveProjectReferences Target

Solution
--------

Override AllowedReferenceRelatedFileExtensions Property before :

  <Import Project="$(MSBuildBinPath)\Microsoft.CSharp.targets" />

to register .Derived.dll as additional files

