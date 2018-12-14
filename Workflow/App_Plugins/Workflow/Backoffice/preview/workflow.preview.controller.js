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
            const [, , nodeId, , taskId, ] = segments;

            this.pageUrl = `/${nodeId}`;

            if (!this.invalid) {
                this.action = actionName => {
                    workflowResource.getTask(taskId)
                        .then(resp => {
                            if (resp.task) {
                                this.workflowOverlay = workflowPreviewService.action(resp.task, actionName);
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