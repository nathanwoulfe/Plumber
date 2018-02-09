[![Build status](https://ci.appveyor.com/api/projects/status/ap94da7169wk0g0v?svg=true)](https://ci.appveyor.com/project/nathanwoulfe/umbracoworkflow)

Plumber
=========
A workflow solution for Umbraco. Plumber adds a heap of useful bits and pieces to Umbraco, to allow multi-staged workflow approval for publish/unpublish actions. 

This is still in beta and probably not suitable for production use, unless you like to live dangerously. Not because it doesn't work (it does), but due to the high chance of breaking changes as the package approaches a release build. There's plenty of room for improvement.

To get started, clone the repo, build the Workflow project (build action should do some copying), then start the Workflow.Site project (credentials below). Running localbuild.bat in /BuildPackage should generate a package in /BuildPackage/artifacts, while the default Grunt task in Workflow looks after the usual concat/minify/copy type tasks. localbuild.bat also runs the default Grunt task to ensure the built package is reasonably tidy.

In the backoffice, the new Workflow section has a documentation tab, which offers more explanation of features and processes, or you can [read the documentation here](Workflow/DOCS.md).

The workflow model is derived from the workflow solution developed by myself and the web team at [USC](http://www.usc.edu.au), but re-visions that basic three-step workflow into something much more flexible.

Grab the latest release from AppVeyor:
=========
https://ci.appveyor.com/project/nathanwoulfe/umbracoworkflow/build/artifacts

On install via Nuget:
=========
```Install-Package Workflow.Umbraco```

Workflow.Site
=========
Login for the test site:

**Username**: test@mail.com<br/>
**Password**: password

