function Get-Content($name, $packagePath, [string]$excludePattern)
{
  [string]$nugetPackageRoot = $env:NUGET_PACKAGES
  if ($nugetPackageRoot -eq "")
  {
    $nugetPackageRoot = Join-Path $env:USERPROFILE ".nuget\packages"
  }

  $realPackagePath = Join-Path $nugetPackageRoot $packagePath 
  $resourceTypeName = $name + "Resources"

  $targetsContent = @"
  <Project>
      <ItemGroup>

"@;

  $codeContent = @"
  // This is a generated file, please edit Generate.ps1 to change the contents

  using System.Collections.Generic;
  using Microsoft.CodeAnalysis;

  namespace Express.Reference.Assemblies
  {

"@;

  $codeContent += @"
    internal static class $resourceTypeName
    {

"@;

  $refContent = @"
    public static class $name
    {

"@

  $name = $name.ToLower()
  $list = @(Get-ChildItem -filter *.dll $realPackagePath | %{ $_.FullName })
  $facadesPath = Join-Path $realPackagePath "Facades"
  if (Test-Path $facadesPath) {
    $list += @(Get-ChildItem -filter *.dll $facadesPath | %{ $_.FullName })
  }

  $allPropNames = @()
  foreach ($dllPath in $list)
  {
    $dllName= Split-Path -Leaf $dllPath
    if ($excludePattern -ne "" -and $dllName -match $excludePattern)
    {
      continue
    }

    $dll = $dllName.Substring(0, $dllName.Length - 4)
    $logicalName = "$($name).$($dll)";
    $dllPath = $dllPath.Substring($nugetPackageRoot.Length)
    $dllPath = '$(NuGetPackageRoot)' + $dllPath

    $targetsContent += @"
        <EmbeddedResource Include="$dllPath">
          <LogicalName>$logicalName</LogicalName>
          <Link>Resources\$name\$dllName</Link>
        </EmbeddedResource>

"@

    $propName = $dll.Replace(".", "");
    $allPropNames += $propName
    $fieldName = "_" + $propName
    $codeContent += @"
        private static byte[]? $fieldName;
        internal static byte[] $propName => ResourceLoader.GetOrCreateResource(ref $fieldName, "$logicalName");

"@

    $refContent += @"
        public static PortableExecutableReference $propName { get; } = AssemblyMetadata.CreateFromImage($($resourceTypeName).$($propName)).GetReference(filePath: "$dllName", display: "$dll ($name)");

"@

  }

  $refContent += @"
        public static IEnumerable<PortableExecutableReference> All { get; }= new PortableExecutableReference[]
        {

"@;
    foreach ($propName in $allPropNames)
    {
      $refContent += @"
            $propName,

"@
    }

    $refContent += @"
        };
    }

"@

    $codeContent += @"
    }

"@
    $codeContent += $refContent;

  $targetsContent += @"
    </ItemGroup>
  </Project>
"@;

  $codeContent += @"
}
"@

  return @{ CodeContent = $codeContent; TargetsContent = $targetsContent}
}

$combinedDir = Join-Path $PSScriptRoot "..\Express.Reference.Assemblies"

# Net50
$map = Get-Content "Net50" 'microsoft.netcore.app.ref\5.0.0\ref\net5.0'
$map.CodeContent | Out-File (Join-Path $combinedDir "Generated.Net50.cs") -Encoding Utf8
$map.TargetsContent | Out-File (Join-Path $combinedDir "Generated.Net50.targets") -Encoding Utf8

# AspNet50
$map = Get-Content "AspNet50" 'microsoft.aspnetcore.app.ref\5.0.0\ref\net5.0'
$map.CodeContent | Out-File (Join-Path $combinedDir "Generated.AspNet50.cs") -Encoding Utf8
$map.TargetsContent | Out-File (Join-Path $combinedDir "Generated.AspNet50.targets") -Encoding Utf8
