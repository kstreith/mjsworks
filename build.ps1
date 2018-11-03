param (
    [string]$envName = "local",
    [Hashtable]$overrides,
    [bool]$skipLocalFile=$false,
    [string]$outputPath="output"
)

&.\configure-env.ps1 $envName -overrides $overrides -skipLocalFile $skipLocalFile
.\packages\wyam\1.6.0\tools\net462\wyam.exe build -o $outputPath
