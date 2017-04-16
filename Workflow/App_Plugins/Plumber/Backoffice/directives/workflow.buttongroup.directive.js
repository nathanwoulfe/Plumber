(function () {
    'use strict';

    function ButtonGroupDirective() {

        var directive = {
            restrict: 'E',
            replace: true,
            templateUrl: '../app_plugins/workflow/backoffice/partials/workflowButtonGroup.html',
            scope: {
                defaultButton: "=",
                subButtons: "=",
                state: "=?",
                item: "=",
                direction: "@?",
                float: "@?"
            }
        };

        return directive;
    }

    angular.module('umbraco.directives').directive('workflowButtonGroup', ButtonGroupDirective);

}());
