(function () {
    'use strict';

    function lineChartDirective() {

        var directive = {
            restrict: 'E',
            templateUrl: '../app_plugins/workflow/backoffice/partials/workflowLineChart.html',
            scope: {
                options: '='
            },
            link: function(scope, element) {
                Highcharts.chart(element[0], scope.options);
            }
        };

        return directive;
    }

    angular.module('umbraco.directives').directive('wfLineChart', lineChartDirective);

}());
