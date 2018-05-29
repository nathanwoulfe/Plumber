(() => {

    function progressStep() {

        const directive = {
            restrict: 'E',
            replace: true,
            scope: {
                task: '=',
                count: '='
            },
            templateUrl: '../app_plugins/workflow/backoffice/views/partials/workflowProgressStepTemplate.html',
            link: scope => {
                scope.width = `${100 / scope.count}%`;

                scope.css = scope.task.cssStatus === 'approved'
                    ? ['done', 'Done']
                    : scope.task.cssStatus === 'pending'
                        ? ['current', 'Pending']
                        : scope.task.cssStatus === 'not'
                            ? ['notrequired', 'Not required']
                            : [scope.task.cssStatus.toLowerCase(), scope.task.cssStatus];
            }
        };

        return directive;
    }

    angular.module('umbraco.directives').directive('wfProgressStep', progressStep);

})();
