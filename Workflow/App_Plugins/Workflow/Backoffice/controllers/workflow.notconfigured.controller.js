(() => {

    function notConfiguredController(notificationsService) {

        this.nodeNames = notificationsService.current[0].args.nodes;

        this.discard = not => {
            notificationsService.remove(not);
        };

    }

    // register controller 
    angular.module('plumber').controller('Workflow.NotConfigured.Controller', ['notificationsService', notConfiguredController]);
})();