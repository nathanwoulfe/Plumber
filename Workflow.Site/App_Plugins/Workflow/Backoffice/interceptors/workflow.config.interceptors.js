/* register all interceptors 
 * 
 */
(function () {
    'use strict';

    angular.module('umbraco')
        .config(function ($httpProvider) {
            $httpProvider.interceptors.push('drawerButtonsInterceptor');
        });
})();