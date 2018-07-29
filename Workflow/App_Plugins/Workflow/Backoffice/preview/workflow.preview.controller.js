(function () {
    const app = angular.module('workflow.preview', []).controller('workflow.preview.controller', function ($scope) {

        $scope.isOpen = false;
        $scope.frameLoaded = false;

        const segments = window.location.pathname.split('/');

        if (segments && segments.length === 5) {
            $scope.pageUrl = `/${segments[2]}`;
        }
       
    }).directive('iframeIsLoaded', function () {
        return {
            restrict: 'A',
            link: function (scope, element) {
                element.load(function () {
                    const iframe = element.context.contentWindow || element.context.contentDocument;

                    if (iframe && iframe.document.getElementById('umbracoPreviewBadge'))
                        iframe.document.getElementById('umbracoPreviewBadge').style.display = 'none';

                    if (!document.getElementById('resultFrame').contentWindow.refreshLayout) {
                        scope.frameLoaded = true;
                        scope.$apply();
                    }
                });
            }
        };
    });
}());