$(document).ready(function() {
    $(document).ready(function () {
        $(window).keydown(function (event) {
            if (event.keyCode === 13) {
                event.preventDefault();
                return false;
            }
        });
    });

    var vm = new Vue({
        el: '#app',
        data: {
            customers: null,
            exceptions: window.preLoadeddata.Exceptions,
            holidays: window.preLoadeddata.HolidayDates,
            routes: window.preLoadeddata.Routes,
            selectedRoute: window.preLoadeddata.SelectedRoute,
            calendar: window.preLoadeddata.Calendar,
            routeOriginal: window.preLoadeddata.Routes[0], // selected route
            routeChanged: JSON.parse(JSON.stringify(window.preLoadeddata.Routes[0])), // any changes to route goes here
            canEditRoute: JSON.parse(JSON.stringify(window.preLoadeddata.CanEditRoute)),
            indexUrl: window.preLoadeddata.IndexUrl,
            customersUrl: window.preLoadeddata.CustomersUrl,
            nextWeekUrl: window.preLoadeddata.NextWeekUrl,
            previousWeekUrl: window.preLoadeddata.PreviousWeekUrl,
            nextFourWeekUrl: window.preLoadeddata.NextFourWeekUrl,
            previousFourWeekUrl: window.preLoadeddata.PreviousFourWeekUrl,
            SitesUrl: window.preLoadeddata.SitesUrl,
            OrderTypesUrl: window.preLoadeddata.OrderTypesUrl,
            AllSitesUrl: window.preLoadeddata.AllSitesUrl,
            sortMethod: localStorage.getItem('sortMethod') || 'number'
        },
        created: function () {
            //set handler to watch route name, if it changes post the changes to the server and update model
            if (this.selectedRoute !== null) {
                this.routeOriginal = this.getRoute(this.selectedRoute.RouteID);
                this.displayRouteInfo(this.getRoute(this.selectedRoute.RouteID));
            }
        },
        mounted: function () {
            this.setCustomers();
        },
        computed: {
            orderedRoutes: function () {
                localStorage.setItem('sortMethod', this.sortMethod);
                if (this.sortMethod === 'description') {
                    const comp = (a, b) => {
                        let aName = a.Description
                        let bName = b.Description

                        if (aName < bName) {
                            return -1;
                        }
                        if (aName > bName) {
                            return 1;
                        }
                        return 0;
                    }
                    return this.routes.sort(comp)
                }
                else if (this.sortMethod === 'number') {
                    const comp = (a, b) => {
                        let aNumber = a.RouteNumber
                        let bNumber = b.RouteNumber

                        if (aNumber < bNumber) {
                            return -1;
                        }
                        if (aNumber > bNumber) {
                            return 1;
                        }
                        return 0;
                    }
                    return this.routes.sort(comp)
                }
            },
            orderedUsers: function () {
                return this.customers.orderBy(this.routeChanged.CustomSettings, 'CustomerId')
            },
        },
        methods: {
            getNextWeek() {
                var self = this;
                $.ajax({
                    type: "POST",
                    url: this.nextWeekUrl,
                    data: {
                        calendar: self.calendar.Week[0].DateString,
                    },
                    success: function (data) {
                        // Run the code here that needs to access the data returned
                        self.calendar = data;
                    },
                    error: function () {
                        alert('Error occured');
                    }
                });
            },
            getPreviousWeek() {
                var self = this;
                $.ajax({
                    type: "POST",
                    url: this.previousWeekUrl,
                    data: {
                        calendar: self.calendar.Week[0].DateString,
                    },
                    success: function (data) {
                        // Run the code here that needs to access the data returned
                        self.calendar = data;
                    },
                    error: function () {
                        alert('Error occured');
                    }
                });
            },
            getNextFourWeek() {
                var self = this;
                $.ajax({
                    type: "POST",
                    url: this.nextFourWeekUrl,
                    data: {
                        calendar: self.calendar.Week[0].DateString,
                    },
                    success: function (data) {
                        // Run the code here that needs to access the data returned
                        self.calendar = data;
                    },
                    error: function () {
                        alert('Error occured');
                    }
                });
            },
            getPreviousFourWeek() {
                var self = this;
                $.ajax({
                    type: "POST",
                    url: this.previousFourWeekUrl,
                    data: {
                        calendar: self.calendar.Week[0].DateString,
                    },
                    success: function (data) {
                        // Run the code here that needs to access the data returned
                        self.calendar = data;
                    },
                    error: function () {
                        alert('Error occured');
                    }
                });
            },
            // displays route info
            displayRouteInfo(route) {
                if (this.canEditRoute && !this.objectsEqual(this.routeOriginal, this.routeChanged)) {
                    if (confirm('Do you want to save your changes?')) {
                        this.onSubmit(route);
                    }
                }
                this.routeOriginal = route;
                this.routeChanged = JSON.parse(JSON.stringify(route));
                this.customers = this.setCustomers();
            },

            setCustomers() {
                // start loading
                $('#customers-content').toggleClass('sk-loading');

                var self = this;
                $.ajax({
                    type: "POST",
                    url: this.customersUrl,
                    data: {
                        routeId: this.routeChanged.RouteID,
                    },
                    success: function (data) {
                        // Run the code here that needs
                        //    to access the data returned
                        self.customers = data;

                        // stop loading
                        $('#customers-content').toggleClass('sk-loading');
                    },
                    error: function () {
                        alert('Error occured');

                        // stop loading
                        $('#customers-content').toggleClass('sk-loading');
                    }
                });
            },
            // update route
            onSubmit(route) {
                if (this.objectsEqual(this.routeOriginal, this.routeChanged)) {
                    var alertWarning = document.getElementById('alert-warning');
                    $(alertWarning).fadeIn("slow")
                        .delay(3000)
                        .fadeOut('slow');
                }
                else {
                    var self = this;
                    startSpin();
                    $.ajax({
                        //context: this,
                        type: 'POST',
                        url: this.indexUrl,
                        //data: JSON.stringify(this.routeChanged),
                        data: {
                            route: self.routeChanged,
                        },
                        //contentType: 'application/json; charset=utf-8',
                        success: function (data) {
                            //this.routes = data.routes;
                            //var newRoute = this.routes.filter(function (ele) {
                            //    return ele.RouteID === route.RouteID;
                            //})[0];
                            //this.routeOriginal = newRoute;
                            //this.routeChanged = JSON.parse(JSON.stringify(route));
                            $('#loading-indicator').hide();
                            //stopSpin();
                            redirect("Success");
                            //var alertSuccess = document.getElementById('alert-success');
                            //$(alertSuccess).fadeIn("slow")
                            //    .delay(3000)
                            //    .fadeOut('slow');
                        },
                        error: function (err) {
                            //this.routeChanged = this.routeOriginal;
                            alert(err.responseText);
                            stopSpin();
                            var alertFailure = document.getElementById('alert-danger');
                            $(alertFailure).fadeIn("slow")
                                .delay(5000)
                                .fadeOut('slow');
                            //redirect("Failure");
                        }
                    });
                }
            },

            // returns defaultDeliveryDate for route order type and day.
            getDefaultDeliveryDate(day, orderType, site) {
                return this.routeChanged.Deliveries.filter(function (ele) {
                    return day.DayOfWorkWeek === ele.DayOfWeekId && orderType.OrderTypeID === ele.OrderTypeId && site.SiteID === ele.SiteId;
                })[0];
            },

            // returns either exception or default date for route order type and day.
            getDeliveryDate(day, orderType, site) {
                var deliveryDateDefault = this.getDefaultDeliveryDate(day, orderType, site);
                var deliveryDateException = this.getExceptionTest(day, orderType, site);  
                return deliveryDateException === null ? deliveryDateDefault : deliveryDateException;
            },

            // checks to see if day is an exception for route order type.
            isException(day, orderType, site) {
                return this.getException(day, orderType, site);
            },

            isHoliday(day, orderType, site) {
                return this.getHoliday(day, orderType, site);
            },

            // returns the exception for that default delivery and date.
            getException(day, orderType, site) {

                const routeId = this.routeChanged.RouteID;

                var exceptions = this.exceptions.filter(function (ele) {
                    return day.DateString === ele.DateString && orderType.OrderTypeID === ele.OrderTypeId
                        && routeId === ele.RouteId && site.SiteID === ele.SiteId;
                });

                if (exceptions.length === 1) {
                    return exceptions[0];
                }
                return null;
            },

            getHoliday(day, orderType, site) {

                var holidayDates = this.holidays.filter(function (ele) {
                    return day.DateString === ele.HolidayDateString;
                });

                if (holidayDates.length === 1) {
                    return holidayDates[0];
                }
                return null;
            },

            getExceptionTest(day, orderType, site) {
                
                var exceptions = this.routeChanged.DeliveryExceptions.filter(function (ele) {
                    return day.DateString === ele.DateString && orderType.OrderTypeID === ele.OrderTypeId && site.SiteID === ele.SiteId;
                });

                if (exceptions.length === 1) {
                    return exceptions[0];
                }
                return null;
            },

            getRoute(routeID) {
                var results = this.routes.filter(function (ele) {
                    return ele.RouteID === routeID;
                });
                if (results.length === 1) {
                    return results[0];
                }
                return null;
            },

            // helps to see if routes are equal
            objectsEqual(obj1, obj2) {
                return JSON.stringify(obj1) === JSON.stringify(obj2) ? true : false;
            },

            isActive(routeID) {
                return this.routeOriginal.RouteID === routeID;
            },
        }
    })
});

