(function () {
    'use strict';

    function editController($scope, $routeParams, $location, $timeout, workflowGroupsResource, workflowResource, dialogService, entityResource, notificationsService, contentResource, navigationService) {

        var vm = this;

        // only fetch group if id is valid - otherwise it's a create action
        function init() {
            if ($routeParams.id !== '-1') {
                workflowGroupsResource.get($routeParams.id)
                    .then(function (resp) {
                        vm.group = resp;
                        vm.name = $routeParams.id !== '-1' ? 'Edit ' : 'Create ' + resp.name;

                        if (vm.group.permissions) {
                            getContentTypes();
                        }
                    });
            } else {
                vm.group = {
                    groupId: -1,
                    name: '',
                    description: '',
                    alias: '',
                    groupEmail: '',
                    users: [],
                    usersSummary: ''
                };
            }
        }

        function getPermissionName() {
            return "a string";
        }

        function getContentTypes() {

            vm.nodePermissions = vm.group.permissions.filter(function (v) {
                return v.nodeId;
            });
            
            vm.docPermissions = vm.group.permissions.filter(function (v) {
                return v.contentTypeId;
            });

            if (vm.nodePermissions.length) {
                contentResource.getByIds(vm.nodePermissions.map(function (v) { return v.nodeId; }))
                    .then(function (resp) {                        
                        resp.forEach(function (v) {
                            vm.nodePermissions.forEach(function (p) {
                                if (p.nodeId === v.id) {
                                    p.icon = v.icon;
                                    p.path = v.path;
                                    p.name = v.name + ' - stage ' + (p.permission + 1);
                                }
                            });
                        });
                    });
            }

            if (vm.docPermissions.length) {
                workflowResource.getContentTypes()
                    .then(function (resp) {
                        resp.forEach(function (v) {
                            vm.docPermissions.forEach(function (p) {
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
        function getHistory() {            
            workflowResource.getAllTasksForGroup($routeParams.id, vm.pagination.perPage, vm.pagination.pageNumber)
                .then(function (resp) {
                    vm.tasks = resp.items;
                    vm.pagination.pageNumber = resp.page;
                    vm.pagination.totalPages = resp.total / resp.count;
                });
        }

        function goToPage(i) {
            vm.pagination.pageNumber = i;
            getHistory();
        }

        function editDocTypePermission() {
            $location.path('/workflow/tree/view/settings');
        }

        // todo -> Would be sweet to open the config dialog from here, rather than just navigating to the node...
        function editContentPermission(id, path) {
            navigationService.changeSection('content');
            $location.path('/content/content/edit/' + id);            
        }

        //
        function remove(id) {
            var index;
            vm.group.users.forEach(function (u, i) {
                if (u.userId === id) {
                    index = i;
                }
            });
            vm.group.users.splice(index, 1);
        }

        function openUserPicker() {
            vm.userPicker = {
                view: '../app_plugins/workflow/backoffice/dialogs/workflow.userpicker.overlay.html',
                selection: vm.group.users,
                show: true,
                submit: function (model) {
                    vm.userPicker.show = false;
                    vm.userPicker = null;

                    vm.group.users = [];
                    model.selection.forEach(function (u) {
                        vm.group.users.push({ userId: u.userId || u.id, groupId: vm.group.groupId, name: u.name });
                    });
                },
                close: function (oldModel) {
                    vm.userPicker.show = false;
                    vm.userPicker = null;
                }
            };
        }

        //
        function save() {
            workflowGroupsResource.save(vm.group)
                .then(function (resp) {
                    if (resp.status === 200) {
                        notificationsService.success('SUCCESS', resp.msg);
                        $scope.approvalGroupForm.$setPristine();                        
                    } else {
                        notificationsService.error('ERROR', resp.msg);
                    }
                }, function (err) {
                    notificationsService.error('ERROR', err);
                });
        }

        angular.extend(vm, {
            save: save,
            remove: remove,
            editContentPermission: editContentPermission,
            editDocTypePermission: editDocTypePermission,
            openUserPicker: openUserPicker,
            perPage: function() {
                return [2, 5, 10, 20, 50];
            },

            tabs: [{
                id: 0,
                label: "Group detail",
                alias: "tab0",
                active: true
            },
            {
                id: 1,
                label: "Activity history",
                alias: "tab1",
                active: false
            }],
            pagination: {
                pageNumber: 1,
                totalPages: 0,
                perPage: 10,
                goToPage: goToPage
            } 
        });

        init();
        getHistory();
    }

    angular.module('umbraco').controller('Workflow.Groups.Edit.Controller', editController);
}());

