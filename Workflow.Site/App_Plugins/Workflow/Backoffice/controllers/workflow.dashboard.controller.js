(function () {
    'use strict';

    function dashboardController($scope, $routeParams, workflowResource, authResource, notificationsService) {

        var vm = this;
        vm.loaded = [0, 0, 0];
        $scope.perPage = 10;

        authResource.getCurrentUser()
            .then(function (user) {
                vm.currentUser = user;
                vm.adminUser = user.userType.indexOf('admin') !== -1;
                init();
            });

        function init() {
            // api call for tasks assigned to the current user
            workflowResource.getApprovalsForUser(vm.currentUser.id)
                .then(function (resp) {
                    vm.tasks = resp;
                    vm.loaded[0] = 1;
                }, function (err) {
                    console.log(err);
                });

            // api call for tasks created by the current user
            workflowResource.getSubmissionsForUser(vm.currentUser.id)
                .then(function (resp) {
                    vm.submissions = resp;
                    vm.loaded[1] = 1;
                });

            // if the current user is in an admin group, display all active tasks
            if (vm.adminUser) {
                workflowResource.getPendingTasks()
                    .then(function (resp) {
                        vm.activeTasks = resp;
                        vm.loaded[2] = 1;
                    });
            }
        };

        // listen for clicks on the cancel button
        $scope.$on('workflow-cancel', function (e, item) {
            vm.workflowOverlay = {
                view: '../app_plugins/workflow/backoffice/dialogs/workflow.cancel.dialog.html',
                show: true,
                title: 'Cancel workflow process',
                subtitle: 'Document: ' + item.nodeName,
                comment: '',
                isFinalApproval: item.activeTask === 'Pending Final Approval',
                submit: function (model) {
                    workflowResource.cancelWorkflowTask(item.taskId, model.comment)
                        .then(function (resp) {
                            notify(resp);
                            vm.workflowOverlay.close();
                        });
                },
                close: function (model) {
                    vm.workflowOverlay.show = false;
                    vm.workflowOverlay = null;
                }
            };
        });

        // listen for clicks on the approve or reject button
        $scope.$on('workflow-action', function (e, args) {
            vm.workflowOverlay = {
                view: '../app_plugins/workflow/backoffice/dialogs/workflow.action.dialog.html',
                show: true,
                title: (args.approve ? 'Approve' : 'Reject') + ' workflow process',
                subtitle: 'Document: ' + args.item.nodeName,
                comment: args.item.comments,
                approvalComment: '',
                requestedBy: args.item.requestedBy,
                requestedOn: args.item.requestedOn,
                submit: function (model) {
                    if (args.approve) {
                        workflowResource.approveWorkflowTask(args.item.taskId, model.comment)
                            .then(function (resp) {
                                notify(resp);
                            });
                    }
                    else {
                        workflowResource.rejectWorkflowTask(args.item.taskId, model.comment)
                            .then(function (resp) {
                                notify(resp);
                            });
                    }
                    vm.workflowOverlay.close();
                },
                close: function (model) {
                    vm.workflowOverlay.show = false;
                    vm.workflowOverlay = null;
                }
            };
        });

        // display notification after actioning workflow task
        function notify(d) {
            if (d.status === 200) {
                notificationsService.success("SUCCESS!", d.message);
            }
            else {
                notificationsService.error("OH SNAP!", d.message);
            }

            init();
        }        

        // expose some bits
        angular.extend(vm, {
            tasks: [],
            submissions: [],
            activeTasks: []
        });
    }

    // register controller 
    angular.module('umbraco').controller('Workflow.Dashboard.Controller', dashboardController);
}());