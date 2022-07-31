Param(
  [String]$version="1.16.5",
  [String]$location=$env:VINTAGE_STORY
) 

echo "Creating $version -> $location"
New-Item `
  -ItemType SymbolicLink `
  -Path $version `
  -Target $location `
  -Force
