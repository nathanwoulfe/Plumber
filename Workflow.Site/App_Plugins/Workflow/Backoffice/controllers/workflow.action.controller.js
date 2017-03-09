(function () {
    'use strict';

    function actionController($scope) {
        $scope.limit = 250;
        $scope.disabled = $scope.isFinalApproval === true ? false : true;       
    };

    angular.module('umbraco').controller('Workflow.Action.Controller', actionController);
}());

