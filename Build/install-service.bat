echo off
cls
set binPath=%~dp0HaasConnectorAPIs.exe
echo Installing service Haas Connector
echo binPath="%binPath%"
sc create HaasConnector displayName= "HaasConnector" binPath= "%binPath%" start= auto
sc description HaasConnector "Connector service for Haas data collection"
sc start HaasConnector 
pause