param (
    [string]$envName = "local",
    [Hashtable]$overrides,
    [bool]$skipLocalFile=$false
)
$file = join-path $env:APPDATA "\mjsworks\${envName}.json"
#echo $file
if (!$skipLocalFile) {
  $json = (Get-Content $file) -join "`n" | ConvertFrom-Json 
  $keys = $json.psobject.properties.name;
} else {
  $keys = $overrides.Keys
}
foreach($key in $keys)
{
    #echo "A", $key
    #echo "B", $json.$key
    if (!$skipLocalFile) {
      $value = $json.$key
    }
    if ($overrides -and $overrides.ContainsKey($key)) {
      $value = $overrides[$key]
      echo "Override value of $key"
    }
    [Environment]::SetEnvironmentVariable($key, $value, "Process")
}
echo "Successfully set environment variables for $envName"