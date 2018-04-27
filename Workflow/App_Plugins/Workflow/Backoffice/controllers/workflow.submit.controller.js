(() => {
    'use strict';

    const submitController = $scope => {
        $scope.$watch('model.comment', newVal => {
            $scope.model.hideSubmitButton = !newVal || newVal.length === 0;
        });
    }

    angular.module('umbraco').controller('Workflow.Submit.Controller', ['$scope', submitController]);
})();