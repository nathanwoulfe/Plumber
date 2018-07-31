/*! plumber - v1.0.0-build1 - 2018-07-31
 * Copyright (c) 2018 Nathan Woulfe;
 * Licensed MIT
 */

(() => {

    angular.module('plumber.directives', []);
    angular.module('plumber.filters', []);
    angular.module('plumber.services', []);

    angular.module('plumber', [ 
        'plumber.directives',
        'plumber.filters', 
        'plumber.services'
    ]);

    angular.module('umbraco').requires.push('plumber');

})();
(() => {
    'use strict';

    function dashboardController(workflowResource, tourService) {

        const storeKey = 'plumberUpdatePrompt';
        const msPerDay = 1000 * 60 * 60 * 24;
        const now = moment();

        /**
         * Returns an array of 0s, length equal to the selected range
         */
        const defaultData = () => Array(this.range).fill([]).map(() => 0);

        const lineChart = items => {

            var series = [],
                seriesNames = [],
                s,
                o,
                isTask = this.type === 'Task';

            const d = new Date();

            d.setDate(d.getDate() - this.range);
            const then = Date.UTC(d.getFullYear(), d.getMonth(), d.getDate());

            var created = {
                name: 'Total (cumulative)',
                type: 'spline',
                data: defaultData(),
                pointStart: then,
                pointInterval: msPerDay,
                className: 'wf-highcharts-color-total',
                lineWidth: 4,
                marker: {
                    enabled: false,
                    fillColor: null,
                    lineColor: null
                }
            };

            items.forEach(v => {
                var statusName = isTask ? v.statusName : v.status;

                // bit messy, but need to modify some returned name values
                // type 1|3 status 7 -> rejected
                // type 2 status 7 -> resubmit
                // status 3 -> pending
                // status 1 -> approved
                // status 4 -> not required

                if (v.type !== 2 && v.status === 7) {
                    statusName = v.statusName = 'Rejected';
                } else if (v.type === 2 && v.status === 7) {
                    statusName = v.statusName = 'Resubmitted';
                }

                if (statusName !== 'Pending Approval') {

                    if (seriesNames.indexOf(statusName) === -1) {
                        o = {
                            name: statusName,
                            type: 'column',
                            data: defaultData(),
                            pointStart: then,
                            pointInterval: msPerDay
                        };
                        series.push(o);
                        seriesNames.push(statusName);
                    }

                    s = series.filter(ss => ss.name === statusName)[0];

                    s.data[this.range - now.diff(moment(v.completedDate), 'days')] += 1;
                    created.data[this.range - now.diff(moment(v.createdDate), 'days')] += 1;

                    if (statusName === 'Approved') {
                        this.totalApproved += 1;
                        s.className = 'wf-highcharts-color-approved';
                    } else if (statusName === 'Rejected') {
                        this.totalRejected += 1;
                        s.className = 'wf-highcharts-color-rejected';
                    } else if (statusName === 'Resubmitted') {
                        this.totalResubmitted += 1;
                        s.className = 'wf-highcharts-color-resubmitted';
                    } else if (statusName === 'Not Required') {
                        this.totalNotRequired += 1;
                        s.className = 'wf-highcharts-color-notreq';
                    } else {
                        this.totalCancelled += 1;
                        s.className = 'wf-highcharts-color-cancelled';
                    }

                } else {
                    const index = this.range - now.diff(moment(v.createdDate), 'days');
                    created.data[index < 0 ? 0 : index] += 1;
                    this.totalPending += 1;
                }
            });

            created.data.forEach((v, i) => {
                if (i > 0) {
                    created.data[i] += created.data[i - 1];
                }
            });
            series.push(created);

            this.series = series.sort((a, b) => a.name > b.name);

            this.title = `Workflow ${this.type.toLowerCase()} activity`;
            this.loaded = true;
        };

        const getForRange = () => {
            if (this.range > 0) {

                this.totalApproved = 0;
                this.totalCancelled = 0;
                this.totalPending = 0;
                this.totalRejected = 0;
                this.totalResubmitted = 0;
                this.totalNotRequired = 0;

                this.loaded = false;
                this.totalApproved = this.totalCancelled = this.totalPending = this.totalRejected = 0;

                workflowResource[this.type === 'Task' ? 'getAllTasksForRange' : 'getAllInstancesForRange'](this.range)
                    .then(resp => {
                        lineChart(resp.items);
                    });
            }
        };

        // check the current installed version against the remote on GitHub, only if the 
        // alert has never been dismissed, or was dismissed more than 7 days ago
        const pesterDate = localStorage.getItem(storeKey);

        if (!pesterDate || moment(new Date(pesterDate)).isBefore(now)) {
            workflowResource.getVersion()
                .then(resp => {
                    if (typeof resp === 'object') {
                        this.version = resp;
                    }
                });
        }

        const updateAlertHidden = () => {
            localStorage.setItem(storeKey, now.add(7, 'days'));
        };

        // start selected tour
        const launchTour = tourAlias => {
            tourService.getTourByAlias(tourAlias)
                .then(resp => {
                    tourService.startTour(resp);
                });
        };

        const getActivity = (filter, friendly) => {
            workflowResource.setActivityFilter({
                type: this.type,
                filter: filter,
                range: this.range,
                friendly: friendly
            });
            window.location = Umbraco.Sys.ServerVariables.umbracoSettings.umbracoPath +
                '/#/workflow/workflow/history/info';
        };

        // kick it off with a four-week span
        angular.extend(this, {
            range: 28,
            type: 'Task',
            loaded: false,
            totalApproved: 0,
            totalCancelled: 0,
            totalPending: 0,
            totalRejected: 0,
            totalResubmitted: 0,
            totalNotRequired: 0,

            getForRange: getForRange,
            updateAlertHidden: updateAlertHidden,
            launchTour: launchTour,
            getActivity: getActivity
        });

        getForRange();
    }

    angular.module('plumber').controller('Workflow.AdminDashboard.Controller', ['plmbrWorkflowResource', 'tourService', dashboardController]);
})();
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
(function () {
    'use strict';

    function dashboardController($rootScope, workflowGroupsResource) {

        this.name = 'Approval groups';
        this.loading = true;
        this.items = [];

        this.init = () => {
            workflowGroupsResource.get()
                .then(resp => {
                    this.loading = false;
                    this.items = resp;
                });
        };

        this.getEmail = (users) => users.map(v => v.user.email).join(';');

        $rootScope.$on('refreshGroupsDash', () => {
            this.init();
        });

        this.init();
    }

    angular.module('plumber').controller('Workflow.Groups.Dashboard.Controller', ['$rootScope', 'plmbrGroupsResource', dashboardController]);

}());
(function () {
    'use strict';

    function importexportController(workflowResource, notificationsService) {

        this.doImport = () => {
            workflowResource.doImport(this.importData)
                .then(resp => {
                    if (resp) {
                        notificationsService.success('SUCCESS', 'Plumber config imported successfully');
                    } else {
                        notificationsService.error('ERROR', 'Plumber config import failed');
                    }
                });
        };

        this.doExport = () => {
            workflowResource.doExport()
                .then(resp => {
                    this.exportData = JSON.stringify(resp);
                });
        };

    }

    angular.module('plumber').controller('Workflow.ImportExport.Controller', ['plmbrWorkflowResource', 'notificationsService', importexportController]);
}());
(() => {
    'use strict';

    function logController(workflowResource) {

        const refresh = () => {
            workflowResource.getLog()
                .then(resp => {
                    this.html = resp;
                });

            workflowResource.getLogDates()
                .then(resp => {
                    // resp is an array of log dates, where [0] is 'txt', for the current date as the source file is undated
                    this.datePickerConfig.minDate = resp.length > 1 ? moment(resp[1]) : moment();
                });
        };

        const datePickerChange = event => {
            // handle change for a valid date - fetch corresponding log file if date is ok
            if (event.date && event.date.isValid() && event.oldDate.isValid()) {
                const date = event.date.format('YYYY-MM-DD');
                workflowResource.getLog(date === moment().format('YYYY-MM-DD') ? '' : date)
                    .then(resp => {
                        this.html = resp;
                    });
            }
        };

        const datePickerError = () => {
            // handle error
        };

        angular.extend(this,
            {
                simple: true,
                filter: 'all',
                datePickerConfig: {
                    defaultDate: moment(),
                    maxDate: moment(),
                    pickDate: true,
                    pickTime: false,
                    format: 'D MMM YYYY',
                    icons: {
                        time: 'icon-time',
                        date: 'icon-calendar',
                        up: 'icon-chevron-up',
                        down: 'icon-chevron-down'
                    }
                },

                refresh: refresh,
                datePickerChange: datePickerChange,
                datePickerError: datePickerError
            });

        refresh();

    }

    angular.module('plumber').controller('Workflow.Log.Controller', ['plmbrWorkflowResource', logController]);
})();
(() => {
    'use strict';

    function dashboardController($scope, $rootScope, $routeParams, workflowResource, authResource, notificationsService, plumberHub) {

        let notify = null;

        const getPending = () => {
            // api call for tasks assigned to the current user
            workflowResource.getApprovalsForUser(this.currentUser.id,
                this.taskPagination.perPage,
                this.taskPagination.pageNumber)
                .then(resp => {
                    this.tasks = resp.items;
                    this.taskPagination.pageNumber = resp.page;
                    this.taskPagination.totalPages = resp.total / resp.count;
                    this.loaded[0] = true;
                },
                err => {
                    notify(err);
                });
        };

        const getSubmissions = () => {
            // api call for tasks created by the current user
            workflowResource.getSubmissionsForUser(this.currentUser.id,
                this.submissionPagination.perPage,
                this.submissionPagination.pageNumber)
                .then(resp => { 
                    this.submissions = resp.items;
                    this.submissionPagination.pageNumber = resp.page;
                    this.submissionPagination.totalPages = resp.total / resp.count;
                    this.loaded[1] = true;
                },
                err => {
                    notify(err);
                });
        };

        const getAdmin = () => {
            // if the current user is in an admin group, display all active tasks
            if (this.adminUser) {
                workflowResource.getPendingTasks(this.adminPagination.perPage, this.adminPagination.pageNumber)
                    .then(resp => {
                        this.activeTasks = resp.items;
                        this.adminPagination.pageNumber = resp.page;
                        this.adminPagination.totalPages = resp.totalPages;
                        this.loaded[2] = true;
                    },
                    err => {
                        notify(err);
                    });
            }
        };

        const goToPage = i => {
            this.pagination.pageNumber = i;
        };

        const init = () => {
            getPending();
            getSubmissions();
            getAdmin();
        };

        // dash needs notification of when to refresh, as the action is in a deeper scope
        $rootScope.$on('refreshWorkflowDash',
            () => {
                init();
            });

        // display notification after actioning workflow task
        notify = d => {
            if (d.status === 200) {
                notificationsService.success('SUCCESS!', d.message);
                init();
            } else {
                notificationsService.error('OH SNAP!', d.message);
            }
        };

        const addTask = task => {
            if (task && task.permissions) {
                const permission = task.permissions.filter(p => p.groupId === task.approvalGroupId);

                // these are independent and can all be true
                if (permission.length &&
                    permission[0].userGroup.usersSummary.indexOf(`|${this.currentUser.id}|`) !== -1) {
                    this.tasks.push(task);
                }

                if (task.requestedById === this.currentUser.id) {
                    this.submissions.push(task);
                }

                if (this.adminUser) {
                    this.activeTasks.push(task);
                }
            }
        };
        
        const removeTask = task => {
            const taskId = task.taskId;

            this.tasks.splice(this.tasks.findIndex(i => i.taskId === taskId), 1);

            this.submissions.splice(this.tasks.findIndex(i => i.taskId === taskId), 1);

            if (this.adminUser) {
                this.activeTasks.splice(this.tasks.findIndex(i => i.taskId === taskId), 1);
            }
        };

        // subscribe to signalr magick
        plumberHub.initHub(hub => {

            ['workflowStarted', 'taskCancelled'].forEach(e => {
                addTask(e);
            });

            ['taskApproved', 'taskRejected'].forEach(e => {
                // add the newest task
                addTask(e[0]);
                // remove the previous tasks
                removeTask(e.splice(1));
            });

            hub.start();
        });

        // expose some bits
        angular.extend(this,
            {
                tasks: [],
                submissions: [],
                activeTasks: [],
                loaded: [false, false, false],
                goToPage: goToPage,

                taskPagination: {
                    pageNumber: 1,
                    totalPages: 0,
                    perPage: 5,
                    goToPage: i => {
                        this.taskPagination.pageNumber = i;
                        getPending();
                    }
                },

                submissionPagination: {
                    pageNumber: 1,
                    totalPages: 0,
                    perPage: 5,
                    goToPage: i => {
                        this.submissionPagination.pageNumber = i;
                        getSubmissions();
                    }
                },

                adminPagination: {
                    pageNumber: 1,
                    totalPages: 0,
                    perPage: 10,
                    goToPage: i => {
                        this.adminPagination.pageNumber = i;
                        getAdmin();
                    }
                }
            });

        // kick it all off
        authResource.getCurrentUser()
            .then(user => {
                this.currentUser = user;
                this.adminUser = user.allowedSections.indexOf('workflow') !== -1;
                init();
            });
    }

    // register controller 
    angular.module('plumber').controller('Workflow.UserDashboard.Controller',
        ['$scope', '$rootScope', '$routeParams', 'plmbrWorkflowResource', 'authResource', 'notificationsService', 'plumberHub', dashboardController]);
})();
(() => {
    'use strict';

    function addController($scope, workflowGroupsResource, navigationService, notificationsService, treeService) {

        $scope.$watch('name',
            () => {
                this.failed = false;
            });

        this.add = name => {
            workflowGroupsResource.add(name)
                .then(resp => {
                        if (resp.status === 200) {
                            if (resp.success === true) {
                                treeService.loadNodeChildren({
                                    node: $scope.$parent.currentNode.parent(),
                                    section: 'workflow'
                                })
                                .then(() => {
                                    window.location = `${Umbraco.Sys.ServerVariables.umbracoSettings.umbracoPath}/#/workflow/workflow/edit-group/${resp.id}`;
                                
                                    navigationService.hideNavigation();
                                });

                                notificationsService.success('SUCCESS', resp.msg);
                            } else {
                                this.failed = true;
                                this.msg = resp.msg;
                            }
                        } else {
                            notificationsService.error('ERROR', resp.msg);
                        }

                    },
                    err => {
                        notificationsService.error('ERROR', err);
                    });
        };

        this.cancelAdd = () => {
            navigationService.hideNavigation();
        };
    }

    angular.module('plumber').controller('Workflow.Groups.Add.Controller',
        ['$scope', 'plmbrGroupsResource', 'navigationService', 'notificationsService', 'treeService', addController]);
})();
(() => {
    'use strict';

    function deleteController($scope,
        $rootScope,
        workflowGroupsResource,
        navigationService,
        treeService,
        notificationsService) {

        this.delete = id => {
            workflowGroupsResource.delete(id)
                .then(resp => {
                    treeService.loadNodeChildren({ node: $scope.$parent.currentNode.parent(), section: 'workflow' })
                        .then(() => {
                            navigationService.hideNavigation();
                            notificationsService.success('SUCCESS', resp);
                            $rootScope.$emit('refreshGroupsDash');
                        });
                });
        };

        this.cancelDelete = () => {
            navigationService.hideNavigation();
        };
    }

    angular.module('plumber').controller('Workflow.Groups.Delete.Controller',
        [
            '$scope', '$rootScope', 'plmbrGroupsResource', 'navigationService', 'treeService', 'notificationsService',
            deleteController
        ]);
})();
(() => {

    function editController($scope,
        $routeParams,
        $location,
        workflowGroupsResource,
        workflowResource,
        notificationsService,
        contentResource,
        navigationService) {

        this.view = '';

        const getContentTypes = () => {

            this.nodePermissions = this.group.permissions.filter(v => v.nodeId);
            this.docPermissions = this.group.permissions.filter(v => v.contentTypeId);

            if (this.nodePermissions.length) {
                contentResource.getByIds(this.nodePermissions.map(v => v.nodeId))
                    .then(resp => {
                        resp.forEach(v => {
                            this.nodePermissions.forEach(p => {
                                if (p.nodeId === v.id) {
                                    p.icon = v.icon;
                                    p.path = v.path;
                                    p.name = v.name + ' - stage ' + (p.permission + 1);
                                }
                            });
                        });
                    });
            }

            if (this.docPermissions.length) {
                workflowResource.getContentTypes()
                    .then(resp => {
                        resp.forEach(v => {
                            this.docPermissions.forEach(p => {
                                if (p.contentTypeId === v.id) {
                                    p.icon = v.icon;
                                    p.path = v.path;
                                    p.name = v.name + ' - stage ' + (p.permission + 1);
                                }
                            });
                        });
                    });
            }
        };

        // history tab
        const getHistory = () => {
            workflowResource.getAllTasksForGroup($routeParams.id, this.pagination.perPage, this.pagination.pageNumber)
                .then(resp => {
                    this.tasks = resp.items;
                    this.pagination.pageNumber = resp.page;
                    this.pagination.totalPages = resp.totalPages;

                    this.tasksLoaded = true;
                    this.view = 'group';
                });
        };

        this.editDocTypePermission = () => {
            $location.path('/workflow/workflow/settings/info');
        };

        this.perPage = () => [2, 5, 10, 20, 50];

        // todo -> Would be sweet to open the config dialog from here, rather than just navigating to the node...
        this.editContentPermission = id => {
            navigationService.changeSection('content');
            $location.path(`/content/content/edit/${id}`);
        };

        /**
         * Remove a user from the group
         * @param {any} id
         */
        this.remove = id => {
            var index;
            this.group.users.forEach((u, i) => {
                if (u.userId === id) {
                    index = i;
                }
            });

            this.group.users.splice(index, 1);
        };

        /**
         * Open the picker to add a new user to the group
         */
        this.openUserPicker = () => {
            this.userPicker = {
                view: '../app_plugins/workflow/backoffice/views/dialogs/workflow.userpicker.overlay.html',
                selection: this.group.users,
                show: true,
                submit: model => {
                    this.userPicker.show = false;
                    this.userPicker = null;

                    this.group.users = [];

                    model.selection.forEach(u => {
                        this.group.users.push({ userId: u.userId || u.id, groupId: this.group.groupId, name: u.name });
                    });
                },
                close: () => {
                    this.userPicker.show = false;
                    this.userPicker = null;
                }
            };
        };

        /**
         * Save the group and show appropriate notifications
         */
        this.save = () => {
            workflowGroupsResource.save(this.group)
                .then(resp => {
                        if (resp.status === 200) {
                            notificationsService.success('SUCCESS', resp.msg);
                            $scope.approvalGroupForm.$setPristine();
                        } else {
                            notificationsService.error('ERROR', resp.msg);
                        }
                    },
                    err => {
                        notificationsService.error('ERROR', err);
                    });
        };

        /**
         * Fetch the group by the given id, or create an empty model if the id is -1 (ie a new group - id doesn't exist until saving)
         */
        const init = () => {
            this.loaded = false;

            if ($routeParams.id !== '-1') {
                workflowGroupsResource.get($routeParams.id)
                    .then(resp => {
                        this.group = resp;
                        this.name = $routeParams.id !== '-1' ? 'Edit ' : `Create ${resp.name}`;

                        if (this.group.permissions) {
                            getContentTypes();
                        }

                        this.loaded = true;
                    });
            } else {
                this.group = {
                    groupId: -1,
                    name: '',
                    description: '',
                    alias: '',
                    groupEmail: '',
                    users: [],
                    usersSummary: ''
                };

                this.loaded = true;
            }
        };

        // declare scoped variables
        this.tabs =
        [
            {
                id: 0,
                label: 'Group detail',
                alias: 'tab0',
                active: true
            },
            {
                id: 1,
                label: 'Activity history',
                alias: 'tab1',
                active: false
            }
        ];

        this.pagination = {
            pageNumber: 1,
            totalPages: 0,
            perPage: 10,
            goToPage: (i) => {
                this.pagination.pageNumber = i;
                getHistory();
            }
        };

        // get the data
        init();
        getHistory();
    }

    angular.module('plumber').controller('Workflow.Groups.Edit.Controller',
        [
            '$scope',
            '$routeParams',
            '$location',
            'plmbrGroupsResource',
            'plmbrWorkflowResource',
            'notificationsService',
            'contentResource',
            'navigationService', editController
        ]);
})();


