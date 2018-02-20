(function () {
    'use strict';

    function logController(workflowResource) {

        var log = this;

        function refresh() {
            workflowResource.getLog()
                .then(function(resp) {
                    log.html = resp;
                });
        }

        angular.extend(log,
            {
                simple: true,
                filter: 'all',

                refresh: refresh
            });

        refresh();

    }

    angular.module('umbraco').controller('Workflow.Log.Controller', logController);
}());