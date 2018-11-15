@echo off
powershell -NoLogo -NoProfile -ExecutionPolicy ByPass -Command "& """%~dp0Build.ps1""" -restore -build -sign -pack -ci %*"
exit /b %ErrorLevel%
