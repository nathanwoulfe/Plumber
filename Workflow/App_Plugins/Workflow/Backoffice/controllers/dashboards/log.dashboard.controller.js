(() => {
    'use strict';

    function logController(workflowResource) {

        const refresh = () => {
            workflowResource.getLog()
                .then(resp => {
                    this.html = resp;
                });

            workflowResource.getLogDates()
                .then(resp => {
                    // resp is an array of log dates, where [0] is 'txt', for the current date as the source file is undated
                    this.datePickerConfig.minDate = resp.length > 1 ? moment(resp[1]) : moment();
                });
        };

        const datePickerChange = event => {
            // handle change for a valid date - fetch corresponding log file if date is ok
            if (event.date && event.date.isValid() && event.oldDate.isValid()) {
                const date = event.date.format('YYYY-MM-DD');
                workflowResource.getLog(date === moment().format('YYYY-MM-DD') ? '' : date)
                    .then(resp => {
                        this.html = resp;
                    });
            }
        };

        const datePickerError = () => {
            // handle error
        };

        angular.extend(this,
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

    angular.module('plumber').controller('Workflow.Log.Controller', ['plmbrWorkflowResource', logController]);
})();