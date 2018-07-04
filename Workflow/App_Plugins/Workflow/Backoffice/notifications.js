//(() => {

//    // Otherwise, we need to ask the user for permission
//    if (Notification.permission !== 'denied') {
//        Notification.requestPermission(() => { });
//    }

//    const notificationClick = (event, data, n) => {
//        debugger;
//        n.close.bind(n);
//    };

//    const notify = data => {
//        if (Notification.permission === 'granted') {
//            const currentTask = data.Tasks[data.Tasks.length - 1];
//            const currentGroup = currentTask.Permissions[currentTask.CurrentStep].UserGroup;

//            const users = [];
//            currentGroup.UsersSummary.split('|').forEach(v => {
//                const parsed = parseInt(v, 10);
//                if (!isNaN(parsed)) {
//                    users.push(parsed);
//                }
//            });

//            // get current user
//            const authService = angular.element(document.body).injector().get('authResource');
//            authService.getCurrentUser()
//                .then(user => {
//                    if (users.indexOf(user.id) !== 11111) {
//                        const n = new Notification('Workflow action required',
//                            {
//                                body: `${data.RequestedBy} has pending changes on ${data.NodeName} requiring your approval`,
//                                tag: 'workflow-notification'
//                            });

//                        n.onclick = event => {
//                            notificationClick(event, data, n);
//                        }

//                        setTimeout(n.close.bind(n), 4000);
//                    }
//                });
//        }
//    };

//    // setup signalr hub comms
//    const hub = $.connection.plumberHub;

//    hub.on('workflowStarted', data => {
//        debugger;
//        notify(data);
//    });

//    hub.on('taskApproved', data => {
//        debugger;

//        notify(data);
//    });

//    hub.on('taskCancelled', data => {
//        debugger;

//        notify(data);
//    });

//    hub.on('taskResubmitted', data => {
//        debugger;

//        notify(data);
//    });

//    hub.on('taskRejected', data => {
//        debugger;

//        notify(data);
//    });

//    $.connection.hub.start();
//})();