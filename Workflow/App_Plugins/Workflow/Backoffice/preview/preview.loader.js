LazyLoad.js([
    '/umbraco/lib/jquery/jquery.min.js',
    '/umbraco/lib/jquery-ui/jquery-ui.min.js',
    '/umbraco/lib/angular/1.1.5/angular.min.js',
    '/umbraco/lib/underscore/underscore-min.js',
    '/umbraco/lib/umbraco/Extensions.js',
    '/umbraco/js/app.js',
    '/umbraco/js/umbraco.resources.js',
    '/umbraco/js/umbraco.services.js',
    '/umbraco/js/umbraco.security.js',
    '/app_plugins/workflow/backoffice/preview/workflow.preview.controller.js'
], function () {
    jQuery(document).ready(function () {
        angular.bootstrap(document, ['workflow.preview']);
    });
});