(() => {

    function actionController($scope, workflowResource) {
        this.limit = 250;
        this.disabled = this.isFinalApproval === true ? false : true;

        /**
         * Fetch all tasks for the current workflow instance
         * Then build a UI-ready object
         */
        workflowResource.getAllTasksByGuid($scope.model.guid)
            .then(resp => {
                const tasks = resp.items;

                // current step should only count approved tasks - maybe rejected/resubmitted into
                this.currentStep = resp.currentStep;
                this.totalSteps = resp.totalSteps;

                // there may be multiple tasks for a given step, due to rejection/resubmission
                // modify the tasks object to nest tasks

                this.tasks = [];
                tasks.forEach(v => {

                    if (!this.tasks[v.currentStep]) {
                        this.tasks[v.currentStep] = [];
                    }

                    this.tasks[v.currentStep].push(v);
                });
            });
    }

    angular.module('plumber')
        .controller('Workflow.Action.Controller', ['$scope', 'plmbrWorkflowResource', actionController]);
})();


(() => {
    'use strict';

    // create controller 
    function cancelController($scope) {
        $scope.model.comment = '';
        this.limit = 250;
        this.intro =
            'This operation will cancel the workflow on this document and notify the workflow participants. Are you sure?';
        this.disabled = $scope.model.isFinalApproval === true ? false : true;

        $scope.$watch('model.comment',
            newVal => {
                $scope.model.hideSubmitButton = !newVal || newVal.length === 0;
            });
    }

    // register controller 
    angular.module('plumber').controller('Workflow.Cancel.Controller', ['$scope', cancelController]);
})();

