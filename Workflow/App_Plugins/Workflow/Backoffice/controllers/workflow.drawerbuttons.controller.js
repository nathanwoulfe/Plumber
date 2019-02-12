(() => {
    'use strict';

    function controller($scope, $rootScope, $q, $window, userService, workflowResource, workflowGroupsResource, workflowActionsService, contentEditingHelper, editorState, $routeParams, notificationsService, plumberHub) {

        this.active = false;
        this.excludeNode = false;
        this.buttonGroupState = 'init';

        let workflowConfigured = false;
        let dirty = false;

        let user;
        let settings;
        let groups;

        const dashboardClick = editorState.current === null;
        const defaultButtons = contentEditingHelper.configureContentEditorButtons({
            create: $routeParams.create,
            content: $scope.content,
            methods: {
                saveAndPublish: $scope.saveAndPublish,
                sendToPublish: $scope.sendToPublish,
                save: $scope.save,
                unPublish: $scope.unPublish
            }
        });

        let defaultUnpublish;
        if (defaultButtons.subButtons) {
            defaultUnpublish = defaultButtons.subButtons.filter(x => x.alias === 'unpublish')[0];
        }

        const saveAndPublish = defaultButtons.defaultButton && defaultButtons.defaultButton.labelKey === 'buttons_saveAndPublish';

        const buttons = {
            approveButton: {
                labelKey: 'workflow_approveButtonLong',
                handler: item => {
                    this.workflowOverlay = workflowActionsService.action(item, 'Approve', dashboardClick);
                }
            },
            cancelButton: {
                labelKey: 'workflow_cancelButtonLong',
                cssClass: 'danger',
                handler: item => {
                    this.workflowOverlay = workflowActionsService.cancel(item, dashboardClick);
                }
            },
            rejectButton: {
                labelKey: 'workflow_rejectButton',
                cssClass: 'warning',
                handler: item => {
                    this.workflowOverlay = workflowActionsService.action(item, 'Reject', dashboardClick);
                }
            },
            resubmitButton: {
                labelKey: 'workflow_resubmitButton',
                handler: item => {
                    this.workflowOverlay = workflowActionsService.action(item, 'Resubmit', dashboardClick);
                }
            },
            detailButton: {
                labelKey: 'workflow_detailButton',
                handler: item => {
                    this.workflowOverlay = workflowActionsService.detail(item);
                }
            },
            saveButton: {
                labelKey: 'workflow_saveButton',
                cssClass: 'success',
                handler: $scope.save
            },
            publishButton: {
                labelKey: 'workflow_publishButton',
                cssClass: 'success',
                handler: () => {
                    var that = this;
                    var contentLastUpdateDate = $scope.content.updateDate;

                    // Perform a Save first, to ensure we catch scenario where the user hasn't been presented
                    // with a Save button due to issues with Umbraco's dirty-checking
                    $scope.save().then(function(d) {
                        // There's no way to know if the Save succeeded from here, as Umbraco returns 200 when
                        // the Saving event is canceled, and the promise only rejects for 500 errors.  For now,
                        // we'll determine success by comparing the updateDate of the current form against the 
                        // one returned from the server's new model.
                        var saveSucceeded = d.updateDate !== contentLastUpdateDate;

                        if (saveSucceeded) {
                            that.workflowOverlay = workflowActionsService.initiate(editorState.current.name, editorState.current.id, true);
                        } else {
                            notificationsService.error('Workflow: Unable to request publish, saving the content failed');
                        }
                    }, function() {
                        notificationsService.error('Workflow', 'Unable to request publish, saving the content failed');
                    });
                }
            },
            unpublishButton: {
                labelKey: 'workflow_unpublishButton',
                cssClass: 'warning',
                handler: () => {
                    this.workflowOverlay = workflowActionsService.initiate(editorState.current.name, editorState.current.id, false);
                }
            }
        };

        /**
         * any user with access to the workflow section will be able to action workflows ie cancel outside their group membership
         * @param {any} task
         */
        const checkUserAccess = task => {
            this.task = task || this.task;
            this.canAction = false;

            this.isAdmin = user.allowedSections.indexOf('workflow') !== -1;
            const currentTaskUsers = this.task.permissions[this.task.currentStep].userGroup.usersSummary;

            if (currentTaskUsers.indexOf(`|${user.id}|`) !== -1) {
                this.canAction = true;
            }

            if (this.active) {

                this.buttonGroup = {};

                if (dirty && (this.userCanEdit || (this.canAction && !settings.lockIfActive))) {
                    this.buttonGroup.defaultButton = buttons.saveButton;
                }
                // primary button is approve when the user is in the approving group and task is not rejected
                else if (this.canAction && !this.rejected) {
                    this.buttonGroup.defaultButton = buttons.approveButton;
                } else if (this.userCanEdit) {
                    // rejected tasks show the resubmit, only when the user is the original author
                    this.buttonGroup.defaultButton = buttons.resubmitButton;
                } else { // all other cases see the detail button
                    this.buttonGroup.defaultButton = buttons.detailButton;
                }
                 
                this.buttonGroup.subButtons = [];

                // if the default button isn't detail, it should be first in the sub button set
                if (this.buttonGroup.defaultButton !== buttons.detailButton) {
                    this.buttonGroup.subButtons.push(buttons.detailButton);
                }

                // if the user is in the approving group, and the task is not rejected, add reject to sub buttons
                if (this.canAction && !this.rejected) {
                    this.buttonGroup.subButtons.push(buttons.rejectButton);
                }
                // if the user is admin, the change author or in the approving group for a non-rejected task, add the cancel button
                if (this.isAdmin || this.userCanEdit || this.isChangeAuthor || (this.canAction && !this.rejected)) {
                    this.buttonGroup.subButtons.push(buttons.cancelButton);
                }
            }
        };

        /**
         * Manages the default states for the buttons - updates when no active task, or when the content form is dirtied
         */
        const setButtons = () => {
            // default button will be null when the current user has browse-only permission
            this.buttonGroup = {};

            if (workflowConfigured && defaultButtons.defaultButton !== null) {
                const subButtons = saveAndPublish ?
                    [buttons.unpublishButton, defaultButtons.defaultButton, buttons.saveButton] :
                    [buttons.unpublishButton, buttons.saveButton];

                // insert the default unpublish button into the subbutton array
                if (saveAndPublish && defaultUnpublish) {
                    subButtons.splice(1, 0, defaultUnpublish);
                }

                // if the content is dirty, show save. otherwise show request approval
                this.buttonGroup = {
                    defaultButton: dirty ? buttons.saveButton : buttons.publishButton,
                    subButtons: dirty ? (saveAndPublish ? [defaultButtons.defaultButton] : []) : subButtons
                };
            } else {
                if (defaultButtons.defaultButton !== null && !this.active) {
                    this.buttonGroup = defaultButtons;
                }
            }

            // if a task is active, the default buttons should be updated to match the current user's access/role in the workflow
            if (this.active) {
                checkUserAccess();
            }
        };

        /**
         * 
         */
        const getPendingTasks = () => {
            workflowResource.getNodePendingTasks(editorState.current.id)
                .then(resp => {
                    if (resp.items && resp.items.length) {
                        this.active = true;

                        // if the workflow status is rejected, the original author should be able to edit and resubmit
                        const currentTask =
                            resp.items.reduce((prev, current) => prev.taskId > current.taskId ? prev : current);

                        this.rejected = currentTask.cssStatus === 'rejected';

                        // if the task has been rejected and the current user requested the change, let them edit
                        this.isChangeAuthor = currentTask.requestedById === user.id;
                        this.userCanEdit = this.rejected && this.isChangeAuthor;

                        checkUserAccess(currentTask);
                    } else {
                        this.active = false;
                        setButtons();
                    }
                },
                () => { });
        };

        const getNodeTasks = () => {
            // only refresh if viewing a content node
            if (editorState.current && !editorState.current.trashed) {
                // check if the node is included in the workflow model
                // groups has been fetched already
                const nodePerms = workflowResource.checkNodePermissions(groups,
                    editorState.current.id,
                    editorState.current.contentTypeAlias);
                const ancestorPerms = workflowResource.checkAncestorPermissions(editorState.current.path, groups);

                if ((nodePerms.approvalPath.length ||
                    nodePerms.contentTypeApprovalPath.length ||
                    ancestorPerms.length) && !this.excludeNode) {

                    workflowConfigured = true;
                    getPendingTasks();
                } else {
                    workflowConfigured = false;
                    this.buttonGroup = defaultButtons;
                }
                
            }
        };

        // use this to ensure changes are saved when submitting for publish
        // event is broadcast from the buttons directive, which watches the content form
        $rootScope.$on('contentFormDirty', (event, data) => {
            dirty = data;
            setButtons();
        });

        // ensures dash/buttons refresh
        $rootScope.$on('workflowActioned', () => {
            getNodeTasks();
        });

        $rootScope.$on('configUpdated', () => {
            getNodeTasks();
        });

        // subscribe to signalr magick for button state
        // events are raised in ActionController - doesn't matter what they return, only care that they are raised
        // as it indicates a change of state for the button
        //const hubEvent = id => {
        //    if (!dashboardClick && id === editorState.current.id) {
        //        getNodeTasks();
        //    }
        //};

        //plumberHub.initHub(hub => {
        //    hub.on('workflowStarted', data => {
        //        hubEvent(data.nodeId);
        //    });

        //    hub.on('taskCancelled', data => {
        //        hubEvent(data.nodeId);
        //    });

        //    hub.on('taskApproved', data => {
        //        hubEvent(data.nodeId);
        //    });

        //    hub.on('taskRejected', data => {
        //        hubEvent(data.nodeId);
        //    });

        //    hub.start();
        //});

        // preview should not save, if the content is in a workflow
        this.preview = content => {
            // Chromes popup blocker will kick in if a window is opened 
            // outwith the initial scoped request. This trick will fix that.
            const previewWindow = $window.open(`preview/?id=${content.id}`, 'umbpreview');
            // Build the correct path so both /#/ and #/ work.
            const redirect = Umbraco.Sys.ServerVariables.umbracoSettings.umbracoPath + '/preview/?id=' + content.id;
            previewWindow.location.href = redirect;
        };

        // it all starts here
        const promises = [userService.getCurrentUser(), workflowResource.getSettings(), workflowGroupsResource.get()];

        $q.all(promises)
            .then(resp => {
                [user, settings, groups] = resp;
                this.excludeNode = workflowResource.checkExclusion(settings.excludeNodes, editorState.current.path);
                getNodeTasks();
            });
    }

    // register controller 
    angular.module('plumber').controller('Workflow.DrawerButtons.Controller',
        [
            '$scope',
            '$rootScope',
            '$q',
            '$window',
            'userService',
            'plmbrWorkflowResource',
            'plmbrGroupsResource',
            'plmbrActionsService',
            'contentEditingHelper',
            'editorState',
            '$routeParams',
            'notificationsService',
            'plumberHub', controller]);
})();