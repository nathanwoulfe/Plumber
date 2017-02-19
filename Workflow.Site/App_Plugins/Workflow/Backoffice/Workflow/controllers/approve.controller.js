(function () {
    'use strict';

    // create controller 
    function Controller($scope, workflowResource, dialogService) {
        $scope.showLoader = false;

        $scope.approvalComment = '';
        $scope.limit = 250;
        $scope.approvalIntro = 'Please action the request to publish this document';

        $scope.isFinalApproval = $scope.dialogData.ActiveTask === 'Pending Final Approval' ? true : false;
        $scope.disabled = $scope.isFinalApproval === true ? false : true;

        $scope.approve = function () {
            $scope.showLoader = true;
            workflowResource.approveWorkflowTask($scope.dialogData.TaskId, $scope.approvalComment)
                .then(function (resp) {
                    $scope.showLoader = false;
                    $scope.submit(resp);
                });
        }

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
    angular.module('umbraco').controller('Workflow.Approve.Controller', Controller);
}());

