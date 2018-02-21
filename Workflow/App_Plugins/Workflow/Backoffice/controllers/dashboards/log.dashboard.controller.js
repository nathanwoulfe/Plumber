(function () {
    'use strict';

    function logController(workflowResource) {

        var log = this;

        function refresh() {
            workflowResource.getLog()
                .then(function(resp) {
                    log.html = resp;
                });

            workflowResource.getLogDates()
                .then(function (resp) {
                    log.dates = resp;
                    if (log.dates.length > 1) {
                        log.datePickerConfig.minDate = moment(log.dates[log.dates.length - 1]);
                    } else {
                        log.datePicker.Config.minDate = moment();
                    }
                });
        }

        function datePickerChange(event) {
            // handle change
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

    angular.module('umbraco').controller('Workflow.Log.Controller', logController);
}());