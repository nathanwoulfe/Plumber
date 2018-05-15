(function () {
    'use strict';

    // create controller 
    // since this controller is loaded in response to an injector match, we can use it to check for active workflow groups 
    // and display a message if none are configured, while also displaying the default button set
    function controller($scope, $rootScope, $timeout, $window, userService, workflowResource, workflowGroupsResource, workflowActionsService, contentEditingHelper, angularHelper, contentResource, editorState, $routeParams, notificationsService) {

        this.active = false;
        this.excludeNode = false;
        this.buttonGroupState = 'init';

        let workflowConfigured = false;
        let dirty = false;
        let user = undefined;

        const dashboardClick = editorState.current === null;
        const defaultButtons = contentEditingHelper.configureContentEditorButtons({
            create: $routeParams.create,
            content: $scope.content,
            methods: {
                saveAndPublish: $scope.saveAndPublish,
                sendToPublish: $scope.sendToPublish,
                save: $scope.save,
                unPublish: angular.noop
            }
        });

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
                handler: () => {
                    workflowActionsService.buttonState('busy', editorState.current.id);
                    contentEditingHelper.contentEditorPerformSave({
                        statusMessage: 'Saving...',
                        saveMethod: contentResource.save,
                        scope: $scope,
                        content: editorState.current
                    }).then(resp => {
                        workflowActionsService.buttonState(
                            resp.notifications && resp.notifications[0].type === 3 ? 'success' : 'error', editorState.current.id);
                    });
                }
            },
            publishButton: {
                labelKey: 'workflow_publishButton',
                cssClass: 'success',
                handler: () => {
                    this.workflowOverlay = workflowActionsService.initiate(editorState.current.name, editorState.current.id, true);
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

        // are there common elements between two arrays?
        const common = (arr1, arr2) => arr1.some(el => arr2.indexOf(el) > -1);

        // fetch settings to check node exclusion stat
        workflowResource.getSettings()
            .then(settings => {
                if (settings && settings.excludeNodes) {
                    const exclude = settings.excludeNodes.split(',');
                    // if any elements are shared, exclude the node from the workflow mechanism
                    // by checking the path not just the id, this becomes recursive, and the excludeNodes cascades down the tree
                    if (common(editorState.current.path.split(','), exclude)) {
                        this.excludeNode = true;
                    }
                }
            });

        /**
         * any user with access to the workflow section will be able to action workflows ie cancel outside their group membership
         * @param {any} task
         */
        const checkUserAccess = task => {
            this.task = task || this.task;
            this.canAction = false;

            this.adminUser = user.allowedSections.indexOf('workflow') !== -1;
            const currentTaskUsers = this.task.permissions[this.task.currentStep].userGroup.usersSummary;

            if (currentTaskUsers.indexOf(`|${user.id}|`) !== -1) {
                this.canAction = true;
            }

            if (this.active) {

                this.buttonGroup = {};

                if (dirty && this.userCanEdit) {
                    this.buttonGroup.defaultButton = buttons.saveButton;
                }
                // primary button is approve when the user is in the approving group and task is not rejected
                else if (this.canAction && !this.rejected) {
                    this.buttonGroup.defaultButton = buttons.approveButton;
                } else if (this.userCanEdit) { // rejected tasks show the resubmit, only when the user is the original author
                    this.buttonGroup.defaultButton = buttons.resubmitButton;
                } else { // all other cases see the detail button
                    this.buttonGroup.defaultButton = buttons.detailButton;
                }

                this.buttonGroup.subButtons = [];

                // if the user is in the approving group, and the task is not rejected, add reject to sub buttons
                if (this.canAction && !this.rejected) {
                    this.buttonGroup.subButtons.push(buttons.rejectButton);
                }
                // if the user is admin, the change author or in the approving group for a non-rejected task, add the cancel button
                if (this.isAdmin || this.userCanEdit || this.isChangeAuthor || (this.canAction && !this.rejected)) {
                    this.buttonGroup.subButtons.push(buttons.cancelButton);
                }
            }
        }

        /**
         * Manages the default states for the buttons - updates when no active task, or when the content form is dirtied
         */
        const setButtons = () => {
            // default button will be null when the current user has browse-only permission
            if (workflowConfigured && defaultButtons.defaultButton !== null) {
                const subButtons = saveAndPublish ? [buttons.unpublishButton, defaultButtons.defaultButton, buttons.saveButton] : [buttons.unpublishButton, buttons.saveButton];
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
        }

        const getNodeTasks = () => {
            // only refresh if viewing a content node
            if (editorState.current && !editorState.current.trashed) {

                const getPendingTasks = () => {
                    workflowResource.getNodePendingTasks(editorState.current.id)
                        .then(resp => {
                            if (resp.items && resp.items.length) {
                                this.active = true;

                                // if the workflow status is rejected, the original author should be able to edit and resubmit
                                const currentTask = resp.items[resp.items.length - 1];
                                this.rejected = currentTask.cssStatus === 'rejected';

                                // if the task has been rejected and the current user requested the change, let them edit
                                this.isChangeAuthor = currentTask.requestedById === user.id;
                                this.userCanEdit = this.rejected && this.isChangeAuthor;

                                checkUserAccess(currentTask);
                            } else {
                                this.active = false;
                                setButtons();
                            }
                        }, () => { });
                }

                // check if the node is included in the workflow model
                workflowGroupsResource.get()
                    .then(groups => {
                        const nodePerms = workflowResource.checkNodePermissions(groups, editorState.current.id, editorState.current.contentTypeAlias);
                        const ancestorPerms = workflowResource.checkAncestorPermissions(editorState.current.path, groups);

                        if (nodePerms.approvalPath.length ||
                            nodePerms.contentTypeApprovalPath.length ||
                            ancestorPerms.length) {

                            workflowConfigured = true;
                            getPendingTasks();
                        } else {
                            workflowConfigured = false;
                            this.buttonGroup = defaultButtons;
                        }
                    });
            }
        }

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

        // preview should not save, if the content is in a workflow
        this.preview = content => {
            // Chromes popup blocker will kick in if a window is opened 
            // outwith the initial scoped request. This trick will fix that.
            const previewWindow = $window.open(`preview/?id=${content.id}`, 'umbpreview');
            // Build the correct path so both /#/ and #/ work.
            const redirect = Umbraco.Sys.ServerVariables.umbracoSettings.umbracoPath + '/preview/?id=' + content.id;
            previewWindow.location.href = redirect;
        }

        // it all starts here
        userService.getCurrentUser()
            .then(userResp => {
                user = userResp;
                getNodeTasks();
            });
    }

    // register controller 
    angular.module('umbraco').controller('Workflow.DrawerButtons.Controller',
        ['$scope',
            '$rootScope',
            '$timeout',
            '$window',
            'userService',
            'plmbrWorkflowResource',
            'plmbrGroupsResource',
            'plmbrActionsService',
            'contentEditingHelper',
            'angularHelper',
            'contentResource',
            'editorState',
            '$routeParams',
            'notificationsService', controller]);
}());

