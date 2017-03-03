angular.module('umbraco').directive('nodeName', function (contentResource) {
    return {
        restrict: 'E',
        link: function (scope, element, attr) {
            contentResource.getById(attr.nodeId)
                .then(function (resp) {
                    element.html(resp.name);
                });
        }
    }
});

angular.module('umbraco').directive('wfComments', function () {
    return {
        restrict: 'AEC',
        scope: {
            labelText: '=',
            comment: '=',
            limit: '=',
            isFinalApproval: '=',
            disabled: '='
        },
        template: '<p ng-bind="intro"></p><label for="comments">{{labelText}} <span ng-bind="info"></span><textarea name="comments" ng-model="comment" ng-change="limitChars()"></textarea>',
        link: function ($scope) {
            $scope.limitChars = function () {

                var limit = $scope.limit;

                if ($scope.comment.length > limit) {
                    $scope.info = '(Comment max length exceeded - limit is ' + limit + ' characters.)';
                    $scope.comment = $scope.comment.substr(0, limit);
                } else {
                    $scope.info = '(' + (limit - $scope.comment.length) + ' characters remaining.)';
                }

                if (!$scope.isFinalApproval) {
                    if ($scope.comment.length > 0) {
                        $scope.disabled = false;
                    } else {
                        $scope.disabled = true;
                    }
                }
            };
        }
    };
})

.directive('wfTasks', function (dialogService, notificationsService) {
    return {
        restrict: 'AEC',
        scope: {
            heading: '=',
            items: '=',
            editLink: '=',
            loaded: '='
        },
        templateUrl: 'wf-tasks-template.html',
        controller: function ($scope) {

            function showDialog(url, item, cb) {
                dialogService.open({
                    template: url,
                    show: true,
                    dialogData: item,
                    callback: function(resp) {
                        if (cb) {
                            if (resp.status === 200) {
                                notificationsService.success("SUCCESS!", resp.data.Message);
                            }
                            else {
                                notificationsService.error("OH SNAP!", resp.data.Message);
                            }

                            $scope.$parent.vm.init();
                        }
                    }
                });
            };

            var buttons = {
                approveButton: {
                    labelKey: "workflow_approveButton",
                    handler: function (item) {
                        showDialog('../app_plugins/workflow/backoffice/dialogs/workflow.approve.dialog.html', item, true);
                    }
                },
                editButton: {
                    labelKey: "workflow_editButton",
                    href: '/umbraco#/content/content/edit/',
                    handler: function (item) {
                        window.location = this.href + item.NodeId;
                    }
                },
                cancelButton: {
                    labelKey: "workflow_cancelButton",
                    cssClass: 'danger',
                    handler: function (item) {
                        showDialog('../app_plugins/workflow/backoffice/dialogs/workflow.cancel.dialog.html', item, true);
                    }
                },                
                rejectButton: {
                    labelKey: "workflow_rejectButton",
                    cssClass: 'warning',
                    handler: function (item) {
                        showDialog('../app_plugins/workflow/backoffice/dialogs/workflow.reject.dialog.html', item, true);
                    }
                },
                diffsButton: {
                    labelKey: "workflow_diffsButton",
                    handler: function (item) {
                        showDialog('../app_plugins/workflow/backoffice/dialogs/workflow.differences.dialog.html', item);
                    }
                }
            };

            $scope.buttonGroup = {
                defaultButton: buttons.approveButton,
                subButtons: [
                    buttons.editButton,
                    buttons.diffsButton,
                    buttons.rejectButton,
                    buttons.cancelButton
                ]
            };            
        },
        link: function (scope, element, attrs) {

            /******** PAGING *******/
            scope.numPerPage = 10;
        }
    };
});

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

})();


(function () {
    'use strict';
    function pagingControls(workflowPagingService) {
        return {
            restrict: 'E',
            replace: true,
            templateUrl: '/App_Plugins/Workflow/backoffice/partials/paginationTemplate.html',

            controller: function ($scope, $element, $attrs) {

                function setPaging() {
                    var paging = workflowPagingService.updatePaging($scope.items, ($scope.filter !== undefined ? $scope.filter : ''), ($scope.currentPage === undefined ? 1 : $scope.currentPage), $scope.numPerPage);
                    $scope.filter = paging.filter;
                    $scope.currentPage = paging.currentPage;
                    $scope.numPages = paging.numPages;
                    $scope.pagedItems = paging.items;
                };

                $scope.prev = function () {
                    $scope.currentPage = workflowPagingService.prevPage($scope.currentPage);
                    setPaging();
                };

                $scope.next = function () {
                    $scope.currentPage = workflowPagingService.nextPage($scope.currentPage, $scope.numPages);
                    setPaging();
                };

                $scope.$watch('items', function (newVal) {
                    if (newVal !== undefined && newVal.length)
                        setPaging();
                }, true);
            }
        };
    }

    angular.module("umbraco.directives").directive('wfPagingControls', pagingControls);

}());