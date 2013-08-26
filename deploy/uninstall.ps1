param (
    $InstallPath,
    $ToolsPath,
    $Package,
    $Project
)

$PropsFile = 'IronText.props'
$TargetsFile = 'IronText.targets'

Add-Type -AssemblyName 'Microsoft.Build, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'

$MSBProject = [Microsoft.Build.Evaluation.ProjectCollection]::GlobalProjectCollection.GetLoadedProjects($Project.FullName) |
    Select-Object -First 1

$ExistingImports = $MSBProject.Xml.Imports |
    Where-Object { $_.Project -like "*\$TargetsFile" -or $_.Project -like "*\$PropsFile" }
if ($ExistingImports) {
    $ExistingImports | 
        ForEach-Object {
            $MSBProject.Xml.RemoveChild($_) | Out-Null
        }
    $Project.Save()
}
