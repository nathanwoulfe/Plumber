[![Build status](https://ci.appveyor.com/api/projects/status/ap94da7169wk0g0v?svg=true)](https://ci.appveyor.com/project/nathanwoulfe/umbracoworkflow)


Plumber
=========
A workflow solution for Umbraco. Plumber adds a heap of useful bits and pieces to Umbraco, to allow multi-staged workflow approval for publish/unpublish actions. 

This is still very much a beta and probably not suitable for production use, unless you like to live dangerously. Not because it doesn't work (it does, I swear!), but due to the high chance of breaking changes as the package approaches a release build. There's plenty of room for improvement.

To get started, clone the repo, build the Workflow project (build action should do some copying), then start the Workflow.Site project (credentials below). Running localbuild.bat in /BuildPackage should generate a package in /BuildPackage/artifacts, while the default Grunt task in Workflow looks after the usual concat/minify/copy type tasks. localbuild.bat also runs the default Grunt task to ensure the built package is reasonably tidy.

In the backoffice, the new Workflow section has a documentation tab, which should offer a bit more explanation of features and processes.

Grab the latest release from AppVeyor:
=========
https://ci.appveyor.com/project/nathanwoulfe/umbracoworkflow/build/artifacts

Workflow.Site
=========
Login for the test site:

**Username**: test@mail.com<br/>
**Password**: password

