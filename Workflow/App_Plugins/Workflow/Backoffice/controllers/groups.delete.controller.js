(function () {  
    'use strict';

    function deleteController($scope, $rootScope, workflowGroupsResource, navigationService, treeService, notificationsService) {

        $scope.delete = function (id) {
          workflowGroupsResource.delete(id)
                .then(function (resp) {
                    treeService.loadNodeChildren({ node: $scope.$parent.currentNode.parent(), section: 'workflow' })
                        .then(function () {
                            navigationService.hideNavigation();
                            notificationsService.success('SUCCESS', resp);
                            $rootScope.$emit('refreshGroupsDash');
                        });
              });
        };

        $scope.cancelDelete = function () {
            navigationService.hideNavigation();
        };
    }

    angular.module('umbraco').controller('Workflow.Groups.Delete.Controller',
        ['$scope', '$rootScope', 'plmbrGroupsResource', 'navigationService', 'treeService', 'notificationsService', deleteController]);
}());

