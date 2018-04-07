(function () {
    'use strict';

    function dashboardController($scope, $rootScope, $routeParams, workflowResource, authResource, notificationsService) {

        var vm = this;

        function init() {
            getPending();
            getSubmissions();
            getAdmin();
        }

        // dash needs notification of when to refresh, as the action is in a deeper scope
        $rootScope.$on('refreshWorkflowDash', function () {
            init();
        });

        function getPending() {
            // api call for tasks assigned to the current user
            workflowResource.getApprovalsForUser(vm.currentUser.id, vm.taskPagination.perPage, vm.taskPagination.pageNumber)
                .then(function (resp) {
                    vm.tasks = resp.items;
                    vm.taskPagination.pageNumber = resp.page;
                    vm.taskPagination.totalPages = resp.total / resp.count;
                    vm.loaded[0] = true;
                }, function (err) {
                    notify(err);
                });
        }

        function getSubmissions() {
            // api call for tasks created by the current user
            workflowResource.getSubmissionsForUser(vm.currentUser.id, vm.submissionPagination.perPage, vm.submissionPagination.pageNumber)
                .then(function (resp) {
                    vm.submissions = resp.items;
                    vm.submissionPagination.pageNumber = resp.page;
                    vm.submissionPagination.totalPages = resp.total / resp.count;
                    vm.loaded[1] = true;
                }, function (err) {
                    notify(err);
                });
        }

        function getAdmin() {
            // if the current user is in an admin group, display all active tasks
            if (vm.adminUser) {
                workflowResource.getPendingTasks(vm.adminPagination.perPage, vm.adminPagination.pageNumber)
                    .then(function (resp) {
                        vm.activeTasks = resp.items;
                        vm.adminPagination.pageNumber = resp.page;
                        vm.adminPagination.totalPages = resp.totalPages;
                        vm.loaded[2] = true;
                    }, function (err) {
                        notify(err);
                    });
            }
        }

        function goToPage(i) {
            vm.pagination.pageNumber = i;
        }

        // display notification after actioning workflow task
        function notify(d) {
            if (d.status === 200) {
                notificationsService.success('SUCCESS!', d.message);
                init();
            }
            else {
                notificationsService.error('OH SNAP!', d.message);
            }
        }

        // expose some bits
        angular.extend(vm, {
            tasks: [],
            submissions: [],
            activeTasks: [],
            loaded: [false, false, false],
            goToPage: goToPage,

            taskPagination: {
                pageNumber: 1,
                totalPages: 0,
                perPage: 5,
                goToPage: function (i) {
                    vm.taskPagination.pageNumber = i;
                    getPending();
                }
            },

            submissionPagination: {
                pageNumber: 1,
                totalPages: 0,
                perPage: 5,
                goToPage: function (i) {
                    vm.submissionPagination.pageNumber = i;
                    getSubmissions();
                }
            },

            adminPagination: {
                pageNumber: 1,
                totalPages: 0,
                perPage: 10,
                goToPage: function (i) {
                    vm.adminPagination.pageNumber = i;
                    getAdmin();
                }
            }
        });

        // kick it all off
        authResource.getCurrentUser()
            .then(function (user) {
                vm.currentUser = user;
                vm.adminUser = user.allowedSections.indexOf('workflow') !== -1;
                init();
            });
    }

    // register controller 
    angular.module('umbraco').controller('Workflow.UserDashboard.Controller',
        ['$scope', '$rootScope', '$routeParams', 'plmbrWorkflowResource', 'authResource', 'notificationsService', dashboardController]);
}());