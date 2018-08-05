(() => {
    'use strict';

    /**
     * Docs are fetched from a parsed markdown file on GitHub - it needs to be parsed into a JSON object to use the healthcheck-style UI
     * Keeping the raw file as markdown makes for easier editing, but does add some processing overhead on the client
     * @param {any} workflowResource
     */
    function dashboardController($timeout, workflowResource) {

        this.viewState = 'list';
        this.selectedDoc = {};

        /**
         * Allow links in docs to open other sections, based on simple matching on the hash and doc name
         */
        const openDocFromDoc = e => {
            e.preventDefault();
            // on click, get the anchor, find the correct section and switch to it
            const target = this.docs.filter(v => {
                var name = v.name.toLowerCase().replace(' ', '-');
                return name.indexOf(e.target.hash.substring(1)) === 0;
            })[0];

            if (target) {
                this.openDoc(target);
            }
        };

        const bindListeners = () => {
            $timeout(() => {
                var elms = document.querySelectorAll('.umb-healthcheck-group__details-check-description a');
                if (elms.length) {
                    for (let i = 0; i < elms.length; i += 1) {
                        elms[i].addEventListener('click', e => { openDocFromDoc(e); });
                    }
                }
            });
        };

        /**
         * 
         * @param {any} node
         * @param {any} index
         * @param {any} elements
         */
        const getContentForHeading = (node, index, elements) => {

            let html = '';

            for (let i = index + 1; i < elements.length; i += 1) {
                if (elements[i].tagName !== 'H3') {
                    html += elements[i].outerHTML;
                } else {
                    break;
                }
            }

            return html;
        };

        /**
         * 
         * @param {any} docs
         */
        const parseDocs = docs => {

            const parser = new DOMParser();
            const article = angular.element(parser.parseFromString(docs, 'text/html')).find('article');

            var elements = article.children();
            var json = [];

            angular.forEach(elements,
                (v, i) => {
                    if (v.tagName === 'H3') {
                        json.push({
                            name: v.innerText,
                            content: getContentForHeading(v, i, elements)
                        });
                    }
                });

            this.docs = json;
            this.loaded = true;
        };

        /**
         * 
         * @param {any} doc
         */
        this.openDoc = doc => {
            this.selectedDoc = doc;
            this.viewState = 'details';

            // this will only be the current open doc
            bindListeners();
        };

        /**
         * 
         * @param {any} state
         */
        this.setViewState = state => {
            this.viewState = state;
        };

        workflowResource.getDocs() 
            .then(docs => {
                if (docs === 'Documentation unavailable') {
                    this.noDocs = docs;
                    this.loaded = true;
                } else {
                    parseDocs(docs);
                }
            });
    }

    angular.module('plumber').controller('Workflow.DocsDashboard.Controller', ['$timeout', 'plmbrWorkflowResource', dashboardController]);

})();