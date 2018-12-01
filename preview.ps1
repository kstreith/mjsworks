param (
    [string]$envName = "local",
    [Hashtable]$overrides,
    [bool]$skipLocalFile=$false,
    [string]$outputPath="output"
)

&.\configure-env.ps1 $envName -overrides $overrides -skipLocalFile $skipLocalFile
npm install
dotnet restore --packages packages Deploy\Deploy.csproj
dotnet build -c "Release"
dotnet .\packages\wyam\2.0.0\tools\netcoreapp2.1\wyam.dll preview -p 8080 -w
