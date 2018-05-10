(function () {
    'use strict';

    function actionController($scope, workflowResource) {
        this.limit = 250;
        this.disabled = this.isFinalApproval === true ? false : true;

        const dateFormat = 'D MMM YYYY [at] h:mma';
        const dateFormatNoMinute = 'D MMM YYYY [at] ha';

        /**
         * The requestedOn date for the instance should be parsed into a UI-ready value
         */
        const requestedOn = new moment($scope.model.requestedOn, 'DD/MM/YYYY hh:mm:ss');
        this.requestedOn = requestedOn.format(requestedOn.minute() === 0 ? dateFormatNoMinute : dateFormat);

        /**
         * Parses the requestedOn date into a UI fiendly version
         * @param {} date 
         * @returns {} 
         */
        this.getRequestedOn = date => {
            var d = new moment(date).utc();
            return d.format(d.minutes() === 0 ? dateFormatNoMinute : dateFormat);
        };

        /**
         * Ensures cssName/statusName is UI-friendly
         * @param {} task 
         * @returns {} 
         */
        this.getStatusName = task => {
            if ((task.type === 1 || task.type === 3) && task.status === 7) {
                task.cssName = 'rejected';
                return 'Rejected';
            }

            return task.statusName.replace(' ', '-');
        };

        /**
         * Set the icon for the given task, based on the stauts
         * @param { } task 
         * @returns { string } 
         */
        this.getIconName = task => {
            //rejected
            if ((task.type === 1 || task.type === 3) && task.status === 7 || task.status === 2) {
                return 'delete';
            }
            // resubmitted or approved
            if (task.type === 2 && task.status === 7 || task.status === 1) {
                return 'check';
            }
            // pending
            if (task.status === 3) {
                return 'record';
            }
            // not required
            if (task.status === 4) {
                return 'next-media';
            }

            return '';
        };

        /**
         * Fetch all tasks for the current workflow instance
         * Then build a UI-ready object
         */
        workflowResource.getAllTasksByGuid($scope.model.guid)
            .then(resp => {
                var tasks = resp.items;//.filter(v => v.comment !== null);

                // current step should only count approved tasks - maybe rejected/resubmitted into
                this.currentStep = resp.currentStep;
                this.totalSteps = resp.totalSteps;

                // there may be multiple tasks for a given step, due to rejection/resubmission
                // modify the tasks object to nest tasks

                this.tasks = [];
                tasks.forEach(v => {

                    if (!this.tasks[v.approvalStep]) {
                        this.tasks[v.approvalStep] = [];
                    }

                    this.tasks[v.approvalStep].push(v);
                });
            });
    }

    angular.module('umbraco').controller('Workflow.Action.Controller', ['$scope', 'plmbrWorkflowResource', actionController]);
}());

