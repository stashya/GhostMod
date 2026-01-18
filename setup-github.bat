@echo off
setlocal

:: ============================================
:: GhostMod GitHub Setup Script
:: ============================================
::
:: This script initializes a git repository and
:: pushes to GitHub. Run this ONCE after downloading.
::
:: Prerequisites:
::   - Git installed (https://git-scm.com)
::   - GitHub CLI installed (https://cli.github.com) OR
::     an empty repo already created on GitHub
::
:: ============================================

echo.
echo ========================================
echo  GhostMod GitHub Setup
echo ========================================
echo.

:: Check if git is available
where git >nul 2>nul
if %ERRORLEVEL% neq 0 (
    echo ERROR: Git not found!
    echo Please install Git from https://git-scm.com
    pause
    exit /b 1
)

:: Check if already a git repo
if exist ".git" (
    echo This folder is already a git repository.
    echo.
    set /p CONTINUE="Reinitialize? This will reset git history. (y/n): "
    if /i not "%CONTINUE%"=="y" (
        echo Aborted.
        pause
        exit /b 0
    )
    rmdir /s /q .git 2>nul
)

:: Get GitHub username
echo.
set /p GITHUB_USER="Enter your GitHub username: "
if "%GITHUB_USER%"=="" (
    echo Username cannot be empty!
    pause
    exit /b 1
)

:: Get repo name (default: GhostMod)
set /p REPO_NAME="Enter repository name (default: GhostMod): "
if "%REPO_NAME%"=="" set REPO_NAME=GhostMod

echo.
echo Setting up repository: https://github.com/%GITHUB_USER%/%REPO_NAME%
echo.

:: Initialize git
echo Initializing git repository...
git init
git branch -M main

:: Add all files
echo Adding files...
git add .

:: Initial commit
echo Creating initial commit...
git commit -m "Initial release v1.0.0"

:: Add remote
set REPO_URL=https://github.com/%GITHUB_USER%/%REPO_NAME%.git
echo.
echo Adding remote: %REPO_URL%
git remote add origin %REPO_URL%

:: Check if gh CLI is available for repo creation
where gh >nul 2>nul
if %ERRORLEVEL% equ 0 (
    echo.
    set /p CREATE_REPO="Create the repository on GitHub now? (y/n): "
    if /i "!CREATE_REPO!"=="y" (
        echo Creating repository on GitHub...
        gh repo create %REPO_NAME% --public --source=. --description "Ghost Racing mod for Initial Drift Online"
    )
)

:: Push
echo.
echo Ready to push to GitHub.
echo.
echo IMPORTANT: Make sure the repository exists on GitHub first!
echo Go to: https://github.com/new
echo Repository name: %REPO_NAME%
echo.
set /p PUSH_NOW="Push to GitHub now? (y/n): "
if /i "%PUSH_NOW%"=="y" (
    echo Pushing to GitHub...
    git push -u origin main
    if %ERRORLEVEL% equ 0 (
        echo.
        echo SUCCESS! Your code is now on GitHub:
        echo https://github.com/%GITHUB_USER%/%REPO_NAME%
    ) else (
        echo.
        echo Push failed. Make sure:
        echo  1. The repository exists on GitHub
        echo  2. You have permission to push
        echo  3. You're logged into git (try: git config --global credential.helper store)
        echo.
        echo You can try again manually with: git push -u origin main
    )
) else (
    echo.
    echo Setup complete! When ready, push with:
    echo   git push -u origin main
)

echo.
pause
