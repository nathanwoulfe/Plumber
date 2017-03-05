(function () {
    'use strict';

    function TasksDirective(dialogService, notificationsService) {

        var directive = {
            restrict: 'AEC',
            scope: {
                heading: '=',
                items: '=',
                editLink: '=',
                loaded: '=',
                type: '='
            },
            templateUrl: '../app_plugins/workflow/backoffice/partials/workflowTasksTemplate.html',
            controller: function ($scope) {

                function showDialog(url, item, cb) {
                    dialogService.open({
                        template: url,
                        show: true,
                        dialogData: item,
                        callback: function(resp) {
                            if (cb) {
                                if (resp.status === 200) {
                                    notificationsService.success("SUCCESS!", resp.data.Message);
                                }
                                else {
                                    notificationsService.error("OH SNAP!", resp.data.Message);
                                }

                                $scope.$parent.vm.init();
                            }
                        }
                    });
                };

                // type = 0, 1
                // 0 -> full button set
                // 1 -> cancel, edit

                var buttons = {
                    approveButton: {
                        labelKey: "workflow_approveButton",
                        handler: function (item) {
                            showDialog('../app_plugins/workflow/backoffice/dialogs/workflow.approve.dialog.html', item, true);
                        }
                    },
                    editButton: {
                        labelKey: "workflow_editButton",
                        href: '/umbraco/#/content/content/edit/',
                        handler: function () {
                        }
                    },
                    cancelButton: {
                        labelKey: "workflow_cancelButton",
                        cssClass: 'danger',
                        handler: function (item) {
                            showDialog('../app_plugins/workflow/backoffice/dialogs/workflow.cancel.dialog.html', item, true);
                        }
                    },                
                    rejectButton: {
                        labelKey: "workflow_rejectButton",
                        cssClass: 'warning',
                        handler: function (item) {
                            showDialog('../app_plugins/workflow/backoffice/dialogs/workflow.reject.dialog.html', item, true);
                        }
                    },
                    diffsButton: {
                        labelKey: "workflow_diffsButton",
                        handler: function (item) {
                            showDialog('../app_plugins/workflow/backoffice/dialogs/workflow.differences.dialog.html', item);
                        }
                    }
                };

                var subButtons = [
                    [buttons.editButton, buttons.rejectButton, buttons.cancelButton],
                    [buttons.editButton]
                ]

                $scope.buttonGroup = {
                    defaultButton: $scope.type === 0 ? buttons.approveButton : buttons.cancelButton,
                    subButtons: subButtons[$scope.type]
                };            
            },
            link: function (scope, element, attrs) {

                /******** PAGING *******/
                scope.numPerPage = 10;
            }
        };

        return directive;
    }

    angular.module('umbraco.directives').directive('wfTasks', TasksDirective);

}());