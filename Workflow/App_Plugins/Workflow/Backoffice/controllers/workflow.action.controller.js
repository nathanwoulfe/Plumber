(function () {
    'use strict';

    function actionController($scope, workflowResource) {
        $scope.limit = 250;
        $scope.disabled = $scope.isFinalApproval === true ? false : true;
        var dateFormat = 'D MMM YYYY [at] h:mm a';

        $scope.getRequestedOn = function(date) {
            return new moment(date).utc().format(dateFormat);
        }

        $scope.requestedOn = new moment($scope.model.requestedOn, 'DD/MM/YYYY hh:mm:ss').format(dateFormat);

        workflowResource.getAllTasksByGuid($scope.model.guid)
            .then(function(resp) {
                $scope.tasks = resp.items.filter(function(v) {
                    return v.comment !== null;
                });
                $scope.totalSteps = resp.totalSteps
            });
    }

    angular.module('umbraco').controller('Workflow.Action.Controller', actionController);
}());

