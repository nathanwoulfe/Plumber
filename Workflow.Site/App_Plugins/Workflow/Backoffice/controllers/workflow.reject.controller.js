(function () {
    'use strict';

    // create controller 
    function Controller($scope, workflowResource, dialogService) {
        $scope.showLoader = false;

        $scope.approvalComment = '';

        $scope.isFinalApproval = $scope.dialogData.ActiveTask === 'Pending Final Approval' ? true : false;
        $scope.disabled = $scope.isFinalApproval === true ? false : true;

        $scope.reject = function () {
            $scope.showLoader = true;
            workflowResource.rejectWorkflowTask($scope.dialogData.TaskId, $scope.approvalComment)
                .then(function (resp) {
                    $scope.showLoader = false;
                    $scope.submit(resp);
                });
        }
    };

    // register controller 
    angular.module('umbraco').controller('Workflow.Reject.Controller', Controller);
}());

