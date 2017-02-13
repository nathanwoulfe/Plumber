(function () {
    'use strict';

    // create controller 
    function DashboardController($scope, UserGroupsResource, notificationsService, workflowPagingService, userService) {

        $scope.name = 'User groups';
        $scope.loading = true;
        $scope.numPerPage = 10;

        UserGroupsResource.getAllGroups()
            .then(function (resp) {
                $scope.loading = false;
                $scope.items = resp.filter(function (v) {
                    return v.Name.indexOf('Deleted') === -1;
                });

                console.log($scope.items);
            });

        $scope.getEmail = function (users) {
            return users.map(function (v) {
                return v.User.Email;
            }).join(';');
        };
    };

    // register controller 
    angular.module('umbraco').controller('Workflow.UserGroups.Dashboard.Controller', DashboardController);

}());