ECHO APPVEYOR_REPO_BRANCH: %APPVEYOR_REPO_BRANCH%
ECHO APPVEYOR_REPO_TAG: %APPVEYOR_REPO_TAG%
ECHO APPVEYOR_BUILD_NUMBER : %APPVEYOR_BUILD_NUMBER%
ECHO APPVEYOR_BUILD_VERSION : %APPVEYOR_BUILD_VERSION%
ECHO PACKAGE_SUFFIX : %UMBRACO_PACKAGE_PRERELEASE_SUFFIX%
cd ..\Workflow\App_Plugins\Workflow
Call npm install
Call grunt default --buildversion=%APPVEYOR_BUILD_VERSION% --buildbranch=%APPVEYOR_REPO_BRANCH% --packagesuffix=%UMBRACO_PACKAGE_PRERELEASE_SUFFIX%
cd ..\..\..\BuildPackage\
Call Tools\nuget.exe restore ..\Workflow.sln
Call "%programfiles(x86)%\Microsoft Visual Studio\2017\BuildTools\MSBuild\15.0\Bin\MSBuild.exe" package.proj