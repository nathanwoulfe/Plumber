(function () {
    'use strict';

    function addController($scope, workflowGroupsResource, navigationService, notificationsService, treeService) {

        $scope.$watch('name',
            function() {
                $scope.failed = false;
            });

        $scope.add = function (name) {
          workflowGroupsResource.add(name)
              .then(function (resp) {
                    if (resp.status === 200) {
                        if (resp.success === true) {
                            treeService.loadNodeChildren({
                                    node: $scope.$parent.currentNode.parent(),
                                    section: 'workflow'
                                })
                                .then(function() {
                                    window.location = Umbraco.Sys.ServerVariables.umbracoSettings.umbracoPath + '/#/workflow/workflow/edit-group/' + resp.id;
                                    navigationService.hideNavigation();
                                });
                            notificationsService.success('SUCCESS', resp.msg);
                        } else {
                            $scope.failed = true;
                            $scope.msg = resp.msg;
                        }
                    } else {
                        notificationsService.error('ERROR', resp.msg);
                    }

                }, function (err) {   
                    notificationsService.error('ERROR', err);
                });
        };

        $scope.cancelAdd = function () {
            navigationService.hideNavigation();
        };
    }

    angular.module('umbraco').controller('Workflow.Groups.Add.Controller', addController);
}());

