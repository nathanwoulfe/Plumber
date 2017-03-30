(function () {
    'use strict';

    function historyDirective() {

        var directive = {
            restrict: 'E',
            scope: {
                items: '=',
                instanceView: '='
            },
            templateUrl: '../app_plugins/workflow/backoffice/partials/workflowhistorytemplate.html'
        };

        return directive;
    }

    angular.module('umbraco.directives').directive('wfHistory', historyDirective);

}());