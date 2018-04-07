(function () {
    'use strict';

    function buttonGroupDirective($rootScope, angularHelper, editorState, workflowActionsService) {

        var directive = {
            restrict: 'E',
            replace: true,
            templateUrl: '../app_plugins/workflow/backoffice/partials/workflowButtonGroup.html',
            require: '^form',
            scope: {
                defaultButton: '=',
                subButtons: '=',
                state: '=?',
                item: '=',
                direction: '@?',
                float: '@?',
                drawer: '@?'
            },
            link: function (scope, elm, attr, contentForm) {

                scope.detail = function (item) {
                    scope.workflowOverlay = workflowActionsService.detail(item);
                };

                scope.state = 'init';

                // can watch the content form state in the directive, then broadcast the state change
                scope.$watch(function () {
                        return contentForm.$dirty;
                    },
                    function (newVal) {
                        $rootScope.$broadcast('contentFormDirty', newVal);
                    });

                $rootScope.$on('buttonStateChanged', function (event, data) {
                    if (scope.item && scope.item.nodeId === data.id || editorState.current && editorState.current.id === data.id) {
                        scope.state = data.state;

                        // button might be in a dashboard, so need to check for content form before resetting form state
                        if (editorState.current && contentForm) {
                            contentForm.$setPristine();
                        }
                    }
                });
            }
        };

        return directive;
    }

    angular.module('umbraco.directives').directive('workflowButtonGroup', ['$rootScope', 'angularHelper', 'editorState', 'plmbrActionsService', buttonGroupDirective]);

}());
