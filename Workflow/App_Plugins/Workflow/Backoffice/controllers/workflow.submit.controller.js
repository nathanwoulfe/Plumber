(() => {
    'use strict';

    const submitController = $scope => {
        $scope.$watch('model.comment',
            newVal => {
                $scope.model.hideSubmitButton = !newVal || newVal.length === 0;
            });
    };

    angular.module('plumber').controller('Workflow.Submit.Controller', ['$scope', submitController]);
})();