(() => {

    function actionController($scope, workflowResource) {
        this.limit = 250;
        this.disabled = this.isFinalApproval === true ? false : true;

        /**
         * Fetch all tasks for the current workflow instance
         * Then build a UI-ready object
         */
        workflowResource.getAllTasksByGuid($scope.model.guid)
            .then(resp => {
                const tasks = resp.items;

                // current step should only count approved tasks - maybe rejected/resubmitted into
                this.currentStep = resp.currentStep;
                this.totalSteps = resp.totalSteps;

                // there may be multiple tasks for a given step, due to rejection/resubmission
                // modify the tasks object to nest tasks

                this.tasks = [];
                tasks.forEach(v => {

                    if (!this.tasks[v.currentStep]) {
                        this.tasks[v.currentStep] = [];
                    }

                    this.tasks[v.currentStep].push(v);
                });
            });
    }

    angular.module('plumber')
        .controller('Workflow.Action.Controller', ['$scope', 'plmbrWorkflowResource', actionController]);
})();

