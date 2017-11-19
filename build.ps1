param (
    [string]$envName = "local",
    [Hashtable]$overrides,
    [bool]$skipLocalFile=$false,
    [string]$outputPath
)

&.\configure-env.ps1 $envName -overrides $overrides -skipLocalFile $skipLocalFile
wintersmith build -o $outputPath