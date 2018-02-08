### Installing

If you're reading this, you've successfully installed the latest version of Plumber, the workflow solution for Umbraco.

The installation process created a new backoffice section (you're in it), and added a suite of new features to your Umbraco website.

Check in on the Workflow section dashboard to stay up-to-date with the latest release - when the installed version is superceded, Plumber will prompt you to upgrade.

### Getting started

#### Settings

Plumber comes pre-wired with sensible defaults, but these should be modified to best suit your site.

- Default approval group: this is the fallback group for cases when no workflow is configured. You'll need to configure your groups before setting this value.
- Flow type: how should the approval flow progress? These options manage how the change author is included in the workflow:
    - All: all steps of the workflow must be completed, and all users will be notified of tasks (including the change author)
    - Other: all steps where the original change author is NOT a member of the group must be completed
    - Exclude: all steps must be completed, but the original change author will not be notified of tasks if they are a member of subsequent approval groups
- Send notifications: if your users are active in the backoffice, email notifications might not be required. Turn them off here.
- Workflow email: Set a sender address for notification emails. This defaults to the system email as defined in umbracoSettings.config
- Site URL: the URL for the public website (including schema - http[s])
- Edit site URL: the URL for the editing environment (including schema - http[s])
- Exclude nodes: nodes selected here are excluded from the workflow engine and will be published per the configured Umbraco user permissions
- Document-type approvals: configure workflows to be applied to all content of the selected document type. Refer to xx for details on approval flow types

#### Configure approval groups

Plumber uses a separate groups model from the rest of your Umbraco website. It's different, but looks familiar.

The view will display the current assigned responsibilities for the group, to help keep track of who is approving which pages.

Add users to approval groups to determine which users will be responsible for approving content changes.

- Group email: sometimes it's more appropriate to send workflow notifications to a generic inbox rather than the individual group members. Add a value here to do exactly that.
- Description: it isn't used anywhere other than the group view. It's a note to remind you why the group exists.

#### Approval flow types

Approval flows come in three flavours: explicit, inherited and document-type.

A given content node may have all three approval flow types applied, but only one will be applied per the following order of priority:

- Explicit: set directly on a content node via the context menu. This type will take priority over all others.
- Document-type: set in the settings section. This approval flow will apply to all content of the selected document type, unless the node has an explicit flow set.
- Inherited: if a node has no explicit approval flow, nor a flow applied to its document-type, Plumber will traverse the content tree until it finds a node with an explicit flow, and will use this flow for the current change. For this reason, an approval flow should always be set on the homepage node as a default.

Current responsibilites for groups can be reviewed on the user group view, for explicit and document-type approval flows only.