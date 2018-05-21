(function () {
    "use strict";

    function EditFileController($scope, $routeParams, editorState, appState, umbRequestHelper, navigationService, notificationsService, eventsService, tourResource) {
        var vm = this;

        var subviewsPath = "~/App_Plugins/TourEditor/backoffice/toureditor/subviews/";

        vm.page = {};

        vm.model = {
            "data": null,
            "aliases": [],
            "groups" : []
        };

        vm.page.loading = false;
        vm.page.menu = {};
        vm.page.menu.currentSection = appState.getSectionState("currentSection");
        vm.page.menu.currentNode = null;
        var evts = [];

        vm.page.navigation = [
            {
                "name": "Tour list",
                "icon": "",
                "view": umbRequestHelper.convertVirtualToAbsolutePath(subviewsPath + "tourlist.html"),
                "active": true
            },
            {
                "name": "Tour details",
                "icon": "",
                "view": umbRequestHelper.convertVirtualToAbsolutePath(subviewsPath + "tourdetails.html"),
                "active": false
            },
            {
                "name": "Steps details",
                "icon": "",
                "view": umbRequestHelper.convertVirtualToAbsolutePath(subviewsPath + "stepdetails.html"),
                "active": false
            }];

        function loadTourFile() {
            vm.page.loading = true;

            return loadAliases().then(function() {
                return tourResource.getTourFile($routeParams.id).then(
                    function(data) {                     
                        vm.model.data = data;

                        editorState.set(vm.model.data);

                        // get all aliases in current file
                        var fileAliases = _.map(vm.model.data.tours,
                            function(x) {
                                return x.alias;
                            });
                        

                        // combine the with aliases from other files
                        vm.model.aliases = vm.model.aliases.concat(fileAliases);

                        // get all groups in current file
                        var groups = _.map(vm.model.data.tours,
                            function (x) {
                                return x.group;
                            });


                        // combine unique the with groups from other files
                        vm.model.groups = vm.model.groups.concat(_.unique(groups));                      

                        vm.page.loading = false;
                    },
                    function(err) {
                        notificationsService.showNotification(err.data.notifications[0]);
                    }
                );
            });
        }

        function loadGroups() {
            return tourResource.getGroups($routeParams.id).then(
                function (data) {
                    vm.model.groups = data;                   
                },
                function (err) {
                    notificationsService.showNotification(err.data.notifications[0]);
                }
            );
        }

        function loadAliases() {

            return loadGroups().then(function() {
                return tourResource.getAliases($routeParams.id).then(
                    function (data) {
                        vm.model.aliases = data;
                    },
                    function (err) {
                        notificationsService.showNotification(err.data.notifications[0]);
                    }
                );
            });            
        }

        function updateTourChanges() {
            eventsService.emit('toureditor.updatetourchanges');            
        }

        vm.updateTourChanges = updateTourChanges;

        function discardTourChanges() {
            eventsService.emit('toureditor.discardtourchanges');
        }

        vm.discardTourChanges = discardTourChanges;

        function discardStepChanges() {
            eventsService.emit('toureditor.discardstepchanges');
        }

        vm.discardStepChanges = discardStepChanges;

        function updateStepChanges() {
            eventsService.emit('toureditor.updatestepchanges');            
        }

        vm.updateStepChanges = updateStepChanges;

        function saveTourFile() {
            tourResource.saveTourFile(vm.model.data).then(
                function (data) {
                    notificationsService.showNotification(data.notifications[0]);
                    loadTourFile();
                },
                function (err) {
                    notificationsService.showNotification(err.data.notifications[0]);
                });
        }

        vm.saveTourFile = saveTourFile;

        function init() {
            loadTourFile().then(function () {
                navigationService.syncTree({ tree: "toureditor", path: "-1," + $routeParams.id }).then(function (syncArgs) {
                    vm.page.menu.currentNode = syncArgs.node;
                });
            });

        }

        init();

        evts.push(eventsService.on("toureditor.stepchangesdiscarded", function (name, args) {
            vm.page.navigation[0].active = false;
            vm.page.navigation[1].active = true;
            vm.page.navigation[2].active = false;
        }));

        evts.push(eventsService.on("toureditor.stepchangesupdate", function (name, args) {
            vm.page.navigation[0].active = false;
            vm.page.navigation[1].active = true;
            vm.page.navigation[2].active = false;

            notificationsService.warning("Don't forget to save your changes",
                "To save your changes you also need to update the tour and save the file");
        }));

        evts.push(eventsService.on("toureditor.tourchangesdiscarded", function (name, args) {
            vm.page.navigation[0].active = true;
            vm.page.navigation[1].active = false;
            vm.page.navigation[2].active = false;
        }));

        evts.push(eventsService.on("toureditor.tourchangesupdate", function (name, args) {

            vm.model.data.tours[args.index] = args.tour;

            if (args.isNew) {
                // if it is a new one add the alias to the list
                vm.model.aliases.push(args.tour.alias);
            }

            // add the group to the list if it's a new one
            var group = args.tour.group;

            if (vm.model.groups.indexOf(group) === -1) {
                vm.model.groups.push(group);
            }

            vm.page.navigation[0].active = true;
            vm.page.navigation[1].active = false;
            vm.page.navigation[2].active = false;

            notificationsService.warning("Don't forget to save your changes",
                "To persist your changes you need to save the file");
        }));

        evts.push(eventsService.on("toureditor.edittour", function (name, args) {
            vm.page.navigation[0].active = false;
            vm.page.navigation[1].active = true;
            vm.page.navigation[2].active = false;
        }));

        evts.push(eventsService.on("toureditor.editstep", function (name, args) {
            vm.page.navigation[0].active = false;
            vm.page.navigation[1].active = false;
            vm.page.navigation[2].active = true;
        }));

        //ensure to unregister from all events!
        $scope.$on('$destroy', function () {
            for (var e in evts) {
                eventsService.unsubscribe(evts[e]);
            }
        });
    }


    angular.module("umbraco").controller("Our.Umbraco.TourEditor.EditFileController",
        [
            '$scope',
            '$routeParams',
            'editorState',
            'appState',
            'umbRequestHelper',
            'navigationService',
            'notificationsService',
            'eventsService',
            'Our.Umbraco.TourEditor.TourResource',
            EditFileController
        ]);

})();