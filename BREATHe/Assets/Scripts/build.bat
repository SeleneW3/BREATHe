@echo off
echo Starting build process...

:: 设置环境变量（如果需要）
set PYTHONPATH=%PYTHONPATH%;.

:: 执行打包命令
pyinstaller --name BreathDetection ^
            --onefile ^
            --windowed ^
            --hidden-import numpy ^
            --hidden-import matplotlib ^
            --hidden-import pyaudio ^
            --add-data "C:\Windows\Fonts\simhei.ttf;." ^
            Scripts\BreathDetection.py

:: 检查打包是否成功
if %ERRORLEVEL% EQU 0 (
    echo Build completed successfully!
    echo Executable can be found in the dist folder
) else (
    echo Build failed with error code %ERRORLEVEL%
)

:: 暂停以查看输出
pause