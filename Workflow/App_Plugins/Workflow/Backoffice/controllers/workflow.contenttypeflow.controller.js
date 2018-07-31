(() => {
    'use strict';

    // create controller 
    function contentTypeFlowController($scope) {

        if ($scope.model.type) {
            this.approvalPath = $scope.model.type.approvalPath;
        }

        const updateSortOrder = () => {};

        /**
         * 
         */
        this.add = () => {
            if (this.approvalPath) {
                this.approvalPath.push(this.selectedApprovalGroup);
            } else {
                this.approvalPath = [this.selectedApprovalGroup];
            }

            $scope.model.type.approvalPath = this.approvalPath;
        };

        /**
         * 
         * @param {any} $event
         * @param {any} index
         */
        this.remove = ($event, index) => {
            $event.stopPropagation();
            this.approvalPath.splice(index, 1);
            $scope.model.type.approvalPath = this.approvalPath;
        };

        this.sortOptions = {
            axis: 'y',
            cursor: 'move',
            handle: '.sort-handle',
            stop: () => {
                updateSortOrder();
            }
        };
    }

    // register controller 
    angular.module('plumber').controller('Workflow.ContentTypeFlow.Controller', ['$scope', contentTypeFlowController]);
})();

