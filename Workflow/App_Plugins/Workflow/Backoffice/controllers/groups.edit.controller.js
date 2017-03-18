(function () {
    'use strict';

    function editController($scope, $routeParams, userGroupsResource, entityResource, notificationsService, localizationService) {

        $scope.action = $routeParams.id !== '-1' ? 'Edit' : 'Create';

        // fetch all active users, then get the group
        // this kicks it all off...
        (function getAllUsers() {
            entityResource.getAll('User', 'IsApproved')
                .then(function (resp) {
                    $scope.allUsers = resp;
                    getGroup();
                });
        })();

        // only fetch group if id is valid - otherwise it's a create action
        function getGroup() {
            if ($routeParams.id !== '-1') {
                userGroupsResource.get($routeParams.id)
                    .then(function (resp) {
                        $scope.group = resp;
                        $scope.name = $scope.action + ' ' + resp.name;
                        getUsersNotInGroup();                        
                    });
            } else {
                $scope.group = {
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

        // filter all users to remove those in the group
        function getUsersNotInGroup() {
            $scope.notInGroup = [];
            angular.forEach($scope.allUsers, function (user) {
                if (!$scope.group.usersSummary || $scope.group.usersSummary.indexOf('|' + user.id + '|') === -1) {
                    $scope.notInGroup.push(user);
                }
            });
        }

        // add a user to the group, and remove from notInGroup
        $scope.add = function (id) {
            var index,
                user = $.grep($scope.notInGroup, function (u, i) {
                    if (u.id === id) {
                        index = i;
                        $scope.group.users.push({ userId: u.id, groupId: $scope.group.groupId, name: u.name });
                        return true;
                    }
                    return false;
                })[0];

            $scope.notInGroup.splice(index, 1);
        };

        //
        $scope.remove = function (id) {
            var index,
                user = $.grep($scope.group.users, function (u, i) {
                    if (u.userId === id) {
                        index = i;
                        $scope.notInGroup.push({ id: u.userId, name: u.name });
                        return true;
                    }
                    return false;
                });

            $scope.group.users.splice(index, 1);
        };

        //
        $scope.saveGroup = function () {
            userGroupsResource.save($scope.group)
                .then(function (resp) {
                    if (resp.status === 200) {
                        notificationsService.success('SUCCESS', resp.msg);
                    } else {
                        notificationsService.error('ERROR', resp.msg);
                    }
                }, function (err) {
                    notificationsService.error('ERROR', err);
                });
        };
    }

    angular.module('umbraco').controller('Workflow.Groups.Edit.Controller', editController);
}());

