%SONAR_SCANNER% begin /k:"trivident_dyndle-templates" /o:trivident-bitbucket /d:sonar.verbose=true /d:sonar.login=%SONAR_LOGIN% /d:sonar.host.url=https://sonarcloud.io
"C:\Program Files (x86)\Microsoft Visual Studio\2019\Professional\MSBuild\Current\Bin\MSBuild.exe" ..\src\Dyndle.Templates.sln" /t:Rebuild 
%SONAR_SCANNER% end /d:sonar.login=%SONAR_LOGIN% 