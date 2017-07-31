(function () {
    'use strict';

    // create controller 
    function controller($scope, $rootScope, userService, workflowResource, workflowActionsService, contentEditingHelper, contentResource, editorState, $routeParams) {
        var vm = this;

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

        var saveAndPublish = defaultButtons.defaultButton.labelKey === 'buttons_saveAndPublish';

        function getNodeTasks() {
            workflowResource.getNodePendingTasks(editorState.current.id)
                .then(function (resp) {
                    if (resp.items.length) {
                        vm.active = true;
                        checkUserAccess(resp.items[0]);
                    }
                    else {
                        vm.active = false;
                        setButtons();
                    }
                }, function (err) {

                });
        }

        // must be a better way of doing this - need to watch the editor state to dynamically change buttons
        $scope.$watch('$parent.$parent.$parent.contentForm.$dirty', function (newVal, oldVal) {
            $scope.dirty = newVal === true;
            setButtons();
        });

        $rootScope.$on('workflowActioned', function () {
            getNodeTasks();
        });

        function setButtons() {
            
            var subButtons = saveAndPublish ? [buttons.unpublishButton, defaultButtons.defaultButton, buttons.saveButton] : [buttons.unpublishButton, buttons.saveButton];

            vm.buttonGroup = {
                defaultButton: $scope.dirty ? buttons.saveButton : buttons.publishButton,
                subButtons: $scope.dirty ? (saveAndPublish ? [defaultButtons.defaultButton] : []) : subButtons
            };
        }

        function checkUserAccess(task) {
            vm.task = task;
            vm.adminUser = user.allowedSections.indexOf('workflow') !== -1;
            var currentTaskUsers = task.permissions[task.currentStep].userGroup.usersSummary;

            if (currentTaskUsers.indexOf('|' + user.id + '|') !== -1) {
                vm.canAction = true;
            }

            if (vm.active) {
                vm.buttonGroup = {
                    defaultButton: vm.adminUser || vm.canAction ? buttons.cancelButton : buttons.approveButton,
                    subButtons: vm.adminUser || vm.canAction ? [] : [buttons.rejectButton, buttons.cancelButton]
                };
            }
        }

        var buttons = {
            approveButton: {
                labelKey: "workflow_approveButtonLong",
                handler: function (item) {
                    vm.workflowOverlay = workflowActionsService.action(item, true);
                }
            },
            cancelButton: {
                labelKey: "workflow_cancelButtonLong",
                cssClass: 'danger',
                handler: function (item) {
                    vm.workflowOverlay = workflowActionsService.cancel(item);
                }
            },
            rejectButton: {
                labelKey: "workflow_rejectButton",
                cssClass: 'warning',
                handler: function (item) {
                    vm.workflowOverlay = workflowActionsService.action(item, false);
                }
            },
            saveButton: {
                labelKey: "workflow_saveButton",
                cssClass: 'success',
                handler: function (item) {
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
                labelKey: "workflow_publishButton",
                cssClass: 'success',
                handler: function (item) {
                    vm.workflowOverlay = workflowActionsService.initiate(editorState.current.name, editorState.current.id, true);
                }
            },
            unpublishButton: {
                labelKey: "workflow_unpublishButton",
                cssClass: 'warning',
                handler: function (item) {
                    vm.workflowOverlay = workflowActionsService.initiate(editorState.current.name, editorState.current.id, false);
                }
            }
        };

        var user;
        userService.getCurrentUser()
            .then(function (userResp) {
                user = userResp;
                getNodeTasks();
            });

        angular.extend(vm, {
            active: false
        });
    }

    // register controller 
    angular.module('umbraco').controller('Workflow.DrawerButtons.Controller', controller);
}());

