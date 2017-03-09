(function () {
    'use strict';

    function addController($scope, userGroupsResource, navigationService, notificationsService, treeService) {

        $scope.add = function (name) {
            userGroupsResource.add(name)
                .then(function (resp) {
                    if (resp.status === 200) {
                        notificationsService.success('SUCCESS', resp.msg);
                        window.location = '/umbraco/#/workflow/tree/edit/' + resp.id;
                        navigationService.hideNavigation();
                        refreshTree();
                    } else {
                        notificationsService.error('ERROR', resp.msg);
                    }
                }, function (err) {   
                    notificationsService.error('ERROR', err);
                });
        };

        function refreshTree() {
            treeService.loadNodeChildren({ node: $scope.$parent.currentNode.parent(), section: 'users' });
        }

        $scope.cancelAdd = function () {
            navigationService.hideNavigation();
        };
    }

    angular.module('umbraco').controller('Workflow.Groups.Add.Controller', addController);
}());

