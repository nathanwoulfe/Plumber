(function () {
    'use strict';

    // create controller 
    function DashboardController($scope, userGroupsResource, notificationsService, workflowPagingService, userService) {

        $scope.name = 'User groups';
        $scope.loading = true;
        $scope.numPerPage = 10;

        userGroupsResource.getAllGroups()
            .then(function (resp) {
                if (resp.data) {
                    $scope.loading = false;
                    $scope.items = resp.data.filter(function (v) {
                        return v.Name.indexOf('Deleted') === -1;
                    });
                }
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