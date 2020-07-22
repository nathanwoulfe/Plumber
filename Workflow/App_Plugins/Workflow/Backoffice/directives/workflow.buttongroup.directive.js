(() => {
    'use strict';

    function buttonGroupDirective($rootScope, $location, editorState, workflowActionsService) {

        const directive = {
            restrict: 'E',
            replace: true,
            templateUrl: '../app_plugins/workflow/backoffice/views/partials/workflowButtonGroup.html',
            require: '^form',
            scope: {
                defaultButton: '=?',
                subButtons: '=?',
                state: '=?',
                item: '=',
                type: '=',
                direction: '@?',
                float: '@?',
                drawer: '@?'
            },
            link: (scope, elm, attr, contentForm) => {
                scope.detail = item => {
                    scope.workflowOverlay = workflowActionsService.detail(item);
                };

                scope.state = 'init';

                if (!scope.drawer) {
                    const buttons = {
                        approveButton: {
                            labelKey: 'workflow_approveButton',
                            handler: item => {
                                scope.workflowOverlay = workflowActionsService.action(item, 'Approve', true);
                            }
                        },
                        editButton: {
                            labelKey: 'workflow_editButton',
                            handler: item => {
                                $location.path(`/content/content/edit/${item.nodeId}`);
                            }
                        },
                        cancelButton: {
                            labelKey: 'workflow_cancelButton',
                            cssClass: 'danger',
                            handler: item => {
                                scope.workflowOverlay = workflowActionsService.cancel(item, true);
                            }
                        },
                        rejectButton: {
                            labelKey: 'workflow_rejectButton',
                            cssClass: 'warning',
                            handler: item => {
                                scope.workflowOverlay = workflowActionsService.action(item, 'Reject', true);
                            }
                        }
                    };

                    const subButtons = [
                        [buttons.editButton, buttons.rejectButton, buttons.cancelButton],
                        [buttons.editButton],
                        [buttons.cancelButton]
                    ];

                    if (scope.type != 2) {
                        scope.defaultButton = scope.type === 0 ? buttons.approveButton : buttons.cancelButton;
                        scope.subButtons = subButtons[scope.type];
                    }

                    if (scope.item && scope.item.cssStatus === 'rejected') {
                        scope.defaultButton = buttons.editButton;
                        scope.subButtons = [buttons.cancelButton];
                    }

                    $rootScope.$on('workflowActioned', (event, data) => {
                        if (data.type && data.type.toLowerCase() === 'reject') {
                            scope.defaultButton = buttons.editButton;
                            scope.subButtons = [buttons.cancelButton];
                        }
                    });
                }

                // can watch the content form state in the directive, then broadcast the state change
                scope.$watch(
                    () => contentForm.$dirty,
                    newVal => $rootScope.$broadcast('contentFormDirty', newVal));

                $rootScope.$on('buttonStateChanged', (event, data) => {
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

    angular.module('plumber.directives').directive('workflowButtonGroup', ['$rootScope', '$location', 'editorState', 'plmbrActionsService', buttonGroupDirective]);

})();
