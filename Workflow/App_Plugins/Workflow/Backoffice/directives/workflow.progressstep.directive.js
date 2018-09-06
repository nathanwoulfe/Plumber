(() => {

    const template = `
        <div class="progress-step {{ css[0] }}" ng-class="{ 'last-of-type' : lastOfType, 'no-gradient' : noGradient }" ng-style="{ 'width' : width }">
            <span class="marker">
                <i class="icon-"></i>
            </span>
            <span class="tooltip">
                <span class="tooltip-{{ css[0] }}" ng-bind="css[1]"></span>
                {{ task.userGroup.name }}
            </span>
        </div>`;

    function progressStep() {

        const directive = {
            restrict: 'E',
            replace: true,
            scope: {
                task: '=',
                status: '=',
                total: '=',
                current: '='
            },
            template: template,
            link: scope => {

                scope.$watch('task',
                    () => {
                        scope.width = `${100 / scope.total}%`;

                        scope.css = scope.current > scope.task.permission
                            ? ['done', 'Approved by']
                            : scope.current === scope.task.permission && scope.status === 'rejected'
                            ? ['current', 'Rejected by']
                            : scope.current === scope.task.permission
                            ? ['current', 'Pending']
                            : ['pending', 'Pending'];

                        scope.lastOfType = scope.task.permission + 1 === scope.current;
                        scope.noGradient = scope.task.permission < scope.current -1;
                    }, true);
            }
        };

        return directive;
    }

    angular.module('plumber.directives').directive('wfProgressStep', progressStep);

})();
