(() => {

    function ctrl(workflowPreviewService, workflowResource) {

        this.frameLoaded = false;
        this.invalid = false;
        this.showActionPane = false;

        /**
         * Extract cookie
         * @param {any} a = the cookie key
         */
        const getCookie = a => {
            var d = [],
                e = document.cookie.split(';');

            a = RegExp(`^\\s*${a}=\\s*(.*?)\\s*$`);

            for (let b = 0; b < e.length; b++) {
                const f = e[b].match(a);
                if (f) {
                    d.push(f[1]);
                }
            }

            return d;
        };

        // if the cookie exists, the request is invalid 
        this.invalid = getCookie('Workflow_Preview').length > 0;

        const segments = window.location.pathname.split('/');
        if (segments && segments.length === 6) {
            // domain/path/nodeid/userid/taskid/guid
            // only need the node id
            const [, , nodeId, , , ] = segments;

            this.pageUrl = `/${nodeId}`;

            if (!this.invalid) {
                this.action = actionName => {
                    workflowResource.getNodePendingTasks(nodeId)
                        .then(resp => {
                            if (resp.items) {
                                this.workflowOverlay = workflowPreviewService.action(resp.items[0], actionName);
                            }
                        });
                };
            }
        }
    }

    angular.module('plumber').controller('workflow.preview.controller', ['workflowPreviewService', 'plmbrWorkflowResource', ctrl])

        .directive('iframeIsLoaded',
        function () {
            return {
                restrict: 'A',
                link: function(scope, element) {
                    element.load(function() {
                        const iframe = element.context.contentWindow || element.context.contentDocument;

                        if (iframe && iframe.document.getElementById('umbracoPreviewBadge')) {
                            iframe.document.getElementById('umbracoPreviewBadge').style.display = 'none';
                        }

                        if (!document.getElementById('resultFrame').contentWindow.refreshLayout) {
                            scope.frameLoaded = true;
                            scope.$apply();
                        }
                    });
                }
            };
        });

})();