(function () {
    'use strict';

    function dashboardController(workflowResource) {

        var vm = this,
            msPerDay = 1000 * 60 * 60 * 24,
            now = new moment();

        function lineChart(items) {

            var series = [],
                seriesNames = [],
                s, o,
                isTask = vm.type === 'Task',
                d = new Date();

            d.setDate(d.getDate() - vm.range);
            var then = Date.UTC(d.getFullYear(), d.getMonth(), d.getDate());

            var created = {
                name: 'Total (cumulative)',
                type: 'spline',
                data: defaultData(),
                pointStart: then,
                pointInterval: msPerDay,
                colorIndex: 5,
                lineWidth: 4,
                marker: {
                    enabled: false
                }
            };

            items.forEach(function (v) {
                var statusName = isTask ? v.statusName : v.status;

                if (statusName !== 'Pending Approval') {
                    if (seriesNames.indexOf(statusName) === -1) {
                        o = {
                            name: statusName,
                            type: 'column',
                            data: defaultData(),
                            pointStart: then,
                            pointInterval: msPerDay
                        };
                        series.push(o);
                        seriesNames.push(statusName);
                    }

                    s = series.filter(function (s) {
                        return s.name === statusName;
                    })[0];

                    s.data[vm.range - now.diff(moment(isTask ? v.completedDate : v.completedOn), 'days')] += 1;
                    created.data[vm.range - now.diff(moment(isTask ? v.createdDate : v.requestedOn), 'days')] += 1;

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
                    var index = vm.range - now.diff(moment(isTask ? v.createdDate : v.requestedOn));
                    created.data[index < 0 ? 0 : index] += 1;
                    vm.totalPending += 1;
                }
            });

            created.data.forEach(function(d, i) {
                if (i > 0) {
                    created.data[i] += created.data[i - 1];
                }
            });
            series.push(created);

            vm.series = series.sort(function (a, b) { return a.name > b.name; });

            vm.title = 'Workflow ' + vm.type.toLowerCase() + ' activity';
            vm.loaded = true;
        }

        function defaultData() {
            var arr = [];
            for (var i = 0; i <= vm.range; i += 1) {
                arr.push(0);
            }
            return arr;
        }

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