angular.module('plumber.services', []);
angular.module('plumber.directives', []);
angular.module('plumber.filters', []);

angular.module('plumber', [
    'umbraco.security',
    'umbraco.resources',
    'umbraco.services',
    'umbraco.packages',
    'umbraco.directives',
    'plumber.services',
    'plumber.directives',
    'plumber.filters'
])

.config(function ($httpProvider) {
    $httpProvider.interceptors.push('plumberPreviewInterceptor');
});

var packages = angular.module('umbraco.packages', []);

/* register all interceptors 
 * 
 */
(() => {

    function interceptor($q) {
        return {
            request: req => {
                if (req.url.indexOf('LocalizedText') !== -1 || req.url.indexOf('views/components') !== -1) {
                    req.url = `/umbraco/${req.url}`;
                }
                return req || $q.when(req);
            }
        };
    }

    angular.module('plumber').factory('plumberPreviewInterceptor', ['$q', interceptor]);

})();