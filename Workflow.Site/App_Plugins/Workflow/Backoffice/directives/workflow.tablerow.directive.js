(function () {
    'use strict';

    function TableRowDirective() {

        var directive = {
            restrict: 'E',
            scope: {
                item: '='
            },
            templateUrl: function(elem, attrs) {
                return attrs.isNode ? '../app_plugins/workflow/backoffice/partials/table/historytask.html' : '../app_plugins/workflow/backoffice/partials/table/historyinstance.html';
            },
            link: function (scope) {
                //if (!scope.hasTasks) {
                //    directive.templateUrl = '../app_plugins/workflow/backoffice/partials/table/historytask.html';
                //}
            }
        };

        return directive;
    }

    angular.module('umbraco.directives').directive('wfTableRow', TableRowDirective);

}());