/* register all interceptors 
 * 
 */
(() => {
    'use strict';

    angular.module('umbraco')
        .config(function($httpProvider) {
            $httpProvider.interceptors.push('drawerButtonsInterceptor');
        });
})();