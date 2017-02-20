(function () {
    'use strict';

    function viewController($scope, $routeParams) {
        $scope.templatePartialUrl = '../App_Plugins/Workflow/backoffice/tree/' + $routeParams.id.replace('%20', '-').replace(' ', '-') + '.html';

        $scope.$on('loadStateChange', function (e, args) {
            $scope.loading = args.state;
        });
    }

    angular.module('umbraco').controller('WorkflowTree.View.Controller', ['$scope', '$routeParams', viewController]);

}());