(function () {
    'use strict';

    function actionController($scope, workflowResource) {
        $scope.limit = 250;
        $scope.disabled = $scope.isFinalApproval === true ? false : true;

        workflowResource.getAllTasksByGuid($scope.model.guid)
            .then(function(resp) {
                $scope.tasks = resp.items.filter(function(v) {
                    return v.comment !== null;
                });
            });
    }

    angular.module('umbraco').controller('Workflow.Action.Controller', actionController);
}());

