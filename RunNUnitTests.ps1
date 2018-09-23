$ProjectDir = "."
$PackagesDir = "$ProjectDir\packages"
$OutDir = "$ProjectDir"

# Install NUnit Test Runner
$nuget = "nuget"
& $nuget install NUnit.Console -o "$PackagesDir"

# Set nunit path test runner
$nunit = Get-ChildItem "$PackagesDir" -Filter nunit3-console.exe -Recurse | % { $_.FullName }
#$nunit = "$ProjectDir\packages\NUnit.Console .2.6.2\tools\nunit-console.exe"
"Nunit Path: $nunit"
#Find tests in OutDir
$tests = (Get-ChildItem "$OutDir\JPB.DataAccess.Tests\bin\Release\" -Recurse -Include *.Tests.dll)
"Test Path: $tests"
# Run tests
& $nunit $tests /framework:"net-4.0" /result:"$OutDir\Tests.nunit.xml" --dispose-runners