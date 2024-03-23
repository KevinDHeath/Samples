param ( [string]$outputRoot )
if( "" -eq $outputRoot ) { $output = "$PSScriptRoot" } else { $output = "$outputRoot" }

function DotNet_Test {
  param($testProject)
  $results = "$output\TestResults\" + $testProject.split('.')[0].ToLower()
  $history = "$results\history"

  # Build project
  Push-Location "$PSScriptRoot\$testProject"
  if( "$PSScriptRoot" -eq "$output" ) { dotnet clean | Out-Null }
  dotnet build --no-restore -c Debug
  if( $LASTEXITCODE -gt 0 ) { Pop-Location; exit 1 }

  # Run unit tests
  if( Test-Path "$results" ) { Remove-Item -Path "$results\*" -Recurse | Out-Null }
  dotnet test --settings "$PSScriptRoot\.runsettings" --no-build --results-directory "$results"
  Pop-Location

  # Copy the oldest 3 history files
  $wrk = "$PSScriptRoot\$testProject\Testdata\history"
  if( Test-Path "$wrk" ) {
    if( !(Test-Path "$history") ) { New-Item -ItemType Directory -Path "$history" | Out-Null }
    foreach( $file in ( Get-ChildItem "$wrk" -Filter *.xml | Sort-Object -Property FullName | Select-Object -First 3 ) )
    { Copy-Item -Path "$file" -Destination "$history" }
  }

  # If running locally generate reports
  if( "$PSScriptRoot" -eq "$output" ) {
    $reports = "$results\*\coverage.cobertura.xml"
    $rpTypes = "-reporttypes:Html;Badges;MarkdownSummaryGithub"
    $setting = "--settings:createSubdirectoryForAllReportTypes=true"
    $title = "-title:$testProject"
    Write-Host "Generating coverage reports for $testProject" -ForegroundColor Yellow
    reportgenerator -reports:"$reports" -targetdir:"$results\reports" -historydir:$history $title -verbosity:Warning $rpTypes $setting
  }
}

# Perform tasks  
DotNet_Test 'xUnitTestProject'