(() => {
    'use strict';

    // create controller 
    function configController($scope, $rootScope, $q, workflowGroupsResource, workflowResource, notificationsService, contentResource, navigationService) {
        const nodeId = $scope.dialogOptions.currentNode ? $scope.dialogOptions.currentNode.id : undefined;
        const nodeIdInt = nodeId ? parseInt(nodeId, 10) : undefined;

        this.inherited = [];
        this.approvalPath = [];
        this.contentTypeApprovalPath = [];

        this.sortOptions = {
            axis: 'y',
            cursor: 'move',
            handle: '.sort-handle',
            stop: () => { }
        };

        let node;
        let settings;

        /**
         * Fetch the groups and content type data
         */
        const init = () => {
            this.contentTypeAlias = node.contentTypeAlias;
            this.contentTypeName = node.contentTypeName;

            const nodePerms = workflowResource.checkNodePermissions(this.groups, nodeIdInt, this.contentTypeAlias);
            this.approvalPath = nodePerms.approvalPath;
            this.contentTypeApprovalPath = nodePerms.contentTypeApprovalPath;

            this.inherited = workflowResource.checkAncestorPermissions(node.path, this.groups);

            if (!this.excludeNode) {
                this.activeType =
                    this.approvalPath.length ? 'content' :
                    this.contentTypeApprovalPath.length ? 'type' :
                    this.inherited.length ? 'inherited' : null;
            }
        };

        if (!nodeId) {
            navigationService.hideDialog();
            notificationsService.error('ERROR', 'No active content node');
        }

        /**
         * Process the approvalPath object, then save it
         */
        this.save = () => {
            var response = {};
            response[nodeIdInt] = [];

            // convert the approvalPath array into something resembling the expected model
            // Dictionary<int, List<UserGroupPermissionsPoco>
            this.approvalPath.forEach((v, i) => {
                response[nodeIdInt].push({
                    nodeId: nodeId,
                    permission: i,
                    groupId: v.groupId
                });
            });

            workflowResource.saveConfig(response)
                .then(() => {
                    navigationService.hideNavigation();
                    notificationsService.success('SUCCESS', 'Workflow configuration updated');
                    $rootScope.$broadcast('configUpdated');
                    init();
                },
                err => {
                    navigationService.hideNavigation();
                    notificationsService.error('ERROR', err);
                });

        };

        /**
         * Adds a stage to the approval flow
         */
        this.add = () => {
            this.selectedApprovalGroup.permissions.push({
                nodeId: nodeId,
                permission: this.approvalPath.length,
                groupId: this.selectedApprovalGroup.groupId
            });

            this.approvalPath.push(this.selectedApprovalGroup);
        };

        /**
         * Removes a stage from the approval flow
         * @param {any} $event
         * @param {any} index
         */
        this.remove = ($event, index) => {
            $event.stopPropagation();
            $event.target.classList.add('disabled');
            this.approvalPath.splice(index, 1);

            this.approvalPath.forEach((v, i) => {
                v.permissions.forEach(p => {
                    if (p.nodeId === nodeIdInt) {
                        p.permission = i;
                    }
                });
            });
        };

        // it all starts here
        const promises = [contentResource.getById(nodeId), workflowResource.getSettings(), workflowGroupsResource.get()];

        $q.all(promises)
            .then(resp => {
                [node, settings, this.groups] = resp;

                this.excludeNode = workflowResource.checkExclusion(settings.excludeNodes, nodeId);
                init();
            });
    }

    // register controller 
    angular.module('plumber').controller('Workflow.Config.Controller',
        ['$scope', '$rootScope', '$q', 'plmbrGroupsResource', 'plmbrWorkflowResource', 'notificationsService', 'contentResource', 'navigationService', configController]);
})();


