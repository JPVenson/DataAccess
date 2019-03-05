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
$tests = (Get-ChildItem -Path "." -Recurse -Include *.Tests.dll -Name | where {$_ -NotLike "*\debug\*"} | where {$_ -NotLike "*\obj\*"})

# Run tests
$baseDirectory = (Get-Location).tostring();
try
{
    Foreach($path in $tests){
        "Test Path: $path"
        Set-Location $baseDirectory;            
        $workingDirectory = $baseDirectory + "\" + [System.IO.Path]::GetDirectoryName($path);
        "Changing Directory to Test Assembly $workingDirectory"
        Set-Location $workingDirectory;
        $filename = [System.IO.Path]::GetFileName($path);

        & $nunit $filename /framework:"net-4.0" /result:"$OutDir\Tests.nunit.xml" --dispose-runners | Write-Host
    }
}
finally
{
        Set-Location $baseDirectory;    
}

$xPathToCases = "//test-run[@testcasecount]"
$pathToResultsFile = Resolve-Path "./TestResults/testResult.xml";

if(-not ([System.IO.File]::Exists($pathToResultsFile))){
    "File Does not exists";
    $pathToResultsFile
    exit;
}

[xml]$testCasesContent = Get-Content $pathToResultsFile;


$testRun = Select-Xml $xPathToCases $testCasesContent | Select-Object -ExpandProperty Node;

$countNunitTests = $testRun.testcasecount;
$countFailedNunitTests = $testRun.failed;

Write-Host "##vso[task.setvariable variable=Tests.TotalTests]$countNunitTests"
Write-Host "##vso[task.setvariable variable=Tests.TotalFailedTests]$countFailedNunitTests"

IF (-not ($countFailedNunitTests -eq 0) ){
    Write-Host "Errors in Nunit tests";
    $LASTEXITCODE = 1;
}
