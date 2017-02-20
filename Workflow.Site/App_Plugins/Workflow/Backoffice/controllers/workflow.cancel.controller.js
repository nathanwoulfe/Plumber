(function () {
    'use strict';

    // create controller 
    function Controller($scope, workflowResource) {
        $scope.approvalComment = '';
        $scope.limit = 250;
        $scope.approvalIntro = 'This operation will cancel the workflow on this document and notify the workflow participants. Are you sure?';
        $scope.isFinalApproval = $scope.dialogData.ActiveTask === 'Pending Final Approval' ? true : false;

        $scope.disabled = $scope.isFinalApproval === true ? false : true;

        $scope.approve = function () {
            workflowResource.cancelWorkflowTask($scope.dialogData.TaskId, $scope.approvalComment)
                .then(function (resp) {
                    $scope.submit(resp);
                });
        }
    };

    // register controller 
    angular.module('umbraco').controller('Workflow.Cancel.Controller', Controller);
}());
