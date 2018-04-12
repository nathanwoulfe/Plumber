(function () {
    'use strict';

    function dashboardController(workflowResource) {

        var vm = this,
            storeKey = 'plumberUpdatePrompt',
            msPerDay = 1000 * 60 * 60 * 24,
            now = moment();

        function lineChart(items) {

            var series = [],
                seriesNames = [],
                s, o, 
                isTask = vm.type === 'Task';

            const d = new Date();

            d.setDate(d.getDate() - vm.range);
            var then = Date.UTC(d.getFullYear(), d.getMonth(), d.getDate());

            var created = {
                name: 'Total (cumulative)',
                type: 'spline',
                data: defaultData(),
                pointStart: then,
                pointInterval: msPerDay,
                className: 'wf-highcharts-color-total',
                lineWidth: 4,
                marker: {
                    enabled: false,
                    fillColor: null,
                    lineColor: null
                }
            };

            items.forEach(v => {
                var statusName = isTask ? v.statusName : v.status;

                // bit messy, but need to modify some returned name values
                // type 1|3 status 7 -> rejected
                // type 2 status 7 -> resubmit
                // status 3 -> pending
                // status 1 -> approved
                // status 4 -> not required

                if (v.type !== 2 && v.status === 7) {
                    statusName = v.statusName = 'Rejected';
                }
                else if (v.type === 2 && v.status === 7) {
                    statusName = v.statusName = 'Resubmitted';
                }

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

                    s = series.filter(ss => ss.name === statusName)[0];

                    s.data[vm.range - now.diff(moment(isTask ? v.completedDate : v.completedOn), 'days')] += 1;
                    created.data[vm.range - now.diff(moment(isTask ? v.createdDate : v.requestedOn), 'days')] += 1;

                    if (statusName === 'Approved') {
                        vm.totalApproved += 1;
                        s.className = 'wf-highcharts-color-approved';
                    }
                    else if (statusName === 'Rejected') {
                        vm.totalRejected += 1;
                        s.className = 'wf-highcharts-color-rejected';
                    }
                    else if (statusName === 'Resubmitted') {
                        vm.totalResubmitted += 1;
                        s.className = 'wf-highcharts-color-resubmitted';
                    }
                    else if (statusName === 'Not Required') {
                        vm.totalNotRequired += 1;
                        s.className = 'wf-highcharts-color-notreq';
                    }
                    else {
                        vm.totalCancelled += 1;
                        s.className = 'wf-highcharts-color-cancelled';
                    }

                } else {
                    const index = vm.range - now.diff(moment(isTask ? v.createdDate : v.requestedOn), 'days');
                    created.data[index < 0 ? 0 : index] += 1;
                    vm.totalPending += 1;
                }
            });

            created.data.forEach((v, i) => {
                if (i > 0) {
                    created.data[i] += created.data[i - 1];
                }
            });
            series.push(created);

            vm.series = series.sort((a, b) => a.name > b.name);

            vm.title = `Workflow ${vm.type.toLowerCase()} activity`;
            vm.loaded = true;
        }

        /**
         * Returns an array of 0s, length equal to the selected range
         */
        function defaultData() {
            return Array(vm.range).fill([]).map(() => 0);
        }

        function getForRange() {
            if (vm.range > 0) {

                vm.totalApproved = 0;
                vm.totalCancelled = 0;
                vm.totalPending = 0;
                vm.totalRejected = 0;
                vm.totalResubmitted = 0;
                vm.totalNotRequired = 0;

                vm.loaded = false;
                vm.totalApproved = vm.totalCancelled = vm.totalPending = vm.totalRejected = 0;

                if (vm.type === 'Task') {
                    workflowResource.getAllTasksForRange(vm.range)
                        .then(resp => {
                            lineChart(resp.items);
                        });
                } else {
                    workflowResource.getAllInstancesForRange(vm.range)
                        .then(resp => {
                            lineChart(resp.items);
                        });
                }
            }
        }

        // check the current installed version against the remote on GitHub, only if the 
        // alert has never been dismissed, or was dismissed more than 7 days ago
        const pesterDate = localStorage.getItem(storeKey);

        if (!pesterDate || moment(pesterDate).isBefore(now)) {
            workflowResource.getVersion()
                .then(resp => {
                    vm.version = resp;
                });
        }

        function updateAlertHidden() {
            localStorage.setItem(storeKey, now.add(7, 'days'));
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
            totalResubmitted: 0,
            totalNotRequired: 0,

            getForRange: getForRange,
            updateAlertHidden: updateAlertHidden
        });

        getForRange();
    }

    angular.module('umbraco').controller('Workflow.AdminDashboard.Controller', ['plmbrWorkflowResource', dashboardController]);
}());