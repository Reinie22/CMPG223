@echo off
REM ===========================================================================
REM  Nkanyezi Tours - database setup
REM  CMPG223 Group 6
REM
REM  Just double-click this file. It sets up the NkanyeziTours database on
REM  YOUR laptop and points Web.config at YOUR SQL Server instance.
REM ===========================================================================

powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0setup-database.ps1" %*

echo.
pause
