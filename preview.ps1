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
.\packages\wyam\1.7.1\tools\net462\wyam.exe -p 8080 -w
