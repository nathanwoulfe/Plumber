(() => {
    // replace the editor buttons with Plumber's version
    function interceptor($q) {
        return {
            request: req => {
                if (req.url.toLowerCase().indexOf('footer-content-right') !== -1) {
                    if (location.href.indexOf('content') !== -1) {
                        req.url = '../app_plugins/workflow/backoffice/partials/umbEditorFooterContentRight.html';
                    }
                }
                return req || $q.when(req);
            }
        };
    }

    angular.module('umbraco').factory('drawerButtonsInterceptor', ['$q', interceptor]);
})();