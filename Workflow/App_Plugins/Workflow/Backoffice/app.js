(() => {

    angular.module('plumber.directives', []);
    angular.module('plumber.filters', []);
    angular.module('plumber.services', []);

    angular.module('plumber', [
        'plumber.directives',
        'plumber.filters',
        'plumber.services'
    ]);

    angular.module('umbraco').requires.push('plumber');

})();