Param (
    [Parameter(Mandatory=$true)]
    [string]$dirToPublish, 
    [Parameter(Mandatory=$true)]
    [string]$githubusername, 
    [Parameter(Mandatory=$true)]
    [string]$githubemail, 
    [Parameter(Mandatory=$true)]
    [string]$githubaccesstoken, 
    [Parameter(Mandatory=$true)]
    [string]$repositoryname, 
    [Parameter(Mandatory=$true)]
    [string]$commitMessage, 
    [string]$branchName = "gh-pages" 
)

Write-Host "Cloning existing $branchName branch"

git clone https://${githubusername}:$githubaccesstoken@github.com/$githubusername/$repositoryname.git --branch=$branchName ghpages --quiet

if ($lastexitcode -gt 0)
{
  Write-Host "##vso[task.logissue type=error;]Unable to clone repository - check username, access token and repository name. Error code $lastexitcode"
  [Environment]::Exit(1)
}

$to = "ghpages"

Write-Host "Copying new documentation into branch"

Copy-Item $dirToPublish $to -recurse -Force

Write-Host "Committing the $branchName Branch"

cd ghpages
git config core.autocrlf false
git config user.email $githubemail
git config user.name $githubusername
git add *
git commit -m $commitMessage

if ($lastexitcode -gt 0)
{
  Write-Host "##vso[task.logissue type=error;]Error committing - see earlier log, error code $lastexitcode"
  [Environment]::Exit(1)
}

git push

if ($lastexitcode -gt 0)
{
  Write-Host "##vso[task.logissue type=error;]Error pushing to gh-pages branch, probably an incorrect Personal Access Token, error code $lastexitcode"
  [Environment]::Exit(1)
}
