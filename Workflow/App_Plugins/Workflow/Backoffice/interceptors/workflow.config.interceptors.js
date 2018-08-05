/* register all interceptors 
 * 
 */
(() => {
    'use strict';

    angular.module('plumber')
        .config(function($httpProvider) {
            $httpProvider.interceptors.push('drawerButtonsInterceptor');
        });
})();