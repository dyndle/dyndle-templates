@echo off

cls
set REMOVE_CONFIG=Y
title Template Installer
if not exist "%~dp0\upload-config.xml" ( 
	TcmUploadAssembly.exe upload-config.xml
	GoTo AskKeepConfig
)

GoTo AfterAskKeepConfig
:AskKeepConfig
echo Would you like to keep the configuration file for future uploads? [Y/N] 
set /p "KEEP_CONFIG= "
if %KEEP_CONFIG%==N (
	GoTo RemoveConfig
)
if %KEEP_CONFIG%==n (
	GoTo RemoveConfig
)
:AfterAskKeepConfig


GoTo AfterRemoveConfig
:RemoveConfig
set REMOVE_CONFIG=y
:AfterRemoveConfig

if not exist "%~dp0\upload-config.xml" ( 
	echo Something must have gone wrong, there is no upload-config.xml
	GoTo Exit
)

If "%1"=="" (
	GoTo AskFolder
)
set TARGET_FOLDER=%1%
GoTo AfterAskFolder
:AskFolder
echo Enter the URI of the folder where you want to store the template building blocks:
set /p "TARGET_FOLDER= "
:AfterAskFolder

TcmUploadAssembly.exe upload-config.xml "files\Trivident.Templates.merged.dll" /folder:%TARGET_FOLDER% /verbose

if %REMOVE_CONFIG%==y (
	echo Removing config file
	del upload-config.xml
)
	
:Exit
set TARGET_FOLDER=
set REMOVE_CONFIG=
set KEEP_CONFIG=

