(() => {
    'use strict';

    function dashboardController(workflowResource, tourService) {

        const storeKey = 'plumberUpdatePrompt';
        const msPerDay = 1000 * 60 * 60 * 24;
        const now = moment();

        const lineChart = items => {

            var series = [],
                seriesNames = [], 
                s,
                o,
                isTask = this.type === 'Task';

            const d = new Date();

            d.setDate(d.getDate() - this.range);
            const then = Date.UTC(d.getFullYear(), d.getMonth(), d.getDate());

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
                } else if (v.type === 2 && v.status === 7) {
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

                    s.data[this.range - now.diff(moment(isTask ? v.completedDate : v.completedOn), 'days')] += 1;
                    created.data[this.range - now.diff(moment(isTask ? v.createdDate : v.requestedOn), 'days')] += 1;

                    if (statusName === 'Approved') {
                        this.totalApproved += 1;
                        s.className = 'wf-highcharts-color-approved';
                    } else if (statusName === 'Rejected') {
                        this.totalRejected += 1;
                        s.className = 'wf-highcharts-color-rejected';
                    } else if (statusName === 'Resubmitted') {
                        this.totalResubmitted += 1;
                        s.className = 'wf-highcharts-color-resubmitted';
                    } else if (statusName === 'Not Required') {
                        this.totalNotRequired += 1;
                        s.className = 'wf-highcharts-color-notreq';
                    } else {
                        this.totalCancelled += 1;
                        s.className = 'wf-highcharts-color-cancelled';
                    }

                } else {
                    const index = this.range - now.diff(moment(isTask ? v.createdDate : v.requestedOn), 'days');
                    created.data[index < 0 ? 0 : index] += 1;
                    this.totalPending += 1;
                }
            });

            created.data.forEach((v, i) => {
                if (i > 0) {
                    created.data[i] += created.data[i - 1];
                }
            });
            series.push(created);

            this.series = series.sort((a, b) => a.name > b.name);

            this.title = `Workflow ${this.type.toLowerCase()} activity`;
            this.loaded = true;
        };

        /**
         * Returns an array of 0s, length equal to the selected range
         */
        const defaultData = () => Array(this.range).fill([]).map(() => 0);

        const getForRange = () => {
            if (this.range > 0) {

                this.totalApproved = 0;
                this.totalCancelled = 0;
                this.totalPending = 0;
                this.totalRejected = 0;
                this.totalResubmitted = 0;
                this.totalNotRequired = 0;

                this.loaded = false;
                this.totalApproved = this.totalCancelled = this.totalPending = this.totalRejected = 0;

                workflowResource[this.type === 'Task' ? 'getAllTasksForRange' : 'getAllInstancesForRange'](this.range)
                    .then(resp => {
                        lineChart(resp.items);
                    });
            }
        };

        // check the current installed version against the remote on GitHub, only if the 
        // alert has never been dismissed, or was dismissed more than 7 days ago
        const pesterDate = localStorage.getItem(storeKey);

        if (!pesterDate || moment(new Date(pesterDate)).isBefore(now)) {
            workflowResource.getVersion()
                .then(resp => {
                    this.version = resp;
                });
        }

        const updateAlertHidden = () => {
            localStorage.setItem(storeKey, now.add(7, 'days'));
        };

        // start selected tour
        const launchTour = tourAlias => {
            tourService.getTourByAlias(tourAlias)
                .then(resp => {
                    tourService.startTour(resp);
                });
        }

        // kick it off with a four-week span
        angular.extend(this, {
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
            updateAlertHidden: updateAlertHidden,
            launchTour: launchTour
        });

        getForRange();
    }

    angular.module('umbraco').controller('Workflow.AdminDashboard.Controller', ['plmbrWorkflowResource', 'tourService', dashboardController]);
})();