(function () {
    'use strict';

    function treeController($scope, $routeParams, eventsService) {
        $scope.templatePartialUrl = '../App_Plugins/Workflow/backoffice/tree/' + $routeParams.id.replace('%20', '-').replace(' ', '-') + '.html';

        $scope.$on('loadStateChange', function (e, args) {
            $scope.loading = args.state;
        });

        // set the current node state in the menu 
        eventsService.on('appState.treeState.changed', function (event, args) {
            
            if (args.key === 'selectedNode') {

                function buildPath(node, path) {
                    path.push(node.id);
                    if (node.id === '-1') return path.reverse();
                    var parent = node.parent(); 
                    if (parent === undefined) return path;
                    return buildPath(parent, path);
                }

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