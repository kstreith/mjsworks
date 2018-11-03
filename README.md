dotnet restore --packages packages
dotnet build -c "Release"
.\build.ps1

.\build.ps1 does the following
gulp and run wyam

When running locally either
  .\configure-env.ps1
  wyam build
  wyam preview -w 8080 -w

