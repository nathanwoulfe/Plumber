(function () {
    'use strict';

    function TableRowDirective() {

        var directive = {
            restrict: 'E',
            scope: {
                item: '=',
                instanceView: '='
            },
            link: function (scope) {
                scope.templateUrl = '../app_plugins/workflow/backoffice/partials/table/' + (scope.instanceView ? 'historyinstance' : 'historytask') + '.html';
            },
            template: '<ng-include src="templateUrl"></ng-include>'
        };

        return directive;
    }

    angular.module('umbraco.directives').directive('wfTableRow', TableRowDirective);

}());