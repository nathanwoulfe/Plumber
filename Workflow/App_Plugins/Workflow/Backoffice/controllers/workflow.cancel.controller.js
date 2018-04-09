(function () {
    'use strict';

    // create controller 
    function cancelController($scope) {
        $scope.model.comment = '';
        this.limit = 250;
        this.intro = 'This operation will cancel the workflow on this document and notify the workflow participants. Are you sure?';
        this.disabled = $scope.model.isFinalApproval === true ? false : true;
        debugger;
    }

    // register controller 
    angular.module('umbraco').controller('Workflow.Cancel.Controller', ['$scope', cancelController]);
}());
