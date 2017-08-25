(function () {
    'use strict';

    function deleteController($scope, workflowGroupsResource, navigationService, treeService, notificationsService) {

        $scope.delete = function (id) {
          workflowGroupsResource.delete(id)
                .then(function (resp) {
                    treeService.loadNodeChildren({ node: $scope.$parent.currentNode.parent(), section: 'users' })
                        .then(function () {
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

