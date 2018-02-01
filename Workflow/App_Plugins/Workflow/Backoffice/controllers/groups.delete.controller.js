(function () {  
    'use strict';

    function deleteController($scope, $rootScope, workflowGroupsResource, navigationService, treeService, notificationsService) {

        $scope.delete = function (id) {
          workflowGroupsResource.delete(id)
                .then(function (resp) {
                    treeService.loadNodeChildren({ node: $scope.$parent.currentNode.parent(), section: 'workflow' })
                        .then(function () {
                            window.location = '/umbraco/#/workflow/workflow/approval-groups/info';
                        });

                    navigationService.hideNavigation();
                    notificationsService.success('SUCCESS', resp);
                    $rootScope.$emit('refreshGroupsDash');
              });
        };

        $scope.cancelDelete = function () {
            navigationService.hideNavigation();
        };
    }

    angular.module('umbraco').controller('Workflow.Groups.Delete.Controller', deleteController);
}());

