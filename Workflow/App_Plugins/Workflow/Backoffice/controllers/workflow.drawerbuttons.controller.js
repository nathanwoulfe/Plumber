(function () {
    'use strict';

    // create controller 
    // since this controller is loaded in response to an injector match, we can use it to check for active workflow groups 
    // and display a message if none are configured, while also displaying the default button set
    function controller($scope, $rootScope, $window, userService, workflowResource, workflowActionsService, contentEditingHelper, contentResource, editorState, $routeParams, notificationsService) {
        var vm = this,
            user;

        // are there common elements between two arrays?
        function common(arr1, arr2) {
            return arr1.some(function(el) {
                return arr2.indexOf(el) > -1;
            });
        }

        workflowResource.getSettings()
            .then(function(settings) {
                if (settings && settings.excludeNodes) {
                    var exclude = settings.excludeNodes.split(',');
                    // if any elements are shared, exclude the node from the workflow mechanism
                    // by checking the path not just the id, this becomes recursive, and the excludeNodes cascades down the tree
                    if (common(editorState.current.path.split(','), exclude)) {
                        vm.excludeNode = true;
                    }
                }
            });


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

        function getNodeTasks() {
            workflowResource.getNodePendingTasks(editorState.current.id)
                .then(function (resp) {
                    if (resp.noFlow || resp.settings) {
                        var msg = resp.noFlow
                            ? 'No workflow groups have been configured - refer to the documentation tab in the Workflow section, then set at minimum an approval flow on the homepage node or document type.'
                            : 'Workflow settings are configured incorrectly - refer to the documentation tab in the Workflow section.';
                        notificationsService.warning('WORKFLOW INSTALLED BUT NOT CONFIGURED', msg);
                    } else if (resp.items && resp.items.length) {
                        vm.active = true;
                        checkUserAccess(resp.items[0]);
                    } else {
                        vm.active = false;
                        setButtons();
                    }
                }, function () {

                });
        }

        // use this to ensure changes are saved when submitting for publish
        $scope.dirty = false;
        $scope.$watch('$parent.$parent.$parent.contentForm.$dirty', function (newVal) {
            if (newVal !== $scope.dirty) {
                $scope.dirty = newVal === true;
                setButtons();
            }
        });

        $rootScope.$on('workflowActioned', function () {
            getNodeTasks();
        });

        var buttons = {
            approveButton: {
                labelKey: 'workflow_approveButtonLong',
                handler: function (item) {
                    vm.workflowOverlay = workflowActionsService.action(item, $scope.dirty, true);
                }
            },
            cancelButton: {
                labelKey: 'workflow_cancelButtonLong',
                cssClass: 'danger',
                handler: function (item) {
                    vm.workflowOverlay = workflowActionsService.cancel(item);
                }
            },
            rejectButton: {
                labelKey: 'workflow_rejectButton',
                cssClass: 'warning',
                handler: function (item) {
                    vm.workflowOverlay = workflowActionsService.action(item, false);
                }
            },
            saveButton: {
                labelKey: 'workflow_saveButton',
                cssClass: 'success',
                handler: function () {
                    contentEditingHelper.contentEditorPerformSave({
                        statusMessage: 'Saving...',
                        saveMethod: contentResource.save,
                        scope: $scope,
                        content: editorState.current
                    });
                    $scope.$parent.$parent.$parent.contentForm.$setPristine();
                }
            },
            publishButton: {
                labelKey: 'workflow_publishButton',
                cssClass: 'success',
                handler: function () {
                    vm.workflowOverlay = workflowActionsService.initiate(editorState.current.name, editorState.current.id, $scope.dirty, true);
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

        // any user with access to the workflow section will be able to action workflows ie cancel outside their group membership
        function checkUserAccess(task) {
            vm.task = task;
            vm.adminUser = user.allowedSections.indexOf('workflow') !== -1;
            var currentTaskUsers = task.permissions[task.currentStep].userGroup.usersSummary;

            if (currentTaskUsers.indexOf('|' + user.id + '|') !== -1) {
                vm.canAction = true;
            }
            if (vm.active) {
                vm.buttonGroup = {
                    defaultButton: vm.canAction ? buttons.approveButton : vm.adminUser ? buttons.cancelButton : null,
                    subButtons: vm.canAction ? [buttons.rejectButton, buttons.cancelButton] : []
                };
            }
        }

        function setButtons() {
            // default button will be null when the current user has browse-only permission
            if (defaultButtons.defaultButton !== null) {
                var subButtons = saveAndPublish ? [buttons.unpublishButton, defaultButtons.defaultButton, buttons.saveButton] : [buttons.unpublishButton, buttons.saveButton];

                vm.buttonGroup = {
                    defaultButton: $scope.dirty ? buttons.saveButton : buttons.publishButton,
                    subButtons: $scope.dirty ? (saveAndPublish ? [defaultButtons.defaultButton] : []) : subButtons
                };
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

        userService.getCurrentUser()
            .then(function (userResp) {
                user = userResp;
                getNodeTasks();
            });

        angular.extend(vm, {
            active: false,
            excludeNode: false,
            preview: preview
        });
    }

    // register controller 
    angular.module('umbraco').controller('Workflow.DrawerButtons.Controller', controller);
}());

