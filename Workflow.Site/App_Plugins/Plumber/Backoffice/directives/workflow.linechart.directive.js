(function () {
    'use strict';

    function lineChartDirective() {

        var directive = {
            restrict: 'E',
            template: '<div class="chart-container"><div></div></div>',
            scope: {
                series: '=',
                ready: '='
            },
            link: function (scope, element) {
                var el = element[0].querySelector('.chart-container div');
               
                scope.$watch('ready', function (newVal, oldVal) {
                    if (newVal === true) {
                        var options = {
                            credits: {
                                enabled:false
                            },
                            title: {
                                text: null
                            },
                            tooltip: {
                                shared: true,
                                formatter: function () {
                                    var r = this.points.filter(function (p, i) {
                                        return p.y > 0;
                                    }).length > 0;

                                    if (!r) { return false; }

                                    var s = '<span>' + new Date(this.x).toDateString() + '</span><br />';
                                    this.points.forEach(function (p, i) {
                                        s += '<span class="highcharts-color-' + i + '">\u25CF</span> ' + p.series.name + ': <b>' + p.y + '</b><br/>';
                                    });

                                    return s;
                                }
                            },
                            series: scope.series,
                            xAxis: {
                                type: 'datetime',
                                dateTimeLabelFormats: {
                                    day: '%b %e'
                                }
                            },
                            yAxis: {
                                allowDecimals: false,
                                minTickInterval:1,
                                title: {
                                    text: null
                                }
                            }
                        };

                        Highcharts.chart(el, options);
                    }
                });
            }
        };

        return directive;
    }

    angular.module('umbraco.directives').directive('wfLineChart', lineChartDirective);

}());
