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
            intro: '=',
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

.directive('wfTasks', function () {
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

            $scope.approveTask = function (task) {
                $scope.$parent.vm.approveTask(task);
            };

            $scope.cancelTask = function (task) {                
                $scope.$parent.vm.cancelTask(task);
            };

            $scope.showDifferences = function (task) {
                $scope.$parent.vm.showDifferences(task);
            };
        },
        link: function (scope, element, attrs) {

            scope.buttonCount = function (task) {
                var i = 1; // always shows cancel option

                if (task.IsPublished === true) {
                    i++;
                }
                if (task.ShowActionLink === true) {
                    i++;
                }
                if (scope.editLink != 'false') {
                    i++;
                }

                return i;
            };

            /******** PAGING *******/
            scope.numPerPage = 10;
        }
    };
});

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