(function () {
    'use strict';

    function editController($scope, $routeParams, $location, workflowGroupsResource, workflowResource, notificationsService, contentResource, navigationService) {

        const getContentTypes = () => {

            this.nodePermissions = this.group.permissions.filter(v => v.nodeId);
            this.docPermissions = this.group.permissions.filter(v => v.contentTypeId);

            if (this.nodePermissions.length) {
                contentResource.getByIds(this.nodePermissions.map(v => v.nodeId))
                    .then(resp => {
                        resp.forEach(v => {
                            this.nodePermissions.forEach(p => {
                                if (p.nodeId === v.id) {
                                    p.icon = v.icon;
                                    p.path = v.path;
                                    p.name = v.name + ' - stage ' + (p.permission + 1);
                                }
                            });
                        });
                    });
            }

            if (this.docPermissions.length) {
                workflowResource.getContentTypes()
                    .then(resp => {
                        resp.forEach(v => {
                            this.docPermissions.forEach(p => {
                                if (p.contentTypeId === v.id) {
                                    p.icon = v.icon;
                                    p.path = v.path;
                                    p.name = v.name + ' - stage ' + (p.permission + 1);
                                }
                            });
                        });
                    });
            }
        }

        // history tab
        const getHistory = () => {
            workflowResource.getAllTasksForGroup($routeParams.id, this.pagination.perPage, this.pagination.pageNumber)
                .then(resp => {
                    this.tasks = resp.items;
                    this.pagination.pageNumber = resp.page;
                    this.pagination.totalPages = resp.totalPages;
                });
        }

        this.editDocTypePermission = () => {
            $location.path('/workflow/workflow/settings/info');
        }

        this.perPage = () => [2, 5, 10, 20, 50];

        // todo -> Would be sweet to open the config dialog from here, rather than just navigating to the node...
        this.editContentPermission = id => {
            navigationService.changeSection('content');
            $location.path(`/content/content/edit/${id}`);
        }

        /**
         * Remove a user from the group
         * @param {any} id
         */
        this.remove = id => {
            var index;
            this.group.users.forEach((u, i) => {
                if (u.userId === id) {
                    index = i;
                }
            });

            this.group.users.splice(index, 1);
        }

        /**
         * Open the picker to add a new user to the group
         */
        this.openUserPicker = () => {
            this.userPicker = {
                view: '../app_plugins/workflow/backoffice/dialogs/workflow.userpicker.overlay.html',
                selection: this.group.users,
                show: true,
                submit: model => {
                    this.userPicker.show = false;
                    this.userPicker = null;

                    this.group.users = [];

                    model.selection.forEach(u => {
                        this.group.users.push({ userId: u.userId || u.id, groupId: this.group.groupId, name: u.name });
                    });
                },
                close: () => {
                    this.userPicker.show = false;
                    this.userPicker = null;
                }
            };
        }

        /**
         * Save the group and show appropriate notifications
         */
        this.save = () => {
            workflowGroupsResource.save(this.group)
                .then(resp => {
                    if (resp.status === 200) {
                        notificationsService.success('SUCCESS', resp.msg);
                        $scope.approvalGroupForm.$setPristine();
                    } else {
                        notificationsService.error('ERROR', resp.msg);
                    }
                }, err => {
                    notificationsService.error('ERROR', err);
                });
        }

        /**
         * Fetch the group by the given id, or create an empty model if the id is -1 (ie a new group - id doesn't exist until saving)
         */
        const init = () => {
            this.loaded = false;

            if ($routeParams.id !== '-1') {
                workflowGroupsResource.get($routeParams.id)
                    .then(resp => {
                        this.group = resp;
                        this.name = $routeParams.id !== '-1' ? 'Edit ' : `Create ${resp.name}`;

                        if (this.group.permissions) {
                            getContentTypes();
                        }

                        this.loaded = true;
                    });
            } else {
                this.group = {
                    groupId: -1,
                    name: '',
                    description: '',
                    alias: '',
                    groupEmail: '',
                    users: [],
                    usersSummary: ''
                };

                this.loaded = true;
            }
        }

        // declare scoped variables
        this.tabs =
            [
                {
                    id: 0,
                    label: 'Group detail',
                    alias: 'tab0',
                    active: true
                },
                {
                    id: 1,
                    label: 'Activity history',
                    alias: 'tab1',
                    active: false
                }
            ];

        this.pagination = {
            pageNumber: 1,
            totalPages: 0,
            perPage: 10,
            goToPage: (i) => {
                this.pagination.pageNumber = i;
                getHistory();
            }
        };

        // get the data
        init();
        getHistory();
    }

    angular.module('umbraco').controller('Workflow.Groups.Edit.Controller',
        ['$scope',
            '$routeParams',
            '$location',
            'plmbrGroupsResource',
            'plmbrWorkflowResource',
            'notificationsService',
            'contentResource',
            'navigationService', editController]);
}());

