(function () {
    'use strict';

    function history() {

        var directive = {
            restrict: 'E',
            scope: {
                items: '=',
                instanceView: '=',
                groupHistoryView: '='
            },
            templateUrl: '../app_plugins/workflow/backoffice/partials/workflowhistorytemplate.html'
        };

        return directive;
    }

    angular.module('umbraco.directives').directive('wfHistory', history);

}());