(function () {
  'use strict';

  function treeController($scope, $routeParams, eventsService, treeService) {
    var vm = this;

    vm.templatePartialUrl = '../App_Plugins/workflow/backoffice/tree/' + $routeParams.tree + '.html';

    $scope.$on('loadStateChange', function (e, args) {
      vm.loading = args.state;
    });

    // set the current node state in the menu 
    eventsService.on('appState.treeState.changed', function (event, args) {
      if (args.key === 'selectedNode' && args.value.routePath.indexOf('workflow') === 0) {
        event.currentScope.nav.syncTree({
          tree: args.value.routePath.split('/')[1],
          path: treeService.getPath(args.value),
          forceReload: false
        });
        event.currentScope.nav.changeSection('workflow');
      }
    });
  }

  angular.module('umbraco').controller('Workflow.Tree.Controller', treeController);

}());