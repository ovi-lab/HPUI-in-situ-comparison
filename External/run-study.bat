start "" /b "..\Builds\HPUI-in-situ-comparison.exe"
start poetry run experiment-server run .\Assets\hpui-in-situ-comparison-study1.toml
start "" "http://127.0.0.1:5000"
For /f "tokens=2-4 delims=/ " %%a in ('date /t') do (set mdate=%%c-%%a-%%b)
For /f "tokens=1-2 delims=/:" %%a in ("%TIME%") do (set mtime=%%a%%b)
xcopy C:\Users\amshamoh\AppData\LocalLow\ubc_ok_ovilab\HPUI-in-situ-comparison C:\Users\amshamoh\AppData\LocalLow\ubc_ok_ovilab\HPUI-in-situ-comparison_back_%mdate%_%mtime% /e /q /i
