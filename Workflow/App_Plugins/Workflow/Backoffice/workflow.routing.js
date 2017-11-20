(function() {
  'use strict';

  var app = angular.module('umbraco');

  app.config(function ($routeProvider) {
    $routeProvider.when('/workflow/:tree',
      {
        template: '<div ng-include="\'/app_plugins/workflow/backoffice/tree/view.html\'"></div>'
      });
  });
}());