(() => {
    'use strict';

    // create controller 
    function contentTypeFlowController($scope) {

        if ($scope.model.type) {
            this.approvalPath = $scope.model.type.approvalPath;
        }

        const updateSortOrder = () => {};

        /**
         * 
         */
        this.add = () => {
            if (this.approvalPath) {
                this.approvalPath.push(this.selectedApprovalGroup);
            } else {
                this.approvalPath = [this.selectedApprovalGroup];
            }

            $scope.model.type.approvalPath = this.approvalPath;
        };

        /**
         * 
         * @param {any} $event
         * @param {any} index
         */
        this.remove = ($event, index) => {
            $event.stopPropagation();
            this.approvalPath.splice(index, 1);
            $scope.model.type.approvalPath = this.approvalPath;
        };

        this.sortOptions = {
            axis: 'y',
            cursor: 'move',
            handle: '.sort-handle',
            stop: () => {
                updateSortOrder();
            }
        };
    }

    // register controller 
    angular.module('plumber').controller('Workflow.ContentTypeFlow.Controller', ['$scope', contentTypeFlowController]);
})();


(() => {
    'use strict';

    function controller($scope, $rootScope, $q, $window, userService, workflowResource, workflowGroupsResource, workflowActionsService, contentEditingHelper, editorState, $routeParams, plumberHub) {

        this.active = false;
        this.excludeNode = false;
        this.buttonGroupState = 'init';

        let workflowConfigured = false;
        let dirty = false;

        let user;
        let settings;
        let groups;

        const dashboardClick = editorState.current === null;
        const defaultButtons = contentEditingHelper.configureContentEditorButtons({
            create: $routeParams.create,
            content: $scope.content,
            methods: {
                saveAndPublish: $scope.saveAndPublish,
                sendToPublish: $scope.sendToPublish,
                save: $scope.save,
                unPublish: $scope.unPublish
            }
        });

        let defaultUnpublish;
        if (defaultButtons.subButtons) {
            defaultUnpublish = defaultButtons.subButtons.filter(x => x.alias === 'unpublish')[0];
        }

        const saveAndPublish = defaultButtons.defaultButton && defaultButtons.defaultButton.labelKey === 'buttons_saveAndPublish';

        const buttons = {
            approveButton: {
                labelKey: 'workflow_approveButtonLong',
                handler: item => {
                    this.workflowOverlay = workflowActionsService.action(item, 'Approve', dashboardClick);
                }
            },
            cancelButton: {
                labelKey: 'workflow_cancelButtonLong',
                cssClass: 'danger',
                handler: item => {
                    this.workflowOverlay = workflowActionsService.cancel(item, dashboardClick);
                }
            },
            rejectButton: {
                labelKey: 'workflow_rejectButton',
                cssClass: 'warning',
                handler: item => {
                    this.workflowOverlay = workflowActionsService.action(item, 'Reject', dashboardClick);
                }
            },
            resubmitButton: {
                labelKey: 'workflow_resubmitButton',
                handler: item => {
                    this.workflowOverlay = workflowActionsService.action(item, 'Resubmit', dashboardClick);
                }
            },
            detailButton: {
                labelKey: 'workflow_detailButton',
                handler: item => {
                    this.workflowOverlay = workflowActionsService.detail(item);
                }
            },
            saveButton: {
                labelKey: 'workflow_saveButton',
                cssClass: 'success',
                handler: $scope.save
            },
            publishButton: {
                labelKey: 'workflow_publishButton',
                cssClass: 'success',
                handler: () => {
                    this.workflowOverlay = workflowActionsService.initiate(editorState.current.name, editorState.current.id, true);
                }
            },
            unpublishButton: {
                labelKey: 'workflow_unpublishButton',
                cssClass: 'warning',
                handler: () => {
                    this.workflowOverlay = workflowActionsService.initiate(editorState.current.name, editorState.current.id, false);
                }
            }
        };

        /**
         * any user with access to the workflow section will be able to action workflows ie cancel outside their group membership
         * @param {any} task
         */
        const checkUserAccess = task => {
            this.task = task || this.task;
            this.canAction = false;

            this.adminUser = user.allowedSections.indexOf('workflow') !== -1;
            const currentTaskUsers = this.task.permissions[this.task.currentStep].userGroup.usersSummary;

            if (currentTaskUsers.indexOf(`|${user.id}|`) !== -1) {
                this.canAction = true;
            }

            if (this.active) {

                this.buttonGroup = {};

                if (dirty && (this.userCanEdit || (this.canAction && !settings.lockIfActive))) {
                    this.buttonGroup.defaultButton = buttons.saveButton;
                }
                // primary button is approve when the user is in the approving group and task is not rejected
                else if (this.canAction && !this.rejected) {
                    this.buttonGroup.defaultButton = buttons.approveButton;
                } else if (this.userCanEdit
                ) { // rejected tasks show the resubmit, only when the user is the original author
                    this.buttonGroup.defaultButton = buttons.resubmitButton;
                } else { // all other cases see the detail button
                    this.buttonGroup.defaultButton = buttons.detailButton;
                }

                this.buttonGroup.subButtons = [];

                // if the user is in the approving group, and the task is not rejected, add reject to sub buttons
                if (this.canAction && !this.rejected) {
                    this.buttonGroup.subButtons.push(buttons.rejectButton);
                }
                // if the user is admin, the change author or in the approving group for a non-rejected task, add the cancel button
                if (this.isAdmin || this.userCanEdit || this.isChangeAuthor || (this.canAction && !this.rejected)) {
                    this.buttonGroup.subButtons.push(buttons.cancelButton);
                }
            }
        };

        /**
         * Manages the default states for the buttons - updates when no active task, or when the content form is dirtied
         */
        const setButtons = () => {
            // default button will be null when the current user has browse-only permission
            this.buttonGroup = {};

            if (workflowConfigured && defaultButtons.defaultButton !== null) {
                const subButtons = saveAndPublish ?
                    [buttons.unpublishButton, defaultButtons.defaultButton, buttons.saveButton] :
                    [buttons.unpublishButton, buttons.saveButton];

                // insert the default unpublish button into the subbutton array
                if (saveAndPublish && defaultUnpublish) {
                    subButtons.splice(1, 0, defaultUnpublish);
                }

                // if the content is dirty, show save. otherwise show request approval
                this.buttonGroup = {
                    defaultButton: dirty ? buttons.saveButton : buttons.publishButton,
                    subButtons: dirty ? (saveAndPublish ? [defaultButtons.defaultButton] : []) : subButtons
                };
            } else {
                if (defaultButtons.defaultButton !== null && !this.active) {
                    this.buttonGroup = defaultButtons;
                }
            }

            // if a task is active, the default buttons should be updated to match the current user's access/role in the workflow
            if (this.active) {
                checkUserAccess();
            }
        };

        const getNodeTasks = () => {
            // only refresh if viewing a content node
            if (editorState.current && !editorState.current.trashed) {

                const getPendingTasks = () => {
                    workflowResource.getNodePendingTasks(editorState.current.id)
                        .then(resp => {
                                if (resp.items && resp.items.length) {
                                    this.active = true;

                                    // if the workflow status is rejected, the original author should be able to edit and resubmit
                                    const currentTask = resp.items[resp.items.length - 1];
                                    this.rejected = currentTask.cssStatus === 'rejected';

                                    // if the task has been rejected and the current user requested the change, let them edit
                                    this.isChangeAuthor = currentTask.requestedById === user.id;
                                    this.userCanEdit = this.rejected && this.isChangeAuthor;

                                    checkUserAccess(currentTask);
                                } else {
                                    this.active = false;
                                    setButtons();
                                }
                            },
                            () => {});
                };

                // check if the node is included in the workflow model
                // groups has been fetched already
                const nodePerms = workflowResource.checkNodePermissions(groups,
                    editorState.current.id,
                    editorState.current.contentTypeAlias);
                const ancestorPerms = workflowResource.checkAncestorPermissions(editorState.current.path, groups);

                if ((nodePerms.approvalPath.length ||
                    nodePerms.contentTypeApprovalPath.length ||
                    ancestorPerms.length) && !this.excludeNode) {

                    workflowConfigured = true;
                    getPendingTasks();
                } else {
                    workflowConfigured = false;
                    this.buttonGroup = defaultButtons;
                }
            }
        };

        // use this to ensure changes are saved when submitting for publish
        // event is broadcast from the buttons directive, which watches the content form
        $rootScope.$on('contentFormDirty', (event, data) => {
            dirty = data;
            setButtons();
        });
       
        // subscribe to signalr magick for button state
        // events are raised in ActionController - doesn't matter what they return, only care that they are raised
        // as it indicates a change of state for the button
        const hubEvent = id => {
            if (!dashboardClick && id === editorState.current.id) {
                getNodeTasks();
            }
        };

        plumberHub.initHub(hub => {
            ['workflowStarted', 'taskCancelled', 'taskApproved', 'taskRejected'].forEach(e => {
                hub.on(e, data => {
                    hubEvent(data.nodeId);
                });
            });

            //hub.on('workflowStarted', data => {
            //    hubEvent(data.nodeId);
            //});

            //hub.on('taskCancelled', data => {
            //    hubEvent(data.nodeId); 
            //});

            //hub.on('taskApproved', data => {
            //    hubEvent(data.nodeId);
            //});

            //hub.on('taskRejected', data => {
            //    hubEvent(data.nodeId);
            //});

            hub.start();
        });

        // preview should not save, if the content is in a workflow
        this.preview = content => {
            // Chromes popup blocker will kick in if a window is opened 
            // outwith the initial scoped request. This trick will fix that.
            const previewWindow = $window.open(`preview/?id=${content.id}`, 'umbpreview');
            // Build the correct path so both /#/ and #/ work.
            const redirect = Umbraco.Sys.ServerVariables.umbracoSettings.umbracoPath + '/preview/?id=' + content.id;
            previewWindow.location.href = redirect;
        };

        // it all starts here
        const promises = [userService.getCurrentUser(), workflowResource.getSettings(), workflowGroupsResource.get()];

        $q.all(promises)
            .then(resp => {
                [user, settings, groups] = resp;
                this.excludeNode = workflowResource.checkExclusion(settings.excludeNodes, editorState.current.path);
                getNodeTasks();
            });
    }

    // register controller 
    angular.module('plumber').controller('Workflow.DrawerButtons.Controller',
        [
            '$scope',
            '$rootScope',
            '$q',
            '$window',
            'userService',
            'plmbrWorkflowResource',
            'plmbrGroupsResource',
            'plmbrActionsService',
            'contentEditingHelper',
            'editorState',
            '$routeParams',
            'plumberHub', controller]);
})();
(() => {
    function historyController($scope, workflowResource) {

        this.activityFilter = workflowResource.getActivityFilter();
        this.perPage = () => [2, 5, 10, 20, 50];
        this.name = 'Workflow history';
        this.view = '';

        this.pagination = {
            pageNumber: 1,
            totalPages: 0,
            perPage: 10,
            goToPage: i => {
                this.pagination.pageNumber = i;
                if (this.activityFilter) {
                    this.getActivity();
                }
                else if (this.node !== undefined) {
                    this.auditNode();
                } else {
                    this.getAllInstances();
                }
            }
        };

        const width = $scope.dialogOptions ? $scope.dialogOptions.currentAction.metaData.width : undefined;
        const node = $scope.dialogOptions ? $scope.dialogOptions.currentNode : undefined;

        const setPaging = resp => {
            this.items = resp.items;
            this.pagination.pageNumber = resp.page;
            this.pagination.totalPages = resp.totalPages;
            this.loading = false;
        };

        if (width) {
            angular.element('#dialog').css('width', width);
        }

        this.selectNode = () => {
            this.overlay = {
                view: 'contentpicker',
                show: true,
                submit: model => {
                    if (model.selection) {
                        this.auditNode(model.selection[0]);
                    } else {
                        $scope.items = [];
                    }
                    this.overlay.close();
                },
                close: () => {
                    this.overlay.show = false;
                    this.overlay = null;
                }
            };
        };

        /**
         * 
         */
        this.getAllInstances = () => {
            this.loading = true;

            // when switching, set state, reset paging and clear node data
            if (this.view !== 'instance') {
                this.view = 'instance';
                this.pagination.pageNumber = 1;
                this.node = undefined;
            }

            workflowResource.getAllInstances(this.pagination.perPage, this.pagination.pageNumber)
                .then(resp => {
                    setPaging(resp);
                    this.instancesLoaded = true;
                });
        };

        /**
         * 
         * @param {any} data
         */
        this.auditNode = data => {
            this.loading = true;

            // when switching from instance to node, reset paging, toggle state and store node
            if (this.view !== 'node') {
                this.pagination.pageNumber = 1;
                this.view = 'node';
            }

            this.node = data || this.node;

            workflowResource.getAllInstancesForNode(this.node.id, this.pagination.perPage, this.pagination.pageNumber)
                .then(resp => {
                    setPaging(resp);
                    this.nodeInstancesLoaded = true;
                });
        };

        /**
         * 
         */
        const getActivity = () => {
            if (this.view.indexOf('activity') === -1) {
                this.pagination.pageNumber = 1;
                this.node = undefined;
                this.view = `activity-${this.activityFilter.type.toLowerCase()}`;
            }

            workflowResource[this.activityFilter.type === 'Task' ? 'getFilteredTasksForRange' : 'getFilteredInstancesForRange'](
                    this.activityFilter.range,
                    this.activityFilter.filter,
                    this.pagination.perPage,
                    this.pagination.pageNumber)
                .then(resp => {
                    setPaging(resp);
                    this.activityLoaded = true;
                });
        };

        // go get the data
        if (this.activityFilter) {
            getActivity();
        }
        else if (node) {
            this.auditNode(node);
        } else {
            this.getAllInstances();
        }
    }

    angular.module('plumber').controller('Workflow.History.Controller', ['$scope', 'plmbrWorkflowResource', historyController]);

})();

