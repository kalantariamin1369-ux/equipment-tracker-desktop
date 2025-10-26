@echo off
echo Building Equipment Tracker...

REM Check if MSBuild is available
where msbuild >nul 2>nul
if %ERRORLEVEL% NEQ 0 (
    echo MSBuild not found. Please install Visual Studio or Build Tools.
    pause
    exit /b 1
)

REM Clean previous builds
if exist "EquipmentTracker\bin" rmdir /s /q "EquipmentTracker\bin"
if exist "EquipmentTracker\obj" rmdir /s /q "EquipmentTracker\obj"

REM Build the solution
msbuild EquipmentTracker.sln /p:Configuration=Release /p:Platform="Any CPU"
if %ERRORLEVEL% NEQ 0 (
    echo Build failed
    pause
    exit /b 1
)

echo Build completed successfully!
echo Output location: EquipmentTracker\bin\Release\

REM Create distribution folder
if not exist "dist" mkdir "dist"
xcopy "EquipmentTracker\bin\Release\*" "dist\" /s /e /y

echo Distribution created in 'dist' folder
pause