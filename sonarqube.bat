FOR /F "tokens=1,2 delims==" %%G IN (sonarqube.properties) DO (set %%G=%%H)  
echo %PROJECT_NAME%
echo %SOLUTION_PATH%
echo %SONAR_URL%
echo %SONAR_LOGIN%
echo %SONAR_ORGANIZATION%
echo USERNAME: %USERNAME%

set vs2015=%ProgramFiles(x86)%\MSBuild\14.0\Bin\MSBuild.exe
set vs2017=%ProgramFiles(x86)%\Microsoft Visual Studio\2017\Professional\MSBuild\15.0\Bin\MSbuild.exe

if exist "%vs2015%" (set MSBUILD="%vs2015%")
if exist "%vs2017%" (set MSBUILD="%vs2017%")

MSBuild.SonarQube.Runner.exe begin /k:"%PROJECT_NAME%" /v:"local-%USERNAME%" /d:"sonar.host.url=%SONAR_URL%" /d:"sonar.login=%SONAR_LOGIN%" /d:"sonar.organization=%SONAR_ORGANIZATION%" 
nuget.exe restore "%SOLUTION_PATH%"
%MSBUILD% "%SOLUTION_PATH%"
MSBuild.SonarQube.Runner.exe end /d:"sonar.login=%SONAR_LOGIN%"