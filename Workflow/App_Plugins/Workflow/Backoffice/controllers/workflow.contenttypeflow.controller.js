(() => {
    'use strict';

    // create controller 
    function contentTypeFlowController($scope) {

        this.properties = [];
        this.approvalPath = [];
        this.conditions = [];
        
        if ($scope.model.type) {
            // if it's an edit, path is on the type, so shift it for convience later
            this.approvalPath = $scope.model.type.approvalPath;

            $scope.model.type.propertyGroups.forEach(pg => {
                pg.propertyTypes.forEach(pt => {
                    this.properties.push(pt);
                });
            });

            // approvalPath will also hold the conditional keys, if any exist
            // use these to build the conditions array
            this.approvalPath.forEach(step => {
                const steps = step.permissions.filter(p => p.contentTypeId === $scope.model.type.id);

                if (steps) {
                    steps.forEach(s => {
                        if (s.condition) {
                            s.condition.forEach(c => {
                                this.conditions.push({
                                    groupName: step.name,
                                    groupId: step.groupId,
                                    condition: c
                                });
                            });
                        }
                    });
                }
            });
        }

        const updateSortOrder = () => {};

        /**
         * 
         * @returns {} 
         */
        this.addCondition = () => {
            this.conditions.push({});
        };

        /**
         * 
         * @param {} $event 
         * @param {} $index 
         * @returns {} 
         */
        this.removeCondition = ($event, index, condition, groupId) => {
            $event.stopPropagation();
            this.conditions.splice(index, 1);

            this.approvalPath.forEach(step => {
                if (step.groupId === groupId) {
                    step.permissions.forEach(p => {
                        if (p.contentTypeId === $scope.model.type.id) {
                            p.condition.splice(p.condition.indexOf(condition), 1);
                        }
                    });
                }
            });
        };

        /**
         * 
         * @param {} groupId 
         * @param {} condition 
         * @returns {} 
         */
        this.setCondition = (groupId, condition) => {
            this.approvalPath.forEach(step => {
                if (step.groupId === groupId) {
                    step.permissions.forEach(p => {
                        if (p.contentTypeId === $scope.model.type.id) {
                            if (p.condition) {
                                p.condition.push(condition);
                            } else {
                                p.condition = [condition];
                            }
                        }
                    });
                }
            });
        };

        /**
         * 
         */
        this.add = () => {
            this.approvalPath.push(this.selectedApprovalGroup);
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

