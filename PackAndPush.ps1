$projects = @("ThriftSharp", "ThriftSharp.Extensions")

ForEach ($proj in $projects) {
  nuget pack "$proj/$proj.csproj" -Symbols -Prop Configuration=Release
}

Write-Host "`nFinished packing.`n" -ForegroundColor Green

$pkgs = dir | where { $_.Extension -eq ".nupkg" -and !$_.Name.Contains(".symbols") }

ForEach ($pkg in $pkgs) {
  nuget push $pkg.Name
}

Write-Host "`nPushed all packages.`n" -ForegroundColor Green