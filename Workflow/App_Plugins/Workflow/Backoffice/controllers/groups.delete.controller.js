(function () {
    'use strict';

    function deleteController($scope, userGroupsResource, navigationService, treeService, notificationsService) {

        $scope.delete = function (id) {
            userGroupsResource.delete(id)
                .then(function (resp) {
                    treeService.loadNodeChildren({ node: $scope.$parent.currentNode.parent(), section: 'users' })
                        .then(function (r) {
                            window.location = '/umbraco/#/workflow/tree/view/groups';
                        });
                    navigationService.hideNavigation();
                    notificationsService.success('SUCCESS', resp);
                });
        };

        $scope.cancelDelete = function () {
            navigationService.hideNavigation();
        };
    }

    angular.module('umbraco').controller('Workflow.Groups.Delete.Controller', deleteController);
}());

