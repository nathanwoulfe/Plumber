(function () {
    'use strict';

    // create controller 
    function cancelController($scope) {
        $scope.model.comment = '';
        $scope.limit = 250;
        $scope.intro = 'This operation will cancel the workflow on this document and notify the workflow participants. Are you sure?';
        $scope.disabled = $scope.model.isFinalApproval === true ? false : true;
    }

    // register controller 
    angular.module('umbraco').controller('Workflow.Cancel.Controller', ['$scope', cancelController]);
}());
