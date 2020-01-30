LazyLoad.js([
    '/umbraco/lib/jquery/jquery.min.js',
    '/umbraco/lib/angular/1.1.5/angular.min.js',
    '/umbraco/lib/angular/angular-ui-sortable.js',
    '/umbraco/lib/underscore/underscore-min.js',
    '/umbraco/lib/umbraco/Extensions.js',

    '/umbraco/js/umbraco.services.js', 
    '/umbraco/js/umbraco.resources.js',
    '/umbraco/js/umbraco.security.js',
    '/umbraco/js/umbraco.directives.js',

    '/app_plugins/workflow/backoffice/preview/app.js',

    '/app_plugins/workflow/backoffice/controllers/workflow.action.controller.js',
    '/app_plugins/workflow/backoffice/filters/workflow.iconName.filter.js',
    '/app_plugins/workflow/backoffice/directives/workflow.comments.directive.js',

    '/app_plugins/workflow/backoffice/preview/workflow.preview.services.js',
    '/app_plugins/workflow/backoffice/preview/workflow.preview.controller.js'
], function () {
    jQuery(document).ready(function () {
        angular.bootstrap(document, ['plumber']);
    });
    });

LazyLoad.css(['/app_plugins/workflow/backoffice/css/styles.css', '/app_plugins/workflow/backoffice/css/preview.css']);