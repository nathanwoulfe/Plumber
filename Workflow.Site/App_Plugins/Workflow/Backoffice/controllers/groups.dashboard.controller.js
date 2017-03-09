(function () {
    'use strict';

    function dashboardController($scope, userGroupsResource, notificationsService, workflowPagingService, userService) {

        $scope.name = 'User groups';
        $scope.loading = true;
        $scope.numPerPage = 10;

        userGroupsResource.get()
            .then(function (resp) {
                $scope.loading = false;
                $scope.items = resp.filter(function (v) {
                    return v.name.indexOf('Deleted') === -1;
                });                
            });

        $scope.getEmail = function (users) {
            return users.map(function (v) {
                return v.user.email;
            }).join(';');
        };
    };

    angular.module('umbraco').controller('Workflow.Groups.Dashboard.Controller', dashboardController);

}());