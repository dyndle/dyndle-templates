$gitbranch = git rev-parse --abbrev-ref HEAD
$whoami = whoami
$env:SonarScannerVersion = $whoami+":"+$gitbranch
Write-Host $env:SonarScannerVersion
cmd /c "sonar-scanner.bat"