(function () {
    'use strict';

    // create controller 
    function contentTypeFlowController($scope) {
        var vm = this;

        vm.approvalPath = $scope.model.type.approvalPath;
        
        function updateSortOrder() { }

        /**
         * 
         */
        function add() {
            if (vm.approvalPath) {
                vm.approvalPath.push(vm.selectedApprovalGroup);
            } else {
                vm.approvalPath = [vm.selectedApprovalGroup];
            }

            $scope.model.type.approvalPath = vm.approvalPath;
        }

        /**
         * 
         * @param {any} $event
         * @param {any} index
         */
        function remove($event, index) {
            vm.approvalPath.splice(index, 1);

            $scope.model.type.approvalPath = vm.approvalPath;
        }

        angular.extend(vm, {

            sortOptions: {
                axis: 'y',
                cursor: 'move',
                handle: '.sort-handle',
                stop: function () {
                    updateSortOrder();
                }
            },

            add: add,
            remove: remove
        });
    }

    // register controller 
    angular.module('umbraco').controller('Workflow.ContentTypeFlow.Controller', contentTypeFlowController);
}());

