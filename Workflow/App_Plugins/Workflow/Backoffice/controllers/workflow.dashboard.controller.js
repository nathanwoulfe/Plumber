
(function () {
    'use strict';

    // create controller 
    function Controller($scope, $routeParams, workflowResource, userGroupsResource, notificationsService, entityResource, dialogService, authResource) {

        var vm = this;

        authResource.getCurrentUser()
            .then(function (user) {
                vm.currentUser = user;
                vm.adminUser = user.userType.indexOf('admin') !== -1;
                init();
            });

        vm.loaded = [0, 0, 0];
        $scope.perPage = 10;

        function init() {
            // api call for tasks assigned to the current user
            workflowResource.getApprovalsForUser(vm.currentUser.id)
                .then(function (resp) {
                    vm.tasks = resp.data;
                    vm.loaded[0] = 1;
                });

            // api call for tasks created by the current user
            workflowResource.getSubmissionsForUser(vm.currentUser.id)
                .then(function (resp) {
                    vm.submissions = resp.data;
                    vm.loaded[1] = 1;
                });

            // if the current user is in an admin group, display all active tasks
            if (vm.adminUser) {
                workflowResource.getActiveTasks()
                    .then(function (resp) {
                        vm.activeTasks = resp.data;
                        vm.loaded[2] = 1;
                    });
            }
        };    

        function checkApprovalForStep(p, item) {
            console.log(p, item);

            return true;
        }

        function approveTask(task) {
            dialogService.open({
                template: '../app_plugins/workflow/backoffice/workflow/dialogs/approvedialog.html',
                show: true,
                dialogData: task,
                callback: workflowTaskDone
            });
        };

        function cancelTask(task) {
            dialogService.open({
                template: '../app_plugins/workflow/backoffice/workflow/dialogs/canceldialog.html',
                show: true,
                dialogData: task,
                callback: workflowTaskDone
            });
        };

        function showDifferences(task) {
            dialogService.open({
                template: '../app_plugins/workflow/backoffice/workflow/dialogs/differencesdialog.html',
                show: true,
                dialogData: task
            });
        };

        function workflowTaskDone(resp) {

            if (resp.status === 200) {
                notificationsService.success("SUCCESS!", resp.data.Message);
            }
            else {
                notificationsService.error("OH SNAP!", resp.data.Message);
            }

            init();
        }

        angular.extend(vm, {
            approveTask: approveTask,
            cancelTask: cancelTask,
            showDifferences: showDifferences,
            checkApprovalForStep: checkApprovalForStep,

            tasks: [],
            submissions: [],
            activeTasks: []
        });
    }

    // register controller 
    angular.module('umbraco').controller('Workflow.Dashboard.Controller', Controller);
}());