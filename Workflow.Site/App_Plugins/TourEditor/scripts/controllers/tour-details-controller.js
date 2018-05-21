(function () {
    "use strict";

    function TourDetailsController($scope, eventsService, sectionResource, formHelper) {
        var vm = this;
        vm.tour = null;
        vm.tourIndex = -1;
        vm.allSections = [];
        vm.selectedSections = [];
        vm.aliases = [];
        vm.groups = [];
        vm.form = null;
        vm.isNew = false;
        vm.sectionsString = '';

        vm.sortableOptions = {
            distance: 10,
            tolerance: 'move',
            scroll: true,
            zIndex: 6000            
        }

        vm.properties = {
            'Name': { 'label': 'Name', 'description': 'Enter the name for this tour', 'propertyErrorMessage': 'The name is a required field' },
            'Group': { 'label': 'Group', 'description': 'Enter the group name for this tour. This is used to group tours in the help drawer', 'propertyErrorMessage': 'The  group name is a required field' },
            'GroupOrder': { 'label': 'Group order', 'description': 'Control the order of tour groups', 'propertyErrorMessage': 'The  group order is a required field' },
            'Alias': { 'label': 'Alias', 'description': 'Enter the unique alias for this tour', 'propertyErrorMessage': 'Alias is a required field and should be unique' },
            'Sections': { 'label': 'Sections', 'description': 'Sections that the tour will access while running, if the user does not have access to the required tour sections, the tour will not load.   ', 'propertyErrorMessage': 'You should select at least one section' },
            'AllowDisable': { 'label': 'Allow disabling', 'description': 'Adds a "Don\'t" show this tour again"-button to the intro step' }
        };

        var evts = [];

        evts.push(eventsService.on("toureditor.edittour", function (name, arg) {
            vm.tourIndex = arg.index;
            vm.tour = arg.tour;
            vm.isNew = arg.isNew;
            vm.aliases = arg.aliases;
            vm.groups = arg.groups;

            // init the sections array
            if (vm.tour.requiredSections === null) {
                vm.tour.requiredSections = [];
            }

            // get the selected sections from data
            vm.selectedSections = _.filter(vm.allSections,
                function (section) {
                    return _.contains(vm.tour.requiredSections, section.alias);
                });
        }));

        evts.push(eventsService.on("toureditor.discardtourchanges", function (name, arg) {
            vm.tour = null;
            vm.tourIndex = -1;
            vm.selectedSections = [];
            vm.form = null;
            vm.isNew = false;
            vm.aliases = [];
            eventsService.emit('toureditor.tourchangesdiscarded');
        }));

        evts.push(eventsService.on("toureditor.updatetourchanges", function (name, arg) {
            if (formHelper.submitForm({ scope: $scope, formCtrl: vm.form })) {
                eventsService.emit('toureditor.tourchangesupdate',
                    {
                        "index": vm.tourIndex,
                        "tour": vm.tour,
                        "isNew" : vm.isNew
                });
                vm.tour = null;
                vm.tourIndex = -1;
                vm.selectedSections = [];
                vm.form = null;
                vm.isNew = false;
                vm.aliases = [];
            }                      
        }));

        evts.push(eventsService.on("toureditor.stepchangesupdate", function (name, args) {
            vm.tour.steps[args.stepIndex] = args.step;
        }));

        //ensure to unregister from all events!
        $scope.$on('$destroy', function () {
            for (var e in evts) {
                eventsService.unsubscribe(evts[e]);
            }
        });

        function openSectionPicker() {
            vm.sectionPicker = {
                view: 'sectionpicker',
                selection: vm.selectedSections,
                closeButtonLabel: 'Cancel',
                show: true,
                submit: function (model) {

                    // update selection of sections on data
                    vm.tour.requiredSections = _.map(model.selection, function (section) { return section.alias });

                    vm.sectionPicker.show = false;
                    vm.sectionPicker = null;
                },
                close: function (oldModel) {
                    if (oldModel.selection) {
                        vm.selectedSections = oldModel.selection;
                    }
                    vm.sectionPicker.show = false;
                    vm.sectionPicker = null;
                }
            };
        }

        vm.openSectionPicker = openSectionPicker;

        function removeSection(index, selection) {
            if (selection && selection.length > 0) {
                selection.splice(index, 1);
            }

            // update selection of sections on data
            vm.tour.requiredSections = _.map(selection, function (section) { return section.alias });

        }

        vm.removeSection = removeSection;

        function openGroupPicker() {

            var groups = _.map(vm.groups,
                function(x) {
                    return {
                        "name": x,
                        "icon": "icon-tag"
                    };
                });

            vm.groupPicker = {
                view: 'itempicker',                
                availableItems : groups,
                closeButtonLabel: 'Cancel',
                show: true,
                submit: function (model) {                   
                    vm.tour.group = model.selectedItem.name;

                    vm.groupPicker.show = false;
                    vm.groupPicker = null;
                },
                close: function (oldModel) {
                    
                    vm.groupPicker.show = false;
                    vm.groupPicker = null;
                }
            };
        }

        vm.openGroupPicker = openGroupPicker;

        function addStep() {

            if (formHelper.submitForm({ scope: $scope, formCtrl: vm.form })) {
                var newStep = {
                    "title": "",
                    "content": "",
                    "type": null,
                    "element": null,
                    "elementPreventClick": false,
                    "backdropOpacity": 0.4,
                    "event": null,
                    "view": null,
                    "eventElement": null,
                    "customProperties": null
                };

                //vm.tour.steps.push(newStep);

                eventsService.emit('toureditor.editstep',
                    {
                        "stepIndex": vm.tour.steps.length,
                        "tourIndex": vm.tourIndex,
                        "step": newStep,
                        "sections": vm.selectedSections
                    });

            }
        }

        vm.addStep = addStep;

        function editStep(index) {
            if (formHelper.submitForm({ scope: $scope, formCtrl: vm.form })) {

                // deep clone
                var step = JSON.parse(JSON.stringify(vm.tour.steps[index]));


                eventsService.emit('toureditor.editstep',
                    {
                        "stepIndex": index,
                        "tourIndex": vm.tourIndex,
                        "step": step,
                        "sections": vm.selectedSections
            });

            }
        }

        vm.editStep = editStep;

        function removeStep(index) {
            vm.tour.steps.splice(index, 1);
        }

        vm.removeStep = removeStep;        

        function init() {
            sectionResource.getAllSections().then(function (data) {
                vm.allSections = data;
                setSectionIcon(vm.allSections);
            });
        }

        function setSectionIcon(sections) {
            angular.forEach(sections, function (section) {
                section.icon = 'icon-section ' + section.cssclass;
            });
        }

        init();

        $scope.$watch('vm.tour.requiredSections', function () {

            if (vm.tour) {
                if (vm.tour.requiredSections) {
                    vm.sectionsString = vm.tour.requiredSections.join();
                } else {
                    vm.sectionsString = '';
                }
            }

        });

    }

    angular.module("umbraco").controller("Our.Umbraco.TourEditor.TourDetailsController",
        [
            '$scope',
            'eventsService',
            'sectionResource',
            'formHelper',
            TourDetailsController
        ]);

})();