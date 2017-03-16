(function () {
    'use strict';

    function dashboardController(workflowResource) {

        var vm = this,
            _MS_PER_DAY = 1000 * 60 * 60 * 24,
            now = new Date();

        function getForRange() {
            if (vm.range > 0) {
                vm.loaded = false;
                if (vm.type === 'Task') {
                    workflowResource.getAllTasksForRange(vm.range)
                        .then(function (resp) {
                            lineChart(resp.items);
                        });
                } else {
                    workflowResource.getAllInstancesForRange(vm.range)
                        .then(function (resp) {
                            lineChart(resp.items);
                        });
                }
            }
        }

        function lineChart(items) {

            var series = [],
                seriesNames = [],
                s, o,
                isTask = vm.type === 'Task',
                d = new Date();

            d.setDate(d.getDate() - vm.range);
            var then = Date.UTC(d.getFullYear(), d.getMonth(), d.getDate());

            items.forEach(function (v, i) {
                var statusName = isTask ? v.statusName : v.status;
                if (seriesNames.indexOf(statusName) === -1) {
                    o = {
                        name: statusName,
                        data: defaultData(),
                        pointStart: then,
                        pointInterval: _MS_PER_DAY
                    };
                    series.push(o);
                    seriesNames.push(statusName);
                }

                s = series.filter(function (s) {
                    return s.name === statusName;
                })[0];

                s.data[vm.range + dateDiffInDays(now, new Date(isTask ? v.createdDate : v.requestedOn))] += 1;
            });

            vm.series = series.sort(function (a, b) { return a.name > b.name; });
            vm.title = 'Workflow ' + vm.type.toLowerCase() + ' activity';
            vm.loaded = true;
        }

        // a and b are javascript Date objects
        function dateDiffInDays(a, b) {
            // Discard the time and time-zone information.
            var utc1 = Date.UTC(a.getFullYear(), a.getMonth(), a.getDate());
            var utc2 = Date.UTC(b.getFullYear(), b.getMonth(), b.getDate());

            return Math.floor((utc2 - utc1) / _MS_PER_DAY);
        }

        function defaultData() {
            var arr = [];
            for (var i = 0; i < vm.range; i += 1) {
                arr.push(0);
            }
            return arr;
        }

        // kick it off with a four-week span
        angular.extend(vm, {
            range: 28,
            type: 'Task',
            loaded: false,            

            getForRange: getForRange
        });

        getForRange();
    }

    angular.module('umbraco').controller('Workflow.AdminDashboard.Controller', dashboardController);
}());