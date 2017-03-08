(function () {
    'use strict';

    function deleteController($scope, userGroupsResource, navigationService, treeService, notificationsService) {

        $scope.delete = function (id) {
            userGroupsResource.deleteGroup(id)
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
                    window.location = '/umbraco/#/workflow/tree/view/usergroups';
                });
        }

    };

    angular.module('umbraco').controller('Workflow.Groups.Delete.Controller', deleteController);
}());

