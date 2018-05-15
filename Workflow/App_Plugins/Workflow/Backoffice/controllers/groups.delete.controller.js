(() => {
    'use strict';

    function deleteController($scope,
        $rootScope,
        workflowGroupsResource,
        navigationService,
        treeService,
        notificationsService) {

        this.delete = id => {
            workflowGroupsResource.delete(id)
                .then(resp => {
                    treeService.loadNodeChildren({ node: $scope.$parent.currentNode.parent(), section: 'workflow' })
                        .then(() => {
                            navigationService.hideNavigation();
                            notificationsService.success('SUCCESS', resp);
                            $rootScope.$emit('refreshGroupsDash');
                        });
                });
        };

        this.cancelDelete = () => {
            navigationService.hideNavigation();
        };
    }

    angular.module('umbraco').controller('Workflow.Groups.Delete.Controller',
        [
            '$scope', '$rootScope', 'plmbrGroupsResource', 'navigationService', 'treeService', 'notificationsService',
            deleteController
        ]);
})();