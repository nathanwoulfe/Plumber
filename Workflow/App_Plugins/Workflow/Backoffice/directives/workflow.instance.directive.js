(() => {

    function instances(workflowActionsService) {

        const directive = {
            restrict: 'E',
            scope: {
                items: '=',
                loaded: '=',
                view: '='
            },
            templateUrl: '../app_plugins/workflow/backoffice/views/partials/workflowInstanceTemplate.html',
            link: scope => {
                scope.detail = item => {
                    scope.instanceOverlay = workflowActionsService.detail(item);
                };

                scope.$watch('view',
                    () => {
                        scope.showProgress = scope.view.indexOf('activity') === -1 && scope.view !== 'group';
                        scope.showName = scope.view === 'instance' || scope.view.indexOf('activity') === 0 || scope.view === 'group';
                    });
            }

        };

        return directive;
    }

    angular.module('plumber.directives').directive('wfInstance', ['plmbrActionsService', instances]);

})();
