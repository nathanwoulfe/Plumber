(() => {

    /**
     * Set the icon for the given task, based on the stauts
     * @param { } task 
     * @returns { string } 
     */
    function iconName() {
        return function (task) {
            let response = '';
            //rejected
            if ((task.typeId === 1 || task.typeId === 3) && task.status === 7 || task.status === 2) {
                response = 'delete';
            }
            // resubmitted or approved
            if (task.typeId === 2 && task.status === 7 || task.status === 1) {
                response = 'check';
            }
            // pending
            if (task.status === 3) {
                response = 'record';
            }
            // not required
            if (task.status === 4) {
                response = 'next-media';
            }
            // not required
            if (task.status === 5) {
                response = 'stop';
            }

            return response;
        };
    }

    angular.module('umbraco').filter('iconName', iconName);

})();