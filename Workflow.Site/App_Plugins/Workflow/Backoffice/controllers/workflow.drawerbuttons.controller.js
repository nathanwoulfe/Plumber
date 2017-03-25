(function () {
    'use strict';

    // create controller 
    function controller($routeParams, userService, userGroupsResource, workflowResource, workflowActionsService) {
        var vm = this,
            nodeId = $routeParams.id;

        workflowResource.getNodePendingTasks(nodeId)
            .then(function (resp) {
                if (resp.items.length) {
                    vm.active = true;
                    checkUserAccess(resp.items[0]);
                    addHeader();
                }
            }, function (err) {

            });

        function addHeader() {
            var elm = document.querySelector('.umb-editor-header'),
                banner = document.createElement('div').innerHTML = '<p>Something</p>';

            banner.setAttribute('class', 'active-banner');
            elm.parentNode.insertBefore(banner, elm.nextSibling);
        }

        function checkUserAccess(task) {
            userService.getCurrentUser()
                .then(function (user) {
                    vm.task = task;
                    vm.adminUser = user.allowedSections.indexOf('workflow') !== -1;
                    var currentTaskUsers = task.permissions[task.currentStep].userGroup.usersSummary;

                    if (currentTaskUsers.indexOf('|' + user.id + '|') !== -1) {
                        vm.canAction = true;
                    }

                    vm.buttonGroup = {
                        defaultButton: vm.adminUser ? buttons.cancelButton : buttons.approveButton,
                        subButtons: vm.adminUser ? [] : [buttons.rejectButton, buttons.cancelButton]
                    };
                });
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
            }
        };

        angular.extend(vm, {
            active: false,
            canAction: false            
        });
    }

    // register controller 
    angular.module('umbraco').controller('Workflow.DrawerButtons.Controller', controller);
}());

