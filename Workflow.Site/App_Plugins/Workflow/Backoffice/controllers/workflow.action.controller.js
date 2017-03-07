(function () {
    'use strict';

    // create controller 
    function ActionController($scope) {
        $scope.limit = 250;
        $scope.disabled = $scope.isFinalApproval === true ? false : true;       
    };

    // register controller 
    angular.module('umbraco').controller('Workflow.Action.Controller', ActionController);
}());

