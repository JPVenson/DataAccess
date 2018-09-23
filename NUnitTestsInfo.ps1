param($pathToResultsFile)

$xPathToCases = "//test-run[@testcasecount]"
$pathToResultsFile = Resolve-Path $pathToResultsFile;

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



#[Reflection.Assembly]::LoadWithPartialName("System.Xml.Linq") | Out-Null
#$doc = [System.Xml.Linq.XDocument]::Load($pathToResultsFile)
#	
#$result = $doc.Root.Elements() | where {$_.Name.LocalName -eq "Results" } | select -First 1
#$tests = $result.Elements() | where {$_.Name.LocalName -eq "UnitTestResult" } 
#$failedTests = $tests | where {$_.Attribute("outcome").Value -eq "Failed"} 
#
##limit to 10, because otherwise errors are not included in the emailed log
##foreach ($test in $failedTests | select -First 10) 
##{
##	$error = "##vso[task.logissue type=error;]" + $test.Attribute("testName").Value.Trim()
##	Write-Host $error
##}
#$countNunitTests = $tests.Length;
#$countFailedNunitTests = $failedTests.Length;
#Write-Host "##vso[task.setvariable variable=Tests.TotalTests]$countNunitTests"
#Write-Host "##vso[task.setvariable variable=Tests.TotalFailedTests]$countFailedNunitTests"