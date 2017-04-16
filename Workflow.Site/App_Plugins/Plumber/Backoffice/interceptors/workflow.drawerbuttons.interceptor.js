(function () {
    // replace the editor buttons if the page is in a workflow and the user has approval rights
    function interceptor($q) {
        return {
            request: function (request) {
                if (request.url.toLowerCase().indexOf('footer-content-right') !== -1) {
                    if (location.href.indexOf('content') !== -1) {
                        request.url = '/App_Plugins/workflow/backoffice/partials/umbEditorFooterContentRight.html';
                    }
                }
                return request || $q.when(request);
            }
        };
    }

    angular.module('umbraco').factory('drawerButtonsInterceptor', ['$q', interceptor]);
})();