(() => {
    function tasks($location, workflowActionsService) {

        const directive = {
            restrict: 'AEC',
            scope: {
                items: '=',
                type: '=',
                loaded: '='
            },
            templateUrl: '../app_plugins/workflow/backoffice/views/partials/workflowTasksTemplate.html',
            controller: function($scope) {

                // type = 0, 1
                // 0 -> full button set
                // 1 -> cancel, edit - this is reversed if the task is rejected
                // 2 -> no buttons

                $scope.detail = item => {
                    $scope.$parent.vm.workflowOverlay = workflowActionsService.detail(item);
                };

                const buttons = {
                    approveButton: {
                        labelKey: 'workflow_approveButton',
                        handler: item => {
                            $scope.$parent.vm.workflowOverlay = workflowActionsService.action(item, 'Approve', true);
                        }
                    },
                    editButton: {
                        labelKey: 'workflow_editButton',
                        handler: item => {
                            $location.path(`/content/content/edit/${item.nodeId}`);
                        }
                    },
                    cancelButton: {
                        labelKey: 'workflow_cancelButton',
                        cssClass: 'danger',
                        handler: item => {
                            $scope.$parent.vm.workflowOverlay = workflowActionsService.cancel(item, true);
                        }
                    },
                    rejectButton: {
                        labelKey: 'workflow_rejectButton',
                        cssClass: 'warning',
                        handler: item => {
                            $scope.$parent.vm.workflowOverlay = workflowActionsService.action(item, 'Reject', true);
                        }
                    }
                };

                const subButtons = [
                    [buttons.editButton, buttons.rejectButton, buttons.cancelButton],
                    [buttons.editButton],
                    [buttons.cancelButton]
                ];

                if ($scope.type !== 2) {
                    $scope.buttonGroup = {
                        defaultButton: $scope.type === 0 ? buttons.approveButton : buttons.cancelButton,
                        subButtons: subButtons[$scope.type]
                    };
                } else {
                    $scope.noActions = true;
                }

                // when the items arrive, if a task was rejected
                // flip the order of the cancel and edit buttons
                $scope.$watch('items',
                    newVal => {
                        if (newVal && newVal.length && $scope.type === 0) {
                            $scope.items.forEach(i => {
                                if (i.cssStatus === 'rejected') {
                                    $scope.buttonGroup.defaultButton = buttons.editButton;
                                    $scope.buttonGroup.subButtons = [buttons.cancelButton];
                                }
                            });
                        }
                    });
            }
        };

        return directive;
    }

    angular.module('umbraco.directives').directive('wfTasks', ['$location', 'plmbrActionsService', tasks]);

})();
