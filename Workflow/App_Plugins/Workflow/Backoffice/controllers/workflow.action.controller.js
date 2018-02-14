(function () {
    'use strict';

    function actionController($scope, workflowResource) {
        $scope.limit = 250;
        $scope.disabled = $scope.isFinalApproval === true ? false : true;

        var dateFormat = 'D MMM YYYY [at] h:mma',
            dateFormatNoMinute = 'D MMM YYYY [at] ha';

        /**
         * The requestedOn date for the instance should be parsed into a UI-ready value
         */
        var requestedOn = new moment($scope.model.requestedOn, 'DD/MM/YYYY hh:mm:ss');
        $scope.requestedOn = requestedOn.format(requestedOn.minute() === 0 ? dateFormatNoMinute : dateFormat);

        /**
         * Parses the requestedOn date into a UI fiendly version
         * @param {} date 
         * @returns {} 
         */
        $scope.getRequestedOn = function (date) {
            var d = new moment(date).utc();
            return d.format(d.minutes() === 0 ? dateFormatNoMinute : dateFormat);
        }

        /**
         * Ensures cssName/statusName is UI-friendly
         * @param {} task 
         * @returns {} 
         */
        $scope.getStatusName = function (task) {
            if (task.type === 1 && task.status === 7) {
                task.cssName = 'rejected';
                return 'Rejected';
            }

            return task.statusName.replace(' ', '-');    
        }

        /**
         * Set the icon for the given task, based on the stauts
         * @param { } task 
         * @returns { string } 
         */ 
        $scope.getIconName = function (task) {
            //rejected
            if (task.type === 1 && task.status === 7 || task.status === 2) {
                return 'delete';
            }
            // resubmitted or approved
            if (task.type === 2 && task.status === 7 || task.status === 1) {
                return 'check';
            }
            // not required
            if (task.status === 4) {
                return 'next-media';
            }
        }

        /**
         * Fetch all tasks for the current workflow instance
         * Then build a UI-ready object
         */
        workflowResource.getAllTasksByGuid($scope.model.guid)
            .then(function(resp) {
                var tasks = resp.items.filter(function(v) {
                    return v.comment !== null;
                });
                // current step should only count approved tasks - maybe rejected/resubmitted into
                $scope.currentStep = resp.currentStep;
                $scope.totalSteps = resp.totalSteps;

                // there may be multiple tasks for a given step, due to rejection/resubmission
                // modify the tasks object to nest tasks

                $scope.tasks = [];
                tasks.forEach(function(v) {

                    if (!$scope.tasks[v.approvalStep]) {
                        $scope.tasks[v.approvalStep] = [];
                    }

                    $scope.tasks[v.approvalStep].push(v);
                });
            });
    }

    angular.module('umbraco').controller('Workflow.Action.Controller', actionController);
}());

