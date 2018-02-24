$outputDir = Resolve-Path artifacts
Push-Location .\src\OpenRCT2.API
  Remove-Item -Force -Recurse $outputDir
  dotnet publish -c Release -o $outputDir
Pop-Location
