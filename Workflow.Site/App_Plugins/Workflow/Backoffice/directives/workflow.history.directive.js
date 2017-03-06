(function () {
    'use strict';

    function HistoryDirective() {

        var directive = {
            restrict: 'E',
            scope: {
                items: '=',
                isNode: '='
            },
            templateUrl: '../app_plugins/workflow/backoffice/partials/workflowhistorytemplate.html',
            link: function (scope, element, attrs) {
                scope.numPerPage = 10;
            }
        };

        return directive;
    }

    angular.module('umbraco.directives').directive('wfHistory', HistoryDirective);

}());