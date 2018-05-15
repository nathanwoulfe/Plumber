(() => {
    'use strict';

    function addController($scope, workflowGroupsResource, navigationService, notificationsService, treeService) {

        $scope.$watch('name',
            () => {
                this.failed = false;
            });

        this.add = name => {
            workflowGroupsResource.add(name)
                .then(resp => {
                        if (resp.status === 200) {
                            if (resp.success === true) {
                                treeService.loadNodeChildren({
                                    node: $scope.$parent.currentNode.parent(),
                                    section: 'workflow'
                                }).then(() => {
                                    window.location = `/umbraco/#/workflow/workflow/edit-group/${resp.id}`;
                                    navigationService.hideNavigation();
                                });

                                notificationsService.success('SUCCESS', resp.msg);
                            } else {
                                this.failed = true;
                                this.msg = resp.msg;
                            }
                        } else {
                            notificationsService.error('ERROR', resp.msg);
                        }

                    },
                    err => {
                        notificationsService.error('ERROR', err);
                    });
        };

        this.cancelAdd = () => {
            navigationService.hideNavigation();
        };
    }

    angular.module('umbraco').controller('Workflow.Groups.Add.Controller',
        ['$scope', 'plmbrGroupsResource', 'navigationService', 'notificationsService', 'treeService', addController]);
})();