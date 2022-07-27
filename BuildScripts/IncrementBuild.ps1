#
# This script will increment the build number in an AssemblyInfo.cs file
#

$assemblyInfoPath = $args[0]

$contents = [System.IO.File]::ReadAllText($assemblyInfoPath)

$versionString = [RegEx]::Match($contents,"(AssemblyFileVersion\("")(?:\d+\.\d+\.\d+\.\d+)(""\))")
Write-Host ("AssemblyFileVersion: " +$versionString)

#Parse out the current build number from the AssemblyFileVersion
$currentBuild = [RegEx]::Match($versionString,"(\.)(\d+)(""\))").Groups[2]
Write-Host ("Current Build: " + $currentBuild.Value)

#Increment the build number
$newBuild= [int]$currentBuild.Value +  1
Write-Host ("New Build: " + $newBuild)

#update AssemblyFileVersion and AssemblyVersion, then write to file
Write-Host ("Setting version in assembly info file ")
$contents = [RegEx]::Replace($contents, "(AssemblyVersion\(""\d+\.\d+\.\d+\.)(?:\d+)(""\))", ("`${1}" + $newBuild.ToString() + "`${2}"))
$contents = [RegEx]::Replace($contents, "(AssemblyFileVersion\(""\d+\.\d+\.\d+\.)(?:\d+)(""\))", ("`${1}" + $newBuild.ToString() + "`${2}"))
[System.IO.File]::WriteAllText($assemblyInfoPath, $contents)