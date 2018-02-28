(function () {
    'use strict';

    function importexportController(workflowResource, notificationsService) {

        var vm = this;

        function doImport() {
            workflowResource.doImport(vm.importData)
                .then(function(resp) {
                    if (resp) {
                        notificationsService.success('SUCCESS', 'Plumber config imported successfully');
                    } else {
                        notificationsService.error('ERROR', 'Plumber config import failed');
                    }
                });
        }

        function doExport() {
            workflowResource.doExport()
                .then(function(resp) {
                    vm.exportData = JSON.stringify(resp);
                });
        }

        angular.extend(vm,
            {
                doImport: doImport,
                doExport: doExport
            });

    }

    angular.module('umbraco').controller('Workflow.ImportExport.Controller', importexportController);
}());