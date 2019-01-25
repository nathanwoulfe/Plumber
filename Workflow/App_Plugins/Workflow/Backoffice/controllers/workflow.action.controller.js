(() => {

    function actionController($scope, workflowResource) {
        this.limit = 250;
        this.disabled = this.isFinalApproval === true ? false : true;
        this.tasksLoaded = false;

        const avatarName = task => {
            // don't show group if admin completed the task
            if (task.actionedByAdmin)
                return 'Admin';

            // if not required, show the group name
            if (task.status === 4)
                return task.approvalGroup;

            // finally show either the group or the user - resubmitted tasks won't have a group, just a user
            return task.approvalGroup || task.completedBy;
        };

        const statusColor = status => {
            switch (status) {
            case 1:
                return 'success'; //approved
            case 2:
                return 'warning'; //rejected
            case 3:
                return 'gray'; //pending
            case 4:
                return 'info'; //not required
            case 5:
                return 'danger'; //cancelled
            case 6:
                return 'danger'; //error
            default:
                return 'gray'; //resubmitted
            }
        };

        const whodunnit = task => {

            // if rejected or incomplete, use the group name
            if (task.status === 4 || !task.completedBy)
                return task.approvalGroup;

            // if actioned by an admin, show
            if (task.actionedByAdmin)
                return `${task.completedBy} as Admin`;

            // if approved, show the user and group name
            if (task.approvalGroup)
                return `${task.completedBy} for ${task.approvalGroup}`;

            // otherwise, just show the user name - resubmitted tasks don't have a group
            return task.completedBy;
        };

        /**
         * If the instance has status === error, the error message is on the author comment
         * wrapped in square brackets. This extracts it.
         * @returns {string} c
         */
        this.extractErrorFromComment = () => {
            const c = $scope.model.item.comment;
            return c.substring(c.indexOf('[') + 1, c.length - 1);
        };

        /**
         * Fetch all tasks for the current workflow instance
         * Then build a UI-ready object
         * TODO => review this. Tasks exist on $scope.model.item, but need current/total step values
         */
        workflowResource.getAllTasksByGuid($scope.model.item.instanceGuid)
            .then(resp => {
                this.tasksLoaded = true;

                // current step should only count approved tasks - maybe rejected/resubmitted into
                this.currentStep = resp.currentStep;
                this.totalSteps = resp.totalSteps;

                // there may be multiple tasks for a given step, due to rejection/resubmission
                // modify the tasks object to nest tasks

                this.tasks = [];
                resp.items.forEach(t => {

                    // push some extra UI strings onto each task
                    t.avatarName = avatarName(t);
                    t.statusColor = statusColor(t.status);
                    t.whodunnit = whodunnit(t);

                    if (!this.tasks[t.currentStep]) {
                        this.tasks[t.currentStep] = [];
                    }

                    this.tasks[t.currentStep].push(t);
                });
            });
    }

    angular.module('plumber')
        .controller('Workflow.Action.Controller', ['$scope', 'plmbrWorkflowResource', actionController]);
})();

