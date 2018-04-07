(function () {
    'use strict';

    // create controller 
    // since this controller is loaded in response to an injector match, we can use it to check for active workflow groups 
    // and display a message if none are configured, while also displaying the default button set
    function controller($scope, $rootScope, $window, userService, workflowResource, workflowActionsService, contentEditingHelper, angularHelper, contentResource, editorState, $routeParams, notificationsService) {
        var vm = this,
            user;

        var dashboardClick = editorState.current === null;

        var defaultButtons = contentEditingHelper.configureContentEditorButtons({
            create: $routeParams.create,
            content: $scope.content,
            methods: {
                saveAndPublish: $scope.saveAndPublish,
                sendToPublish: $scope.sendToPublish,
                save: $scope.save,
                unPublish: angular.noop
            }
        });

        var saveAndPublish = defaultButtons.defaultButton && defaultButtons.defaultButton.labelKey === 'buttons_saveAndPublish';

        var buttons = {
            approveButton: {
                labelKey: 'workflow_approveButtonLong',
                handler: function (item) {
                    vm.workflowOverlay = workflowActionsService.action(item, 'Approve', dashboardClick);
                }
            },
            cancelButton: {
                labelKey: 'workflow_cancelButtonLong',
                cssClass: 'danger',
                handler: function (item) {
                    vm.workflowOverlay = workflowActionsService.cancel(item, dashboardClick);
                }
            },
            rejectButton: {
                labelKey: 'workflow_rejectButton',
                cssClass: 'warning',
                handler: function (item) {
                    vm.workflowOverlay = workflowActionsService.action(item, 'Reject', dashboardClick);
                }
            },
            resubmitButton: {
                labelKey: 'workflow_resubmitButton',
                handler: function (item) {
                    vm.workflowOverlay = workflowActionsService.action(item, 'Resubmit', dashboardClick);
                }
            },
            detailButton: {
                labelKey: 'workflow_detailButton',
                handler: function (item) {
                    vm.workflowOverlay = workflowActionsService.detail(item);
                }
            },
            saveButton: {
                labelKey: 'workflow_saveButton',
                cssClass: 'success',
                handler: function () {
                    workflowActionsService.buttonState('busy', editorState.current.id);
                    contentEditingHelper.contentEditorPerformSave({
                        statusMessage: 'Saving...',
                        saveMethod: contentResource.save,
                        scope: $scope,
                        content: editorState.current
                    }).then(function (resp) {
                        workflowActionsService.buttonState(
                            resp.notifications && resp.notifications[0].type === 3 ? 'success' : 'error', editorState.current.id);
                    });
                }
            },
            publishButton: {
                labelKey: 'workflow_publishButton',
                cssClass: 'success',
                handler: function () {
                    vm.workflowOverlay = workflowActionsService.initiate(editorState.current.name, editorState.current.id, true);
                }
            },
            unpublishButton: {
                labelKey: 'workflow_unpublishButton',
                cssClass: 'warning',
                handler: function () {
                    vm.workflowOverlay = workflowActionsService.initiate(editorState.current.name, editorState.current.id, false);
                }
            }
        };

        // are there common elements between two arrays?
        function common(arr1, arr2) {
            return arr1.some(function (el) {
                return arr2.indexOf(el) > -1;
            });
        }


        // fetch settings to check node exclusion stat
        workflowResource.getSettings()
            .then(function (settings) {
                if (settings && settings.excludeNodes) {
                    var exclude = settings.excludeNodes.split(',');
                    // if any elements are shared, exclude the node from the workflow mechanism
                    // by checking the path not just the id, this becomes recursive, and the excludeNodes cascades down the tree
                    if (common(editorState.current.path.split(','), exclude)) {
                        vm.excludeNode = true;
                    }
                }
            });

        function getNodeTasks() {
            // only refresh if viewing a content node
            if (editorState.current) {
                workflowResource.getNodePendingTasks(editorState.current.id)
                    .then(function (resp) {
                        if ((resp.noFlow || resp.settings) && !editorState.current.trashed) {
                            var msg = resp.noFlow ?
                                'No workflow groups have been configured - refer to the documentation tab in the Workflow section, then set at minimum an approval flow on the homepage node or document type.' :
                                'Workflow settings are configured incorrectly - refer to the documentation tab in the Workflow section.';
                            notificationsService.warning('WORKFLOW INSTALLED BUT NOT CONFIGURED', msg);
                        } else if (resp.items && resp.items.length) {
                            vm.active = true;

                            // if the workflow status is rejected, the original author should be able to edit and resubmit
                            var currentTask = resp.items[resp.items.length - 1];
                            vm.rejected = currentTask.cssStatus === 'rejected';

                            // if the task has been rejected and the current user requested the change, let them edit
                            vm.isChangeAuthor = currentTask.requestedById === user.id;
                            vm.userCanEdit = vm.rejected && vm.isChangeAuthor;

                            checkUserAccess(currentTask);
                        } else {
                            vm.active = false;
                            setButtons();
                        }

                    },
                    function () {

                    });
            }
        }

        // use this to ensure changes are saved when submitting for publish
        // event is broadcast from the buttons directive, which watches the content form
        var dirty = false;
        $rootScope.$on('contentFormDirty', function (event, data) {
            dirty = data;
            setButtons();
        });

        // ensures dash/buttons refresh
        $rootScope.$on('workflowActioned', function () {
            getNodeTasks();
        });

        /**
         * any user with access to the workflow section will be able to action workflows ie cancel outside their group membership
         * @param {any} task
         */
        function checkUserAccess(task) {
            vm.task = task || vm.task;
            vm.canAction = false;

            vm.adminUser = user.allowedSections.indexOf('workflow') !== -1;
            var currentTaskUsers = vm.task.permissions[vm.task.currentStep].userGroup.usersSummary;

            if (currentTaskUsers.indexOf('|' + user.id + '|') !== -1) {
                vm.canAction = true;
            }

            if (vm.active) {

                vm.buttonGroup = {};

                if (dirty && vm.userCanEdit) {
                    vm.buttonGroup.defaultButton = buttons.saveButton;
                }
                // primary button is approve when the user is in the approving group and task is not rejected
                else if (vm.canAction && !vm.rejected) {
                    vm.buttonGroup .defaultButton = buttons.approveButton;
                } else if (vm.userCanEdit) { // rejected tasks show the resubmit, only when the user is the original author
                    vm.buttonGroup.defaultButton = buttons.resubmitButton;
                } else { // all other cases see the detail button
                    vm.buttonGroup.defaultButton = buttons.detailButton;
                }

                vm.buttonGroup.subButtons = [];

                // if the user is in the approving group, and the task is not rejected, add reject to sub buttons
                if (vm.canAction && !vm.rejected) {
                    vm.buttonGroup.subButtons.push(buttons.rejectButton);
                } 
                // if the user is admin, the change author or in the approving group for a non-rejected task, add the cancel button
                if (vm.isAdmin || vm.userCanEdit || vm.isChangeAuthor || (vm.canAction && !vm.rejected)) {
                    vm.buttonGroup.subButtons.push(buttons.cancelButton);
                }
                

            }
        }

        /**
         * Manages the default states for the buttons - updates when no active task, or when the content form is dirtied
         */
        function setButtons() {
            // default button will be null when the current user has browse-only permission
            if (defaultButtons.defaultButton !== null && !vm.active) {
                var subButtons = saveAndPublish ? [buttons.unpublishButton, defaultButtons.defaultButton, buttons.saveButton] : [buttons.unpublishButton, buttons.saveButton];
                // if the content is dirty, show save. otherwise show request approval
                vm.buttonGroup = {
                    defaultButton: dirty ? buttons.saveButton : buttons.publishButton,
                    subButtons: dirty ? (saveAndPublish ? [defaultButtons.defaultButton] : []) : subButtons
                };
            }

            // if a task is active, the default buttons should be updated to match the current user's access/role in the workflow
            if (vm.active) {
                checkUserAccess();
            }
        }

        // preview should not save, if the content is in a workflow
        function preview(content) {
            // Chromes popup blocker will kick in if a window is opened 
            // outwith the initial scoped request. This trick will fix that.
            var previewWindow = $window.open('preview/?id=' + content.id, 'umbpreview');
            // Build the correct path so both /#/ and #/ work.
            var redirect = Umbraco.Sys.ServerVariables.umbracoSettings.umbracoPath + '/preview/?id=' + content.id;
            previewWindow.location.href = redirect;
        }

        // it all starts here
        userService.getCurrentUser()
            .then(function (userResp) {
                user = userResp;
                getNodeTasks();
            });

        angular.extend(vm, {
            active: false,
            excludeNode: false,
            buttonGroupState: 'init',
            preview: preview
        });
    }

    // register controller 
    angular.module('umbraco').controller('Workflow.DrawerButtons.Controller',
        ['$scope',
        '$rootScope',
        '$window',
        'userService',
        'plmbrWorkflowResource',
        'plmbrActionsService',
        'contentEditingHelper',
        'angularHelper',
        'contentResource',
        'editorState',
        '$routeParams',
        'notificationsService', controller]);
}());