(() => {
    'use strict';

    function settingsController($scope, $q, workflowResource, notificationsService, workflowGroupsResource, contentResource) {

        const promises = [workflowResource.getSettings(), workflowResource.getContentTypes(), workflowGroupsResource.get()];
        const overlayBase = '../app_plugins/workflow/backoffice/views/dialogs/';

        this.excludeNodesModel = {
            view: 'contentpicker',
            editor: 'Umbraco.MultiNodeTreePicker2',
            alias: 'excludeNodesPicker',
            config: {
                multiPicker: '1',
                maxNumber: null,
                minNumber: null,
                idType: 'id',
                showEditButton: '0',
                showOpenButton: '0',
                showPathOnHover: '0',
                startNode: {
                    type: 'content'
                }
            }
        };

        this.name = 'Workflow settings';
        this.email = '';
        this.defaultApprover = '';
        this.settings = {
            email: '',
            defaultApprover: ''
        };

        $q.all(promises)
            .then(resp => {

                [this.settings, this.docTypes, this.groups] = resp;

                if (this.settings.excludeNodes) {
                    this.excludeNodesModel.value = this.settings.excludeNodes;

                    // this feels super hacky - fetch nodes and push into the content picker
                    // there's a watch in the picker controller, but it's not seeing changes to the value
                    const picker = document.querySelector('#exclude-nodes-picker ng-form');
                    if (picker) {
                        var s = angular.element(picker).scope();

                        this.settings.excludeNodes.split(',').forEach(id => {
                            contentResource.getById(id).then(entity => {
                                s.add(entity);
                            });
                        });
                    }
                }

                this.flowTypes = [
                    { i: 0, v: 'Explicit' },
                    { i: 1, v: 'Implicit' },
                ];

                this.flowType = this.flowTypes[this.settings.flowType];

                if (this.settings.defaultApprover) {
                    this.defaultApprover = this.groups.filter(g => parseInt(g.groupId, 10) === parseInt(this.settings.defaultApprover, 10))[0];
                }

                this.groups.forEach(g => {
                    g.permissions.forEach(p => {
                        if (p.contentTypeId > 0) {
                            this.docTypes.forEach(dt => {
                                if (dt.id === p.contentTypeId) {
                                    if (!dt.approvalPath) {
                                        dt.approvalPath = [];
                                    }

                                    dt.approvalPath[p.permission] = g;
                                }
                            });
                        }
                    });
                });
            });


        this.save = () => {
            const permissions = {};

            this.settings.defaultApprover = this.defaultApprover ? this.defaultApprover.groupId : '';
            this.settings.flowType = this.flowType.i;

            if (this.excludeNodesModel.value) {
                this.settings.excludeNodes = this.excludeNodesModel.value;
            }

            // convert the approval path group collection into a set of permissions objects for saving
            // means we're holding extra data, but makes it easier to manipulate as it's less abstract
            this.docTypes.forEach((dt, i) => {
                if (dt.approvalPath && dt.approvalPath.length) {
                    permissions[i] = [];
                    dt.approvalPath.forEach((path, ii) => {
                        permissions[i].push({
                            contentTypeId: dt.id,
                            permission: ii,
                            groupId: path.groupId
                        });
                    });
                }
            });

            var p = [workflowResource.saveDocTypeConfig(permissions), workflowResource.saveSettings(this.settings)];

            $q.all(p)
                .then(
                () => { notificationsService.success('SUCCESS!', 'Settings updated'); },
                err => { notificationsService.error('OH SNAP!', err); });
        };


        /**
         * Removes the approval path for the group, which will remove it from config on save
         * @param {any} type
         */
        this.removeDocTypeFlow = type => {
            delete type.approvalPath;
        };

        this.editDocTypeFlow = type => {
            this.overlay = {
                view: `${overlayBase}workflow.contenttypeflow.overlay.html`,
                show: true,
                type: type,
                groups: this.groups,
                types: this.docTypes.filter(v => !v.approvalPath),
                title: `${type ? 'Edit' : 'Add'} content type approval flow`,
                submit: model => {

                    // map the updated approval path back onto the doctypes collection 
                    if (model.type.approvalPath.length) {
                        this.docTypes.forEach(v => {
                            if (v.id === model.type.id) {
                                v.approvalPath = model.type.approvalPath;
                            }
                        });
                    }

                    this.overlay.close();
                },
                close: () => {
                    this.overlay.show = false;
                    this.overlay = null;
                }
            };
        };

        this.hasApprovalPath = d => d.approvalPath !== undefined;
    }

    angular.module('plumber').controller('Workflow.Settings.Controller',
        ['$scope', '$q', 'plmbrWorkflowResource', 'notificationsService', 'plmbrGroupsResource', 'contentResource', settingsController]);

})();
(() => {
    'use strict';

    const submitController = $scope => {
        $scope.$watch('model.comment',
            newVal => {
                $scope.model.hideSubmitButton = !newVal || newVal.length === 0;
            });
    };

    angular.module('plumber').controller('Workflow.Submit.Controller', ['$scope', submitController]);
})();
// this is almost identical to the Umbraco default, only the id property on the user object is changed to userId
(() => {
    'use strict';

    function userPickerController($scope, usersResource, localizationService) {

        this.users = [];
        this.loading = false;
        this.usersOptions = {};

        //////////

        const preSelect = (selection, users) => {
            selection.forEach(selected => {
                users.forEach(user => {
                    if (selected.userId === user.id) {
                        user.selected = true;
                    }
                });
            });
        };

        const getUsers = () => {

            this.loading = true;

            // Get users
            usersResource.getPagedResults(this.usersOptions).then(users => {

                this.users = users.items;

                this.usersOptions.pageNumber = users.pageNumber;
                this.usersOptions.pageSize = users.pageSize;
                this.usersOptions.totalItems = users.totalItems;
                this.usersOptions.totalPages = users.totalPages;

                preSelect($scope.model.selection, this.users);

                this.loading = false;

            });
        };

        const search = _.debounce(() => {
            $scope.$apply(() => {
                getUsers();
            });
        }, 500);

        const onInit = () => {

            this.loading = true;

            // set default title
            if (!$scope.model.title) {
                $scope.model.title = localizationService.localize('defaultdialogs_selectUsers');
            }

            // make sure we can push to something
            if (!$scope.model.selection) {
                $scope.model.selection = [];
            }

            // get users
            getUsers();

        };


        this.searchUsers = () => {
            search();
        };

        this.changePageNumber = pageNumber => {
            this.usersOptions.pageNumber = pageNumber;
            getUsers();
        };

        this.selectUser = user => {

            if (!user.selected) {

                user.selected = true;
                $scope.model.selection.push(user);

            } else {

                $scope.model.selection.forEach((selectedUser, index) => {
                    if (selectedUser.userId === user.id) {
                        user.selected = false;
                        $scope.model.selection.splice(index, 1);
                    }
                });

            }

        };

        onInit();

    }

    angular.module('plumber').controller('Workflow.UserPicker.Controller', ['$scope', 'usersResource', 'localizationService', userPickerController]);

})();
(() => {
    'use strict';

    function buttonGroupDirective($rootScope, angularHelper, editorState, workflowActionsService) {

        const directive = {
            restrict: 'E',
            replace: true,
            templateUrl: '../app_plugins/workflow/backoffice/views/partials/workflowButtonGroup.html',
            require: '^form',
            scope: {
                defaultButton: '=',
                subButtons: '=',
                state: '=?',
                item: '=',
                direction: '@?',
                float: '@?',
                drawer: '@?'
            },
            link: (scope, elm, attr, contentForm) => {

                scope.detail = item => {
                    scope.workflowOverlay = workflowActionsService.detail(item);
                };

                scope.state = 'init';

                // can watch the content form state in the directive, then broadcast the state change
                scope.$watch(
                    () => contentForm.$dirty,
                    newVal => {
                        $rootScope.$broadcast('contentFormDirty', newVal);
                    });

                $rootScope.$on('buttonStateChanged', (event, data) => {
                    if (scope.item && scope.item.nodeId === data.id || editorState.current && editorState.current.id === data.id) {
                        scope.state = data.state;

                        // button might be in a dashboard, so need to check for content form before resetting form state
                        if (editorState.current && contentForm) {
                            contentForm.$setPristine();
                        }
                    }
                });
            }
        };

        return directive;
    }

    angular.module('plumber.directives').directive('workflowButtonGroup', ['$rootScope', 'angularHelper', 'editorState', 'plmbrActionsService', buttonGroupDirective]);

})();

