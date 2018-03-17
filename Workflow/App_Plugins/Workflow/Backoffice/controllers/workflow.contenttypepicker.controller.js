(function () {
    'use strict';

    function contentTypePickerController($scope) {

        var vm = this;

        vm.types = [];
        vm.loading = false;

        vm.selectType = selectType;

        //////////

        function onInit() {
            vm.loading = true;
            vm.types = $scope.model.types;
            vm.loading = false;
        }

        function selectType(type) {
            $scope.model.types.forEach(function (t) {
                t.selected = false;
            });

            type.selected = true;
            $scope.model.selection = type;
        }
        
        onInit();

    }

    angular.module('umbraco').controller('Workflow.ContentTypePicker.Controller', contentTypePickerController);

})();