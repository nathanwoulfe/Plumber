(function () {
    'use strict';

    /**
     * Docs are fetched from a parsed markdown file on GitHub - it needs to be parsed into a JSON object to use the healthcheck-style UI
     * Keeping the raw file as markdown makes for easier editing, but does add some processing overhead on the client
     * @param {any} workflowResource
     */
    function dashboardController(workflowResource) {
        var vm = this,
            i;

        function getContentForHeading(node, index, elements) {

            var html = '';

            for (i = index + 1; i < elements.length; i += 1) {
                if (elements[i].tagName !== 'H3') {
                    html += elements[i].outerHTML;
                } else {
                    break;
                }
            }

            return html;
        }

        function parseDocs(docs) {
            
            var parser = new DOMParser();
            var article = angular.element(parser.parseFromString(docs, 'text/html')).find('article');
            var elements = article.children();

            var json = [];

            angular.forEach(elements, function (v, i) {
                if (v.tagName === 'H3') {
                    var o = {};
                    o['name'] = v.innerText;
                    o['content'] = getContentForHeading(v, i, elements);

                    json.push(o);
                }
            });

            vm.docs = json;
            vm.loaded = true;

        }

        function openDoc(doc) {
            vm.selectedDoc = doc;
            vm.viewState = 'details';
        }

        function setViewState(state) {
            vm.viewState = state;
        }

        workflowResource.getDocs()
            .then(function(docs) {
                parseDocs(docs);
            });


        angular.extend(vm,
            {
                viewState: 'list',
                selectedDoc: {},

                openDoc: openDoc,
                setViewState: setViewState
            });
    }

    angular.module('umbraco').controller('Workflow.DocsDashboard.Controller', dashboardController);

}());