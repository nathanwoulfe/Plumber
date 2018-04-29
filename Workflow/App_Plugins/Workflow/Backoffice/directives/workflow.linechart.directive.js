(() => {
    'use strict';

    function lineChart() {

        const directive = {
            restrict: 'E',
            template: '<div class="chart-container"><div></div></div>',
            scope: {
                series: '=',
                ready: '='
            },
            link: (scope, element) => {
                const el = element[0].querySelector('.chart-container div');

                scope.$watch('ready', newVal => {
                    if (newVal === true) {
                         const options = {
                            credits: {
                                enabled: false
                            },
                            title: {
                                text: null
                            },
                            legend: {
                                itemStyle: {
                                    fontSize: '15px'
                                }
                            },
                            tooltip: {
                                shared: true,
                                formatter: function() {
                                    const r = this.points.filter(p => p.y > 0).length > 0;

                                    if (!r) {
                                        return false;
                                    }

                                    var s = `<span>${new Date(this.x).toDateString()}</span><br />`;

                                    this.points.forEach(p => {
                                        if (p.y > 0) {
                                            s += `<span class="wf-highcharts-color-${p.series.name.toLowerCase().replace(' ', '-')}">\u25CF</span> ${p.series.name}: <b>${p.y}</b><br/>`;
                                        }
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
                                minTickInterval: 1,
                                min: 0,
                                type: 'logarithmic',
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

    angular.module('umbraco.directives').directive('wfLineChart', lineChart);

})();
