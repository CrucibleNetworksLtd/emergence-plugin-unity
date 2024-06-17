@echo off
setlocal enabledelayedexpansion

rem Set the .git directory path
set GIT_DIR=.git

rem Read the current branch name from HEAD
for /f "tokens=*" %%a in ('type %GIT_DIR%\HEAD') do set GIT_HEAD=%%a
set GIT_BRANCH=!GIT_HEAD:~16!

rem Read the latest commit hash from the current branch ref
set /p GIT_COMMIT=<%GIT_DIR%\refs\heads\!GIT_BRANCH!

rem Create the Resources directory if it doesn't exist
if not exist "Assets\YourPlugin\Resources" mkdir "Assets\Emergence\Resources"

rem Write the Git information to a text file
echo Branch: !GIT_BRANCH! / Commit: !GIT_COMMIT! > Assets\Emergence\Resources\git_info.txt

echo "Git info generated successfully."