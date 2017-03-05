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

                    console.log($scope.numPages);
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