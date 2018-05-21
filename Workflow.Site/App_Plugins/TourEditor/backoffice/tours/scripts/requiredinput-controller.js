(function () {
    'use strict';
    function RequiredInputController($scope) {
        var vm = this;
        var element = angular.element($scope.model.currentStep.element);
        vm.inputvalue = $scope.model.currentStep.customProperties.input;
        vm.error = false;
        vm.initNextStep = initNextStep;
        function initNextStep() {
            if (element.val().toLowerCase() === vm.inputvalue.toLowerCase()) {
                $scope.model.nextStep();
            } else {
                vm.error = true;
            }
        }
    }
    angular.module('umbraco').controller('Our.Umbraco.TourEditor.RequiredInputController', RequiredInputController);
}());