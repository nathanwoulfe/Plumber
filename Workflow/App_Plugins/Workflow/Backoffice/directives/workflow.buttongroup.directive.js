(function () {
    'use strict';

    function buttonGroupDirective(workflowActionsService) {

        var directive = {
            restrict: 'E',
            replace: true,
            templateUrl: '../app_plugins/workflow/backoffice/partials/workflowButtonGroup.html',
            scope: {
                defaultButton: '=',
                subButtons: '=',
                state: '=?',
                item: '=',
                direction: '@?',
                float: '@?',
                drawer: '@?'
            }, 
            link: function (scope) {
                scope.detail = function (item) {
                    scope.workflowOverlay = workflowActionsService.detail(item);
                }
            }
        };

        return directive;
    }

    angular.module('umbraco.directives').directive('workflowButtonGroup', buttonGroupDirective);

}());
