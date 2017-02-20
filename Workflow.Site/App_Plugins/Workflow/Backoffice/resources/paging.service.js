(function () {
    function pagingService() {
        return {
            countPages: function (i, j) {
                return Math.ceil(parseInt(i, 10) / parseInt(j, 10));
            },

            updatePaging: function (items, filter, currentPage, numPerPage) {

                var begin = (currentPage - 1) * numPerPage,
                    end = begin + numPerPage,
                    numPages, 
                    paged = items;

                if (filter.length) {
                    paged = this.getFilteredPage(items, filter);
                }

                numPages = Math.ceil(paged.length / numPerPage);

                if (currentPage > numPages) {
                    begin = 0;
                    end = begin + numPerPage;
                    currentPage = 1;
                }

                return { items: paged.slice(begin, end), numPages: numPages, currentPage: currentPage, filter: filter };
            },

            getFilteredPage: function (items, term) {

                var paged = [],
                    i,
                    j,
                    o,
                    keys,
                    l = items.length,
                    pushed;

                for (i = 0; i < l; i += 1) {
                    o = items[i];
                    pushed = false;

                    if (typeof o === 'object') {
                        keys = Object.keys(o);
                        for (j = 0; j < keys.length; j++) {
                            if (!pushed && o[keys[j]] !== undefined && o[keys[j]] !== null && o[keys[j]].toString().toLowerCase().indexOf(term.toString().toLowerCase()) !== -1) {
                                paged.push(o);
                                pushed = true;
                            }
                        }
                    } else {
                        if (o.toLowerCase().indexOf(term.toLowerCase()) !== -1) {
                            paged.push(o);
                        }
                    }
                }
                return paged;
            },

            prevPage: function (i) {
                return i - 1 > 0 ? i - 1 : i;
            },

            nextPage: function (i, j) {
                return i + 1 <= j ? i + 1 : i;
            }
        };

    }
    angular.module("umbraco.services").factory('workflowPagingService', pagingService);
}());