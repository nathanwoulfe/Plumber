(function () {
    'use strict';

    /**
     * Docs are fetched from a parsed markdown file on GitHub - it needs to be parsed into a JSON object to use the healthcheck-style UI
     * Keeping the raw file as markdown makes for easier editing, but does add some processing overhead on the client
     * @param {any} workflowResource
     */
    function dashboardController($timeout, workflowResource) {
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

            // check for anchors between sections - these need to be handled by angular
            //var links = article.find('a:not(.anchor)');
            //if (links.length) {
            //    for (i = 0; i < links.length; i += 1) {
            //        if (links[i].hash) {
            //            links[i].setAttribute('ng-click', 'vm.docsLinkClick($event)');
            //        }
            //    }
            //}

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

            // this will only be the current open doc
            bindListeners();
        }

        function setViewState(state) {
            vm.viewState = state;
        }

        /**
         * Allow links in docs to open other sections, based on simple matching on the hash and doc name
         */
        function bindListeners() {
            $timeout(function () {
                var elms = document.querySelectorAll('.umb-healthcheck-group__details-check-description a');
                if (elms.length) {
                    for (i = 0; i < elms.length; i += 1) {
                        elms[i].addEventListener('click',
                            function (e) {
                                e.preventDefault();
                                // on click, get the anchor, find the correct section and switch to it
                                var target = vm.docs.filter(function (v) {
                                    var name = v.name.toLowerCase().replace(' ', '-');
                                    return name.indexOf(e.target.hash.substring(1)) === 0;
                                })[0];

                                if (target) {
                                    openDoc(target);
                                }
                            });
                    }
                }
            });
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
                setViewState: setViewState,
            });
    }

    angular.module('umbraco').controller('Workflow.DocsDashboard.Controller', dashboardController);

}());