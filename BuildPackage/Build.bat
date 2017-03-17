ECHO APPVEYOR_REPO_BRANCH: %APPVEYOR_REPO_BRANCH%
ECHO APPVEYOR_REPO_TAG: %APPVEYOR_REPO_TAG%
ECHO APPVEYOR_BUILD_NUMBER : %APPVEYOR_BUILD_NUMBER%
ECHO APPVEYOR_BUILD_VERSION : %APPVEYOR_BUILD_VERSION%
REM cd ..\Workflow\App_Plugins\Workflow
REM Call npm install
REM Call grunt --buildversion %APPVEYOR_BUILD_VERSION% --buildbranch %APPVEYOR_REPO_BRANCH% --packagesuffix %UMBRACO_PACKAGE_PRERELEASE_SUFFIX%
cd ..\BuildPackage\
Call Tools\nuget.exe restore ..\Workflow.sln
Call "%programfiles(x86)%\MSBuild\12.0\Bin\MsBuild.exe" package.proj