(() => {

    const template = `
                <div>
                    <p ng-bind="intro"></p>
                    <label for="comments">
                        {{ labelText }} <span ng-bind="info"></span>
                    </label>
                    <textarea no-dirty-check id="comments" ng-model="comment" ng-change="limitChars()"></textarea>
                </div>`;

    function comments() {

        const directive = {
            restrict: 'AEC',
            scope: {
                intro: '=',
                labelText: '=',
                comment: '=',
                limit: '=',
                isFinalApproval: '=',
                disabled: '='
            },
            template: template,
            link: scope => {

                scope.limitChars = () => {

                    var limit = scope.limit;

                    if (scope.comment.length > limit) {
                        scope.info = `(Comment max length exceeded - limit is ${limit} characters.)`;
                        scope.comment = scope.comment.substr(0, limit);
                    } else {
                        scope.info = `(${limit - scope.comment.length} characters remaining.)`;
                    }

                    if (!scope.isFinalApproval) {
                        scope.disabled = scope.comment.length === 0;
                    }
                };
            }
        };

        return directive;
    }

    angular.module('plumber.directives').directive('wfComments', comments);

})();
(() => {

    function instances(workflowActionsService) {

        const directive = {
            restrict: 'E',
            scope: {
                items: '=',
                loaded: '=',
                view: '='
            },
            templateUrl: '../app_plugins/workflow/backoffice/views/partials/workflowInstanceTemplate.html',
            link: scope => {
                scope.detail = item => {
                    scope.instanceOverlay = workflowActionsService.detail(item);
                };

                scope.$watch('view',
                    () => {
                        scope.showProgress = scope.view.indexOf('activity') === -1 && scope.view !== 'group';
                        scope.showName = scope.view === 'instance' || scope.view.indexOf('activity') === 0 || scope.view === 'group';
                    });
            }

        };

        return directive;
    }

    angular.module('plumber.directives').directive('wfInstance', ['plmbrActionsService', instances]);

})();

(() => {
    'use strict';

    function lineChart() {

        const directive = {
            restrict: 'E',
            template: '<div class="chart-container"><div></div></div>',
            scope: {
                series: '=',
                ready: '='
            },
            link: (scope, element) => {
                const el = element[0].querySelector('.chart-container div');

                scope.$watch('ready', newVal => {
                    if (newVal === true) {
                         const options = {
                            credits: {
                                enabled: false
                            },
                            title: {
                                text: null
                            },
                            legend: {
                                itemStyle: {
                                    fontSize: '15px'
                                }
                            },
                            tooltip: {
                                shared: true,
                                formatter: function() {
                                    const r = this.points.filter(p => p.y > 0).length > 0;

                                    if (!r) {
                                        return false;
                                    }

                                    var s = `<span>${new Date(this.x).toDateString()}</span><br />`;

                                    this.points.forEach(p => {
                                        if (p.y > 0) {
                                            s += `<span class="wf-highcharts-color-${p.series.name.toLowerCase().replace(' ', '-')}">\u25CF</span> ${p.series.name}: <b>${p.y}</b><br/>`;
                                        }
                                    });

                                    return s;
                                }
                            },
                            series: scope.series,
                            xAxis: {
                                type: 'datetime',
                                dateTimeLabelFormats: {
                                    day: '%b %e'
                                }
                            },
                            yAxis: {
                                allowDecimals: false,
                                minTickInterval: 1,
                                min: 0,
                                type: 'logarithmic',
                                title: {
                                    text: null
                                }
                            }
                        };

                        Highcharts.chart(el, options);
                    }
                });
            }
        };

        return directive;
    }

    angular.module('plumber.directives').directive('wfLineChart', lineChart);

})();

