//tripsController.js
(function() {

    "use strict";

    //Getting the existing module
    angular.module("app-trips")
        .controller("tripsController", tripsController);

    function tripsController($http,$scope) {
        var vm = this;
        vm.name = "Shawn";
        vm.trips = [];
        vm.newTrip = {};

        vm.errorMessage =
        vm.pendingRequests = 0;
       // vm.isBusy = true;

        $http.get("/api/trips")
            .then(function success(response) {
                //success callback
                angular.copy(response.data, vm.trips);
                
            }, function failure(error) {
                //failire callback
                vm.errorMessage = "Failed to load data: " + error;
            }).finally(function() {
                vm.isBusy = false;
            });  

        vm.addTrip = function() {
           // vm.isBusy = true;
            vm.errorMessage = "";

            $http.post(appWorld.api.trips.path, vm.newTrip)
                .then(function successAdd(response) {
                    vm.trips.push(response.data);
                    vm.newTrip = {};
                    }
                , function errorAdd() {
                        vm.errorMessage = "Failed to save new trip";
                    })
                .finally(function() {
             //       vm.isBusy = false;
                }
           );

        };

        $scope.$watch(function () {
            vm.pendingRequests = $http.pendingRequests.length;
            return $http.pendingRequests.length > 0;
        }, function (hasPending) {
            if (hasPending) {
                vm.isBusy = true;
            }
            else {
                vm.isBusy = false;
            }
        });

    }
})();