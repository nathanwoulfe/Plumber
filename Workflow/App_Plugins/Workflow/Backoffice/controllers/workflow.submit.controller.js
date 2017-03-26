(function () {
    'use strict';

    function submitController($scope) {

        var formScope = angular.element($('form[name="contentForm"]')).scope();
        $scope.dirty = formScope ? formScope.contentForm.$dirty : false;  

        $scope.$watch('model.comment', function (newVal) {
            $scope.model.hideSubmitButton = !newVal || newVal.length === 0;
        });
    }

    angular.module('umbraco').controller('Workflow.Submit.Controller', submitController);
}());

