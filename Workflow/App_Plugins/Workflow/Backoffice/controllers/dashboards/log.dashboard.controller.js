(function () {
    'use strict';

    function logController(workflowResource) {

        var log = this;

        function refresh() {
            workflowResource.getLog()
                .then(function (resp) {
                    log.html = resp;
                });

            workflowResource.getLogDates()
                .then(function (resp) {
                    // resp is an array of log dates, where [0] is 'txt', for the current date as the source file is undated
                    log.datePickerConfig.minDate = resp.length > 1 ? moment(resp[1]) : moment();
                });
        }

        function datePickerChange(event) {
            // handle change for a valid date - fetch corresponding log file if date is ok
            if (event.date && event.date.isValid() && event.oldDate.isValid()) {
                var date = event.date.format('YYYY-MM-DD');
                workflowResource.getLog(date === moment().format('YYYY-MM-DD') ? '' : date)
                    .then(function (resp) {
                        log.html = resp;
                    });
            }
        }

        function datePickerError() {
            // handle error
        }

        angular.extend(log,
            {
                simple: true,
                filter: 'all',
                datePickerConfig: {
                    defaultDate: moment(),
                    maxDate: moment(),
                    pickDate: true,
                    pickTime: false,
                    format: 'D MMM YYYY',
                    icons: {
                        time: 'icon-time',
                        date: 'icon-calendar',
                        up: 'icon-chevron-up',
                        down: 'icon-chevron-down'
                    }
                },

                refresh: refresh,
                datePickerChange: datePickerChange,
                datePickerError: datePickerError
            });

        refresh();

    }

    angular.module('umbraco').controller('Workflow.Log.Controller', ['plmbrWorkflowResource', logController]);
}());