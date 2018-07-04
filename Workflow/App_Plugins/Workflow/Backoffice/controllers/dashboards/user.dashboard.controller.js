(() => {
    'use strict';

    function dashboardController($scope, $rootScope, $routeParams, workflowResource, authResource, notificationsService, plumberHub) {

        let notify = null;

        const getPending = () => {
            // api call for tasks assigned to the current user
            workflowResource.getApprovalsForUser(this.currentUser.id,
                    this.taskPagination.perPage,
                    this.taskPagination.pageNumber)
                .then(resp => {
                        this.tasks = resp.items;
                        this.taskPagination.pageNumber = resp.page;
                        this.taskPagination.totalPages = resp.total / resp.count;
                        this.loaded[0] = true;
                    },
                    err => {
                        notify(err);
                    });
        };

        const getSubmissions = () => {
            // api call for tasks created by the current user
            workflowResource.getSubmissionsForUser(this.currentUser.id,
                    this.submissionPagination.perPage,
                    this.submissionPagination.pageNumber) 
                .then(resp => {
                        this.submissions = resp.items; 
                        this.submissionPagination.pageNumber = resp.page;
                        this.submissionPagination.totalPages = resp.total / resp.count;
                        this.loaded[1] = true;
                    },
                    err => {
                        notify(err);
                    });
        };

        const getAdmin = () => {
            // if the current user is in an admin group, display all active tasks
            if (this.adminUser) {
                workflowResource.getPendingTasks(this.adminPagination.perPage, this.adminPagination.pageNumber)
                    .then(resp => {
                            this.activeTasks = resp.items;
                            this.adminPagination.pageNumber = resp.page;
                            this.adminPagination.totalPages = resp.totalPages;
                            this.loaded[2] = true;
                        },
                        err => {
                            notify(err);
                        });
            }
        };

        const goToPage = i => {
            this.pagination.pageNumber = i;
        };

        const init = () => {
            getPending();
            getSubmissions();
            getAdmin();
        };

        // dash needs notification of when to refresh, as the action is in a deeper scope
        $rootScope.$on('refreshWorkflowDash',
            () => {
                init();
            });

        // display notification after actioning workflow task
        notify = d => {
            if (d.status === 200) {
                notificationsService.success('SUCCESS!', d.message);
                init();
            } else {
                notificationsService.error('OH SNAP!', d.message);
            }
        };

        plumberHub.initHub(hub => {
            hub.on('workflowStarted', data => {
                debugger;
                console.log(data);
            });

            hub.on('taskApproved', data => {
                debugger;
                console.log(data);
            });

            hub.on('taskCancelled', data => {
                debugger;
                console.log(data);
            });

            hub.on('taskResubmitted', data => {
                debugger;
                console.log(data);
            });

            hub.on('taskRejected', data => {
                debugger;
                console.log(data);
            });

            hub.start();
        });

        // expose some bits
        angular.extend(this,
            {
                tasks: [],
                submissions: [],
                activeTasks: [],
                loaded: [false, false, false],
                goToPage: goToPage,

                taskPagination: {
                    pageNumber: 1,
                    totalPages: 0,
                    perPage: 5,
                    goToPage: i => {
                        this.taskPagination.pageNumber = i;
                        getPending();
                    }
                },

                submissionPagination: {
                    pageNumber: 1,
                    totalPages: 0,
                    perPage: 5,
                    goToPage: i => {
                        this.submissionPagination.pageNumber = i;
                        getSubmissions();
                    }
                },

                adminPagination: {
                    pageNumber: 1,
                    totalPages: 0,
                    perPage: 10,
                    goToPage: i => {
                        this.adminPagination.pageNumber = i;
                        getAdmin();
                    }
                }
            });

        // kick it all off
        authResource.getCurrentUser()
            .then(user => {
                this.currentUser = user;
                this.adminUser = user.allowedSections.indexOf('workflow') !== -1;
                init();
            });
    }

    // register controller 
    angular.module('umbraco').controller('Workflow.UserDashboard.Controller',
        ['$scope', '$rootScope', '$routeParams', 'plmbrWorkflowResource', 'authResource', 'notificationsService', 'plumberHub', dashboardController]);
})();