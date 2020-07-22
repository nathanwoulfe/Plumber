(() => {
    function tasks() {

        const directive = {
            restrict: 'AEC',
            scope: {
                items: '=',
                type: '=',
                loaded: '='
            },
            templateUrl: '../app_plugins/workflow/backoffice/views/partials/workflowTasksTemplate.html',
            controller: function($scope) {

                // type = 0, 1
                // 0 -> full button set
                // 1 -> cancel, edit - this is reversed if the task is rejected
                // 2 -> no buttons
                $scope.noActions = $scope.type === 2;                
            }
        };

        return directive;
    }

    angular.module('plumber.directives').directive('wfTasks', [tasks]);

})();
