(function () {
    'use strict';

    function editController($scope, $routeParams, userGroupsResource, workflowResource, entityResource, notificationsService, localizationService) {

        var vm = this;

        // filter all users to remove those in the group
        function getUsersNotInGroup() {
            vm.notInGroup = [];
            angular.forEach(vm.allUsers, function (user) {
                if (!vm.group.usersSummary || vm.group.usersSummary.indexOf('|' + user.id + '|') === -1) {
                    vm.notInGroup.push(user);
                }
            });
        }

        // only fetch group if id is valid - otherwise it's a create action
        function getGroup() {
            if ($routeParams.id !== '-1') {
                userGroupsResource.get($routeParams.id)
                    .then(function (resp) {
                        vm.group = resp;
                        vm.name = $routeParams.id !== '-1' ? 'Edit ' : 'Create ' + resp.name;
                        getUsersNotInGroup();
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
                getUsersNotInGroup();
            }
        }

        // fetch all active users, then get the group
        // this kicks it all off...
        function getAllUsers() {
            entityResource.getAll('User', 'IsApproved')
                .then(function (resp) {
                    vm.allUsers = resp;
                    getGroup();
                });
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

        // add a user to the group, and remove from notInGroup
        function add(id) {
            var index,
                user = $.grep(vm.notInGroup, function (u, i) {
                    if (u.id === id) {
                        index = i;
                        vm.group.users.push({ userId: u.id, groupId: vm.group.groupId, name: u.name });
                        return true;
                    }
                    return false;
                })[0];

            vm.notInGroup.splice(index, 1);
        }

        //
        function remove(id) {
            var index,
                user = $.grep(vm.group.users, function (u, i) {
                    if (u.userId === id) {
                        index = i;
                        vm.notInGroup.push({ id: u.userId, name: u.name });
                        return true;
                    }
                    return false;
                });

            vm.group.users.splice(index, 1);
        }

        //
        function save() {
            userGroupsResource.save(vm.group)
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
            add: add,
            remove: remove,
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

        getAllUsers();
        getHistory();
    }

    angular.module('umbraco').controller('Workflow.Groups.Edit.Controller', editController);
}());

