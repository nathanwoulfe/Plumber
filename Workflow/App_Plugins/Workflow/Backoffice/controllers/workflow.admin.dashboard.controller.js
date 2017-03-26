(function () {
    'use strict';

    function dashboardController(workflowResource, localizationService) {

        var vm = this,
            _MS_PER_DAY = 1000 * 60 * 60 * 24,
            now = new Date();

        function getForRange() {
            if (vm.range > 0) {
                vm.loaded = false;
                vm.totalApproved = vm.totalCancelled = vm.totalPending = vm.totalRejected = 0;

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
                active = [],
                s, o,
                isTask = vm.type === 'Task',
                d = new Date();

            d.setDate(d.getDate() - vm.range);
            var then = Date.UTC(d.getFullYear(), d.getMonth(), d.getDate());

            var spline = {
                type: 'spline',
                name: 'Pending ' + vm.type.toLowerCase() + 's',
                data: defaultData(),
                colorIndex: 2,
                pointStart: then,
                pointInterval: _MS_PER_DAY
            };

            items.forEach(function (v, i) {
                var statusName = isTask ? v.statusName : v.status;

                if (statusName !== 'Pending Approval') {
                    if (seriesNames.indexOf(statusName) === -1) {
                        o = {
                            name: statusName,
                            type: 'column',
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

                    if (statusName === 'Approved') {
                        vm.totalApproved += 1;
                        s.colorIndex = 0;
                    }
                    else if (statusName === 'Rejected') {
                        vm.totalRejected += 1;
                        s.colorIndex = 2;
                    }
                    else {
                        vm.totalCancelled += 1;
                        s.colorIndex = 1;
                    }

                } else {
                    spline.data[vm.range + dateDiffInDays(now, new Date(isTask ? v.createdDate : v.requestedOn))] += 1;
                    vm.totalPending += 1;
                }
            });

            spline.data.forEach(function (d, i) {
                if (i > 0) {
                    spline.data[i] += spline.data[i - 1];
                }
            });
            series.push(spline);

            vm.series = series.sort(function (a, b) { return a.name > b.name; });

            vm.title = 'Workflow ' + vm.type.toLowerCase() + ' activity';
            vm.loaded = true;
        }

        // a and b are javascript Date objects
        function dateDiffInDays(a, b) {
            // Discard the time and time-zone information.
            var utc1 = Date.UTC(a.getFullYear(), a.getMonth(), a.getDate());
            var utc2 = Date.UTC(b.getFullYear(), b.getMonth(), b.getDate());

            return Math.floor((utc2 - utc1) / _MS_PER_DAY - 1);
        }

        function defaultData() {
            var arr = [];
            for (var i = 0; i <= vm.range; i += 1) {
                arr.push(0);
            }
            return arr;
        }

        // kick it off with a four-week span
        angular.extend(vm, {
            range: 28,
            type: 'Task',
            loaded: false,
            totalApproved: 0,
            totalCancelled: 0,
            totalPending: 0,
            totalRejected: 0,

            getForRange: getForRange
        });

        getForRange();
    }

    angular.module('umbraco').controller('Workflow.AdminDashboard.Controller', dashboardController);
}());