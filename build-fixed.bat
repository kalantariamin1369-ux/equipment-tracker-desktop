@echo off
setlocal enabledelayedexpansion

echo ========================================
echo    Equipment Tracker Build Script
echo ========================================

REM Check if MSBuild is available
where msbuild >nul 2>nul
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: MSBuild not found. Please install Visual Studio or Build Tools.
    pause
    exit /b 1
)

REM Clean previous builds
echo Cleaning previous builds...
if exist "EquipmentTracker\bin" rmdir /s /q "EquipmentTracker\bin"
if exist "EquipmentTracker\obj" rmdir /s /q "EquipmentTracker\obj"
if exist "dist" rmdir /s /q "dist"

REM Restore NuGet packages
echo Restoring NuGet packages...
nuget restore EquipmentTracker.sln
if %ERRORLEVEL% NEQ 0 (
    echo WARNING: NuGet restore failed. Continuing with build...
)

REM Build for x64 (recommended for SQLite)
echo Building for x64 platform...
msbuild EquipmentTracker.sln /p:Configuration=Release /p:Platform=x64 /p:OutputPath=bin\Release\
if %ERRORLEVEL% NEQ 0 (
    echo Build failed for x64. Trying AnyCPU...
    msbuild EquipmentTracker.sln /p:Configuration=Release /p:Platform="Any CPU" /p:OutputPath=bin\Release\
    if %ERRORLEVEL% NEQ 0 (
        echo ERROR: Both x64 and AnyCPU builds failed
        pause
        exit /b 1
    )
    set "PLATFORM=AnyCPU"
) else (
    set "PLATFORM=x64"
)

echo Build completed successfully for !PLATFORM! platform!
echo Output location: EquipmentTracker\bin\Release\

REM Create distribution folder
echo Creating distribution...
mkdir "dist" 2>nul
xcopy "EquipmentTracker\bin\Release\*" "dist\" /s /e /y /q

REM Copy SQLite native libraries
echo Copying SQLite native libraries...

REM Find SQLite packages folder
for /d %%i in ("packages\System.Data.SQLite.Core.*") do (
    set "SQLITE_PACKAGE=%%i"
    goto :found_sqlite
)

:found_sqlite
if defined SQLITE_PACKAGE (
    echo Found SQLite package: !SQLITE_PACKAGE!
    
    REM Copy x64 native library
    if exist "!SQLITE_PACKAGE!\runtimes\win-x64\native\SQLite.Interop.dll" (
        if not exist "dist\x64" mkdir "dist\x64"
        copy "!SQLITE_PACKAGE!\runtimes\win-x64\native\SQLite.Interop.dll" "dist\x64\" /y
        echo   ✓ Copied x64 SQLite.Interop.dll
    )
    
    REM Copy x86 native library for compatibility
    if exist "!SQLITE_PACKAGE!\runtimes\win-x86\native\SQLite.Interop.dll" (
        if not exist "dist\x86" mkdir "dist\x86"
        copy "!SQLITE_PACKAGE!\runtimes\win-x86\native\SQLite.Interop.dll" "dist\x86\" /y
        echo   ✓ Copied x86 SQLite.Interop.dll
    )
    
    REM Copy main SQLite.Interop.dll to root for AnyCPU builds
    if exist "!SQLITE_PACKAGE!\build\net48\x64\SQLite.Interop.dll" (
        copy "!SQLITE_PACKAGE!\build\net48\x64\SQLite.Interop.dll" "dist\" /y
        echo   ✓ Copied main SQLite.Interop.dll
    ) else if exist "!SQLITE_PACKAGE!\runtimes\win-x64\native\SQLite.Interop.dll" (
        copy "!SQLITE_PACKAGE!\runtimes\win-x64\native\SQLite.Interop.dll" "dist\" /y
        echo   ✓ Copied main SQLite.Interop.dll from runtime
    )
) else (
    echo WARNING: SQLite package not found. Native libraries may be missing.
    echo Please ensure System.Data.SQLite.Core package is installed.
)

REM Create a simple deployment check script
echo Creating deployment verification script...
echo @echo off > "dist\check-dependencies.bat"
echo echo Checking Equipment Tracker dependencies... >> "dist\check-dependencies.bat"
echo if exist "EquipmentTracker.exe" (echo   ✓ Main executable found) else (echo   ✗ Main executable missing) >> "dist\check-dependencies.bat"
echo if exist "System.Data.SQLite.dll" (echo   ✓ SQLite managed library found) else (echo   ✗ SQLite managed library missing) >> "dist\check-dependencies.bat"
echo if exist "SQLite.Interop.dll" (echo   ✓ SQLite native library found) else (echo   ✗ SQLite native library missing) >> "dist\check-dependencies.bat"
echo if exist "x64\SQLite.Interop.dll" (echo   ✓ x64 SQLite native library found) else (echo   ✗ x64 SQLite native library missing) >> "dist\check-dependencies.bat"
echo echo. >> "dist\check-dependencies.bat"
echo echo Press any key to continue... >> "dist\check-dependencies.bat"
echo pause >> "dist\check-dependencies.bat"

REM List distribution contents
echo.
echo ========================================
echo           DISTRIBUTION CONTENTS
echo ========================================
dir "dist" /b

echo.
echo ========================================
echo         BUILD COMPLETED SUCCESSFULLY!
echo ========================================
echo Distribution created in 'dist' folder
echo Run 'dist\check-dependencies.bat' to verify deployment
echo.
pause