(() => {

    const template = `
        <div class="progress-step {{ css[0] }}" ng-style="{ 'width' : width }">
            <span class="marker"></span>
            <span class="tooltip">
                <span class="tooltip-{{ css[0] }}" ng-bind="css[1]"></span>
                {{ task.approvalGroup }}
            </span>
        </div>`;

    function progressStep() {

        const directive = {
            restrict: 'E',
            replace: true,
            scope: {
                task: '=',
                count: '='
            },
            template: template,
            link: scope => {
                scope.width = `${100 / scope.count}%`;

                scope.css = scope.task.cssStatus === 'approved' ? ['done', 'Done'] :
                    scope.task.cssStatus === 'pending' ? ['current', 'Pending'] :
                        scope.task.cssStatus === 'not' ? ['notrequired', 'Not required'] :
                            [scope.task.cssStatus.toLowerCase(), scope.task.cssStatus];
            }
        };

        return directive;
    }

    angular.module('plumber.directives').directive('wfProgressStep', progressStep);

})();

(() => {
    function tasks($location, workflowActionsService) {

        const directive = {
            restrict: 'AEC',
            scope: {
                items: '=',
                type: '=',
                loaded: '='
            },
            templateUrl: '../app_plugins/workflow/backoffice/views/partials/workflowTasksTemplate.html',
            controller: function($scope) {

                // type = 0, 1
                // 0 -> full button set
                // 1 -> cancel, edit - this is reversed if the task is rejected
                // 2 -> no buttons

                $scope.detail = item => {
                    $scope.$parent.vm.workflowOverlay = workflowActionsService.detail(item);
                };

                const buttons = {
                    approveButton: {
                        labelKey: 'workflow_approveButton',
                        handler: item => {
                            $scope.$parent.vm.workflowOverlay = workflowActionsService.action(item, 'Approve', true);
                        }
                    },
                    editButton: {
                        labelKey: 'workflow_editButton',
                        handler: item => {
                            $location.path(`/content/content/edit/${item.nodeId}`);
                        }
                    },
                    cancelButton: {
                        labelKey: 'workflow_cancelButton',
                        cssClass: 'danger',
                        handler: item => {
                            $scope.$parent.vm.workflowOverlay = workflowActionsService.cancel(item, true);
                        }
                    },
                    rejectButton: {
                        labelKey: 'workflow_rejectButton',
                        cssClass: 'warning',
                        handler: item => {
                            $scope.$parent.vm.workflowOverlay = workflowActionsService.action(item, 'Reject', true);
                        }
                    }
                };

                const subButtons = [
                    [buttons.editButton, buttons.rejectButton, buttons.cancelButton],
                    [buttons.editButton],
                    [buttons.cancelButton]
                ];

                if ($scope.type !== 2) {
                    $scope.buttonGroup = {
                        defaultButton: $scope.type === 0 ? buttons.approveButton : buttons.cancelButton,
                        subButtons: subButtons[$scope.type]
                    };
                } else {
                    $scope.noActions = true;
                }

                // when the items arrive, if a task was rejected
                // flip the order of the cancel and edit buttons
                $scope.$watch('items',
                    newVal => {
                        if (newVal && newVal.length && $scope.type === 0) {
                            $scope.items.forEach(i => {
                                if (i.cssStatus === 'rejected') {
                                    $scope.buttonGroup.defaultButton = buttons.editButton;
                                    $scope.buttonGroup.subButtons = [buttons.cancelButton];
                                }
                            });
                        }
                    });
            }
        };

        return directive;
    }

    angular.module('plumber.directives').directive('wfTasks', ['$location', 'plmbrActionsService', tasks]);

})();

/* register all interceptors 
 * 
 */
