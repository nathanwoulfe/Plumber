(function () {
    'use strict';

    function treeController($scope, $routeParams, eventsService) {
        var vm = this;

        vm.templatePartialUrl = '../App_Plugins/workflow/backoffice/tree/' + $routeParams.id.replace('%20', '-').replace(' ', '-') + '.html';

        $scope.$on('loadStateChange', function (e, args) {
            vm.loading = args.state;
        });

        function buildPath(node, path) {
            path.push(node.id);

            if (node.id === '-1') {
                return path.reverse();
            }

            var parent = node.parent();

            if (parent === undefined) {
                return path;
            }

            return buildPath(parent, path);
        }

        // set the current node state in the menu 
        eventsService.on('appState.treeState.changed', function (event, args) {            
            if (args.key === 'selectedNode') {
                event.currentScope.nav.syncTree({
                    tree: $routeParams.tree || 'tree',
                    path: buildPath(args.value, []),
                    forceReload: false
                });
            }
        });
    }

    angular.module('umbraco').controller('Workflow.Tree.Controller', treeController);

}());