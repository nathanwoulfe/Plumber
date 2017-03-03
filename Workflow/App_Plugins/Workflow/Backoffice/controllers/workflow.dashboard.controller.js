(function () {
    'use strict';

    function Controller($scope, $routeParams, workflowResource, authResource) {

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

        angular.extend(vm, {
            tasks: [],
            submissions: [],
            activeTasks: []
        });
    }

    // register controller 
    angular.module('umbraco').controller('Workflow.Dashboard.Controller', Controller);
}());