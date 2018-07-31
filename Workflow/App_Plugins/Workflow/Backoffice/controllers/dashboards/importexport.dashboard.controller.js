(function () {
    'use strict';

    function importexportController(workflowResource, notificationsService) {

        this.doImport = () => {
            workflowResource.doImport(this.importData)
                .then(resp => {
                    if (resp) {
                        notificationsService.success('SUCCESS', 'Plumber config imported successfully');
                    } else {
                        notificationsService.error('ERROR', 'Plumber config import failed');
                    }
                });
        };

        this.doExport = () => {
            workflowResource.doExport()
                .then(resp => {
                    this.exportData = JSON.stringify(resp);
                });
        };

    }

    angular.module('plumber').controller('Workflow.ImportExport.Controller', ['plmbrWorkflowResource', 'notificationsService', importexportController]);
}());