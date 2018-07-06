(() => {

    /**
     * Set the icon for the given task, based on the stauts
     * @param { } task 
     * @returns { string } 
     */
    function iconName() {
        return function (task) {
            //rejected
            if ((task.typeId === 1 || task.typeId === 3) && task.status === 7 || task.status === 2) {
                return 'delete';
            }
            // resubmitted or approved
            if (task.typeId === 2 && task.status === 7 || task.status === 1) {
                return 'check';
            }
            // pending
            if (task.status === 3) {
                return 'record';
            }
            // not required
            if (task.status === 4) {
                return 'next-media';
            }
            // not required
            if (task.status === 5) {
                return 'stop';
            }

            return '';
        };
    }

    angular.module('umbraco').filter('iconName', iconName);

})();