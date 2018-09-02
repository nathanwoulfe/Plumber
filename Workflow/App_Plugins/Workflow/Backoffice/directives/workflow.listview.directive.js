(() => {

    // directives here are used to change the icon on nodes in a list view
    // fetches the status for all nodes in the current view
    // sets a class on the list view row if an active workflow exists
    // raises an event once that is complete
    // in the table row directive, the event triggers adding a class to the table row which changes the icon and title attribute
    function tableRow() {

        const directive = {
            restrict: 'C',
            link: (scope, element) => {

                scope.$on('listViewStatus',
                    () => {
                        if (scope.item && scope.item.activeWorkflow) {
                            element[0].classList.add('active-workflow');
                            element[0].childNodes.forEach(c => {
                                if (c.classList && c.classList.contains('umb-table-cell')) {
                                    c.setAttribute('title', 'Workflow active');
                                }
                            });
                        }
                    });
            }
        };

        return directive;
    }

    angular.module('plumber.directives').directive('umbTableRow', tableRow);

    function listview(workflowResource) {

        const directive = {
            restrict: 'C',
            link: scope => {

                const setIcons = nodes => {
                    scope.listViewResultSet.items.forEach(v => {
                        v.activeWorkflow = nodes[v.id] && nodes[v.id] === true;
                    });
                };

                scope.$watch(() => scope.listViewResultSet.items,
                    (a, b) => {
                        if (a.length && a !== b) {
                            scope.items = a;
                            scope.ids = scope.items.map(i => i.id);

                            workflowResource.getStatus(scope.ids.join(','))
                                .then(resp => {
                                    setIcons(resp.nodes);
                                    scope.$broadcast('listViewStatus');
                                });
                        }
                    });

            }
        };

        return directive;
    }

    angular.module('plumber.directives').directive('umbListview', ['plmbrWorkflowResource', listview]);

})();