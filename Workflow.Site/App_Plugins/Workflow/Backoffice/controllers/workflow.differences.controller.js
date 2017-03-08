(function () {
    'use strict';

    // create controller 
    function differencesController($scope, workflowResource) {

        workflowResource.showDifferences($scope.dialogData.NodeId, $scope.dialogData.TaskId)
            .then(function (resp) {
                $scope.differences = resp.data;
            });
    }

    // register controller 
    angular.module('umbraco').controller('Workflow.Differences.Controller', differencesController);
}());
