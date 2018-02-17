(function () {
    'use strict';

    function buttonGroupDirective($rootScope, editorState, workflowActionsService) {

        var directive = {
            restrict: 'E',
            replace: true,
            templateUrl: '../app_plugins/workflow/backoffice/partials/workflowButtonGroup.html',
            scope: {
                defaultButton: '=',
                subButtons: '=',
                state: '=?',
                item: '=',
                direction: '@?',
                float: '@?',
                drawer: '@?'
            }, 
            link: function (scope) {
                scope.detail = function (item) {
                    scope.workflowOverlay = workflowActionsService.detail(item);
                }

                scope.state = 'init';

                $rootScope.$on('buttonStateChanged', function (event, data) {
                    if (scope.item && scope.item.nodeId === data.id || editorState.current && editorState.current.id === data.id) {
                        scope.state = data.state;

                        if (editorState.current && scope.$parent.contentForm) {
                            // surely there's a better way...
                            scope.$parent.contentForm.$setPristine();
                        }
                    }
                });
            }
        };

        return directive;
    }

    angular.module('umbraco.directives').directive('workflowButtonGroup', buttonGroupDirective);

}());
