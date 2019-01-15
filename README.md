# Plumber - workflow for Umbraco

[![Build status](https://ci.appveyor.com/api/projects/status/ap94da7169wk0g0v?svg=true)](https://ci.appveyor.com/project/nathanwoulfe/umbracoworkflow)
[![Latest build](https://img.shields.io/nuget/dt/Workflow.Umbraco.svg)](https://www.nuget.org/packages/Workflow.Umbraco)
[![NuGet release](https://img.shields.io/nuget/dt/Workflow.Umbraco.svg)](https://www.nuget.org/packages/Workflow.Umbraco)
[![Our Umbraco project page](https://img.shields.io/badge/our-umbraco-brightgreen.svg)](https://our.umbraco.org/projects/backoffice-extensions/plumber-workflow-for-umbraco)

Plumber adds a heap of useful bits and pieces to Umbraco, to allow multi-staged workflow approval for publish/unpublish actions. 

To get started, clone the repo, build the Workflow project (build action should do some copying), then start the Workflow.Site project (credentials below). Running localbuild.bat in /BuildPackage should generate a package in /BuildPackage/artifacts, while the default Grunt task in Workflow looks after the usual concat/minify/copy type tasks. localbuild.bat also runs the default Grunt task to ensure the built package is reasonably tidy.

In the backoffice, the new Workflow section has a documentation tab, which offers more explanation of features and processes, or you can [read the documentation here](Workflow/DOCS.md).

## Get started

### Grab the latest release from Our.Umbraco:

https://our.umbraco.com/packages/backoffice-extensions/plumber-workflow-for-umbraco/

### Grab the latest build from AppVeyor:

https://ci.appveyor.com/project/nathanwoulfe/umbracoworkflow/build/artifacts

### Or install via Nuget (preferred option):

```Install-Package Workflow.Umbraco```

## Workflow.Site

Logins for the test site:

**Username**: EditorUser@mail.com<br />
**Password**: JOP{H#kG

**Username**: AdminUser@mail.com<br />
**Password**: tzX)TSiA

Users have different permissions - admin has the full set, editor is much more limited.

Other user accounts exist in the site, as do a range of workflow configurations.

The database is my development environment, so I'll likely introduce breaking changes (password, users deleted etc), but will try to remember not to remove the two listed above.

## Like it? Love it? 

[I'm on Patreon](https://www.patreon.com/user?u=16154946) if you feel like buying me a coffee.

