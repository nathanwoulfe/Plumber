(() => {
    'use strict';

    // create controller 
    function contentTypeFlowController($scope) {

        this.properties = [];
        this.approvalPath = [];
        this.conditions = [];

        const updateSortOrder = () => { };
        
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
        } else {
            this.isAdd = true;
        }

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
        this.setCondition = (groupId, condition, oldValue) => {
            this.approvalPath.forEach(group => {
                if (group.groupId === groupId) {
                    group.permissions.forEach(p => {
                        if (p.contentTypeId === $scope.model.type.id) {
                            if (p.condition) {
                                // if oldvalue exists, replace it since this is a change to an existing condition
                                // oldvalue won't exist if it's a new condition
                                const oldIndex = p.condition.indexOf(oldValue);
                                if (oldIndex !== -1) {
                                    p.condition[oldIndex] = condition;
                                } else {
                                    p.condition.push(condition);
                                }
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

            // when adding a new config, type will not exist.
            if (!$scope.model.type) {
                $scope.model.type = {};
            }

            // the group assigned to the stage also needs a permission object created - this isn't stored anywhere, just used for UI stuff
            this.selectedApprovalGroup.permissions.push({
                contentTypeId: $scope.model.type.id,
                permission: this.approvalPath.length,
                groupId: this.selectedApprovalGroup.groupId
            });

            this.approvalPath.push(this.selectedApprovalGroup);
            $scope.model.type.approvalPath = this.approvalPath;
        };

        /**
         * 
         * @param {any} $event
         * @param {any} index
         */
        this.remove = ($event, index, groupId) => {
            $event.stopPropagation();
            this.approvalPath.splice(index, 1);
            $scope.model.type.approvalPath = this.approvalPath;

            // also remove any conditions - can't do in the existing method as params are different.
            if (this.conditions.length > 0) {
                this.conditions = this.conditions.filter(c => c.groupId !== groupId);
            }
        };

        this.multiNames = () => {
            const names = $scope.model.typesToAdd.map(m => m.name);
            let resp = '';

            if (names.length === 1) {
                resp = names[0];
            } else if (names.length === 2) {
                resp = names.join(' and ');
            } else if (names.length > 2) {
                resp = names.slice(0, -1).join(', ') + ', and ' + names.slice(-1);
            }

            // if it's a multi  and an add, populate type if there is only one selected
            // doing it here so that it updates when/if the types are added/removed from model.multi
            if ($scope.model.typesToAdd.length === 1) {
                $scope.model.type = $scope.model.typesToAdd[0];
            }

            return resp;
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

