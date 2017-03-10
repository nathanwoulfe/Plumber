(function () {
    'use strict';

    function deleteController($scope, userGroupsResource, navigationService, treeService, notificationsService) {

        $scope.delete = function (id) {
            userGroupsResource.delete(id)
                .then(function (resp) {
                    notificationsService.success('SUCCESS', resp);
                    refreshTree();
                    navigationService.hideNavigation();
                });
        };

        $scope.cancelDelete = function () {
            navigationService.hideNavigation();
        };

        function refreshTree() {
            treeService.loadNodeChildren({ node: $scope.$parent.currentNode.parent(), section: 'users' })
                .then(function (r) {
                    window.location = '/umbraco/#/workflow/tree/view/groups';
                });
        }

    };

    angular.module('umbraco').controller('Workflow.Groups.Delete.Controller', deleteController);
}());