(() => {
    'use strict';

    angular.module('plumber')
        .config(function($httpProvider) {
            $httpProvider.interceptors.push('drawerButtonsInterceptor');
        });
})();
(() => {
    // replace the editor buttons with Plumber's version
    function interceptor($q) {
        return {
            request: req => {
                if (req.url.toLowerCase().indexOf('footer-content-right') !== -1) {
                    if (location.href.indexOf('content') !== -1) {
                        req.url = '../app_plugins/workflow/backoffice/views/partials/workflowEditorFooterContentRight.html';
                    }
                }
                return req || $q.when(req);
            }
        };
    }

    angular.module('plumber').factory('drawerButtonsInterceptor', ['$q', interceptor]);
})();
(() => {
    'use strict';

    function workflowActionsService($rootScope, workflowResource, notificationsService) {

        const dialogPath = '../app_plugins/workflow/backoffice/views/dialogs/'; 

        // UI feedback for button directive
        const buttonState = (state, id) => {
            $rootScope.$emit('buttonStateChanged', { state: state, id: id });
        };

        // display notification after actioning workflow task
        const notify = (d, fromDash, id) => {
            if (d.status === 200) {

                notificationsService.success('SUCCESS', d.message);

                if (fromDash) {
                    $rootScope.$emit('refreshWorkflowDash');
                }
                $rootScope.$emit('workflowActioned');
                buttonState('success', id);
            } else {
                notificationsService.error('OH SNAP', d.message);
                buttonState('error', id);
            }
        };

        const service = {

            action: (item, type, fromDash) => {
                let workflowOverlay = {
                    view: dialogPath + 'workflow.action.dialog.html',
                    show: true,
                    title: type + ' workflow process',
                    subtitle: `Document: ${item.nodeName}`,
                    comment: item.comment,
                    approvalComment: '',
                    guid: item.instanceGuid,
                    requestedBy: item.requestedBy,
                    requestedOn: item.requestedOn,
                    submit: model => {

                        buttonState('busy', item.nodeId);

                        // build the function name and access it via index rather than property - saves duplication
                        const functionName = type.toLowerCase() + 'WorkflowTask';
                        workflowResource[functionName](item.instanceGuid, model.approvalComment)
                            .then(resp => {
                                notify(resp, fromDash, item.nodeId);
                            });
                       
                        workflowOverlay.close();
                    },
                    close: () => {
                        workflowOverlay.show = false;
                        workflowOverlay = null;
                    }
                };

                return workflowOverlay;
            },

            initiate: (name, id, publish) => {
                let workflowOverlay = {
                    view: dialogPath + 'workflow.submit.dialog.html',
                    show: true,
                    title: `Send for ${publish ? 'publish' : 'unpublish'} approval`,
                    subtitle: `Document: ${name}`,
                    isPublish: publish,
                    nodeId: id,
                    submit: model => {

                        buttonState('busy', id);

                        workflowResource.initiateWorkflow(id, model.comment, publish)
                            .then(resp => {
                                notify(resp, false, id);
                            });

                        workflowOverlay.close();
                    },
                    close: () => {
                        workflowOverlay.show = false;
                        workflowOverlay = null;
                    }
                };
                return workflowOverlay;
            },

            cancel: (item, fromDash) => {
                let workflowOverlay = {
                    view: dialogPath + 'workflow.cancel.dialog.html',
                    show: true,
                    title: 'Cancel workflow process',
                    subtitle: `Document: ${item.nodeName}`,
                    comment: '',
                    isFinalApproval: item.activeTask === 'Pending Final Approval',
                    submit: model => {

                        buttonState('busy', item.nodeId);

                        workflowResource.cancelWorkflowTask(item.instanceGuid, model.comment)
                            .then(resp => {
                                notify(resp, fromDash, item.nodeId);
                            });

                        workflowOverlay.close();
                    },
                    close: () => {
                        workflowOverlay.show = false;
                        workflowOverlay = null;
                    }
                };

                return workflowOverlay;
            },

            detail: item => {
                let workflowOverlay = {
                    view: dialogPath + 'workflow.action.dialog.html',
                    show: true,
                    title: 'Workflow detail',
                    subtitle: `Document: ${item.nodeName}`,
                    comment: item.comment,
                    guid: item.instanceGuid,
                    requestedBy: item.requestedBy,
                    requestedOn: item.requestedOn,
                    detail: true,
                    
                    close: () => {
                        workflowOverlay.show = false;
                        workflowOverlay = null;
                    }
                };

                return workflowOverlay;
            },

            buttonState: (state, id) => {
                buttonState(state, id);
            }
        };

        return service;
    }

    angular.module('plumber.services').factory('plmbrActionsService',
        ['$rootScope', 'plmbrWorkflowResource', 'notificationsService', workflowActionsService]);

})();
(() => {
    'use strict';

    function workflowGroupsResource($http, $q, umbRequestHelper) {

        const urlBase = Umbraco.Sys.ServerVariables.umbracoSettings.umbracoPath + '/backoffice/api/workflow/groups/';

        const request = (method, url, data) =>
            umbRequestHelper.resourcePromise(
                method === 'DELETE' ? $http.delete(url)
                    : method === 'POST' ? $http.post(url, data)
                        : method === 'PUT' ? $http.put(url, data)
                            : $http.get(url),
                'Something broke'
            );

        const service = {

            /**
             * @returns {array} user groups
             * @description Get single group by id, or all groups if no id parameter provided
             */
            get: id => request('GET', urlBase + (id ? `get/${id}` : 'get')),

            /**
             * @returns the new user group
             * @description Add a new group, where the param is the new group name
             */
            add: name => request('POST', urlBase + 'add', { data: name }),

            /**
             * @returns {string}
             * @description save updates to an existing group object
             */
            save: group => request('PUT', urlBase + 'save', group),

            /**
             * @returns {string}
             * @description delete group by id
             */
            'delete': id => request('DELETE', urlBase + 'delete/' + id)
        };

        return service;
    }

    angular.module('plumber.services').factory('plmbrGroupsResource', ['$http', '$q', 'umbRequestHelper', workflowGroupsResource]);

})();
(() => {

    function plumberHub($rootScope, $q, assetsService) {

        const scripts = [
            '../App_Plugins/workflow/backoffice/lib/signalr/jquery.signalr-2.2.1.min.js',
            '/umbraco/backoffice/signalr/hubs'
        ];

        function initHub(callback) {
            if ($.connection == undefined) {

                const promises = [];
                scripts.forEach(script => {
                    promises.push(assetsService.loadJs(script));
                });

                $q.all(promises)
                    .then(() => {
                        hubSetup(callback);
                    });
            } else {
                hubSetup(callback);
            }
        }

        function hubSetup(callback) {

            const proxy = $.connection.plumberHub;

            const hub = {
                start: () => {
                    $.connection.hub.start();
                },
                on: (eventName, callback) => {
                    proxy.on(eventName,
                        result => {
                            $rootScope.$apply(() => {
                                if (callback) {
                                    callback(result);
                                }
                            });
                        });
                },
                invoke: (methodName, callback) => {
                    proxy.invoke(methodName)
                        .done(result => {
                            $rootScope.$apply(() => {
                                if (callback) {
                                    callback(result);
                                }
                            });
                        });
                }
            };

            return callback(hub);
        }

        return {
            initHub: initHub
        };
    }

    angular.module('plumber.services').factory('plumberHub', ['$rootScope', '$q', 'assetsService', plumberHub]);

})();
(() => {
    'use strict';

    // create service
    function workflowResource($http, $q, umbRequestHelper) {

        let activityFilter;

        const urlBase = Umbraco.Sys.ServerVariables.umbracoSettings.umbracoPath + '/backoffice/api/workflow/';

        // are there common elements between two arrays?
        const common = (arr1, arr2) => arr1.some(el => arr2.indexOf(el) > -1);

        const request = (method, url, data) =>
            umbRequestHelper.resourcePromise(
                method === 'GET' ? $http.get(url) : $http.post(url, data),
                'Something broke');

        const urls = {
            settings: urlBase + 'settings/',
            tasks: urlBase + 'tasks/',
            instances: urlBase + 'instances/',
            actions: urlBase + 'actions/',
            logs: urlBase + 'logs/',
        };

        const service = {

            getContentTypes: () => request('GET', urls.settings + 'getcontenttypes'),

            /* tasks and approval endpoints */
            getApprovalsForUser: (userId, count, page) => request('GET', urls.tasks + 'flows/' + userId + '/0/' + count + '/' + page),

            getSubmissionsForUser: (userId, count, page) => request('GET', urls.tasks + 'flows/' + userId + '/1/' + count + '/' + page),

            getPendingTasks: (count, page) => request('GET', urls.tasks + 'pending/' + count + '/' + page),

            getAllTasksForRange: days => request('GET', urls.tasks + 'range/' + days),

            getFilteredTasksForRange: (days, filter, count, page) => request('GET', urls.tasks + 'filteredRange/' +
                days +
                (filter ? `/${filter}` : '') +
                (count ? `/${count}` : '') +
                (page ? `/${page}` : '')),

            getAllInstances: (count, page, filter) => request('GET', urls.instances + count + '/' + page + '/' + (filter || '')),

            getAllInstancesForRange: days => request('GET', urls.instances + 'range/' + days),

            getAllInstancesForNode: (nodeId, count, page) => request('GET', urls.instances + nodeId + '/' + count + '/' + page),

            getFilteredInstancesForRange: (days, filter, count, page) => request('GET', urls.instances + 'filteredRange/' +
                days +
                (filter ? `/${filter}` : '') +
                (count ? `/${count}` : '') +
                (page ? `/${page}` : '')),

            getAllTasksForGroup: (groupId, count, page) => request('GET', urls.tasks + 'group/' + groupId + '/' + count + '/' + page),

            getAllTasksByGuid: guid => request('GET', urls.tasks + 'tasksbyguid/' + guid),

            getNodeTasks: (id, count, page) => request('GET', urls.tasks + 'node/' + id + '/' + count + '/' + page),

            getNodePendingTasks: id => request('GET', urls.tasks + 'node/pending/' + id),


            /* workflow actions */
            initiateWorkflow: (nodeId, comment, publish) =>
                request('POST',
                    urls.actions + 'initiate', { nodeId: nodeId, comment: comment, publish: publish }),

            approveWorkflowTask: (instanceGuid, comment) =>
                request('POST',
                    urls.actions + 'approve', { instanceGuid: instanceGuid, comment: comment }),

            rejectWorkflowTask: (instanceGuid, comment) =>
                request('POST',
                    urls.actions + 'reject', { instanceGuid: instanceGuid, comment: comment }),

            resubmitWorkflowTask: (instanceGuid, comment) =>
                request('POST',
                    urls.actions + 'resubmit', { instanceGuid: instanceGuid, comment: comment }),

            cancelWorkflowTask: (instanceGuid, comment) =>
                request('POST',
                    urls.actions + 'cancel', { instanceGuid: instanceGuid, comment: comment }),


            /* get/set workflow settings*/
            getSettings: () => request('GET', urls.settings + 'get'),

            saveSettings: settings => request('POST', urls.settings + 'save', settings),

            getVersion: () => request('GET', urls.settings + 'version'),

            getDocs: () => request('GET', urls.settings + 'docs'),

            getLog: date => request('GET', urls.logs + 'get/' + (date || '')),

            getLogDates: () => request('GET', urls.logs + 'datelist'),


            doImport: model => request('POST', urlBase + 'import', model),

            doExport: () => request('GET', urlBase + 'export'),

            /*** SAVE PERMISSIONS ***/
            saveConfig: p => request('POST', urlBase + 'config/saveconfig', p),

            saveDocTypeConfig: p => request('POST', urlBase + 'config/savedoctypeconfig', p),

            checkExclusion: (excludedNodes, path) => {
                if (!excludedNodes) {
                    return false;
                }

                const excluded = excludedNodes.split(',');
                // if any elements are shared, exclude the node from the workflow mechanism
                // by checking the path not just the id, this becomes recursive, and the excludeNodes cascades down the tree
                return common(path.split(','), excluded);
            },

            checkNodePermissions: (groups, id, contentTypeAlias) => {
                const resp = {
                    approvalPath: [],
                    contentTypeApprovalPath: []
                };

                groups.forEach(v => {
                    v.permissions.forEach(p => {
                        if (p.nodeId === id) {
                            resp.approvalPath[p.permission] = v;
                        }

                        if (p.contentTypeAlias === contentTypeAlias) {
                            resp.contentTypeApprovalPath[p.permission] = v;
                        }
                    });
                });
                return resp;
            },

            checkAncestorPermissions: (path, groups) => {
                // first is -1, last is the current node
                path = path.split(',');
                path.shift();
                path.pop();

                const resp = [];

                path.forEach(id => {
                    groups.forEach(group => {
                        group.permissions.forEach(p => {
                            if (p.nodeId === parseInt(id, 10)) {
                                resp[p.permission] = {
                                    name: group.name,
                                    groupId: p.groupId,
                                    nodeName: p.nodeName,
                                    permission: p.permission
                                };
                            }
                        });
                    });
                });

                return resp;
            },

            // pass the activity filter between the admin and history views
            setActivityFilter: filter => {
                activityFilter = filter;
            },

            getActivityFilter: () => activityFilter,

        };

        return service;
    }

    // register service
    angular.module('plumber.services').factory('plmbrWorkflowResource', ['$http', '$q', 'umbRequestHelper', workflowResource]);

})();