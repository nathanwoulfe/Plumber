(() => {

    function progressStep() {

        const directive = {
            restrict: 'E',
            replace: true,
            scope: {
                task: '=',
                count: '='
            },
            template: `
                <div class="progress-step {{ css[0] }}" ng-style="{ 'width' : width }">
                    <span class="marker"></span>
                    <span class="tooltip">
                        <span class="tooltip-{{ css[0] }}" ng-bind="css[1]"></span>
                        {{ task.approvalGroup }}
                    </span>
                </div>`,
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
