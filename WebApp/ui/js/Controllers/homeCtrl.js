
angular.module('DeepDev.controllers')
.controller('homeCtrl', ['$scope', '$rootScope', '$http', '$window', '$location', '$animate', '$timeout', '$routeParams', 'server', 'local', 'authentication', 'ui',
function ($scope, $rootScope, $http, $window, $location, $animate, $timeout, $routeParams, server, local, authentication, ui) { //, $timeout, $routeParams, $location, $http, $rootScope, ngDialog, Geo, Flickr, data, Platform, apiService, localAuth, storageService) {

    /* Initialization */
    $scope.$on('$viewContentLoaded', function () {
        // local.setBackButton('DeepDev', 'home');
        authentication.initialize(function () {
            getData();
        }, function () { 
            console.log('OK to be logged out in Home');
        });
    });

    var getData = function () {
        server.getEntity({ id: $rootScope.userId }, gotUserData, failedGetUserData, "User");
    }

    var gotUserData = function (item) {
        local.initialize(item, $scope, gotUserData);
    }

    var failedGetUserData = function (failure) {
        $rootScope.loading = false;
        console.log("Failed to get user in  " + $location.path());
        console.log(failure);
    }

    $scope.$on('flow::fileAdded', function (event, $flow, flowFile) {
        //if (flowFile.name.indexOf(".zip") < 1) {
        //    event.preventDefault();
        //}
        // event.preventDefault();//prevent file from uploading
        $flow.defaults.headers = $flow.defaults.headers || {};
        $flow.defaults.headers.Authorization = $rootScope.tokenText;
        console.log($flow);
    });

    $scope.$on('flow::complete', function (event, $flow, flowFile) {
        // Save only after complete
        console.log("Note save");
        $scope.$flow.files = [];
    });

    $scope.importProject = function () {
        if ($scope.$flow.files.length > 0) {
            $scope.$flow.resume();
        }
    }


    /* API */

    /* Tabs */
    $scope.tabs = [
        {
            "title": "The Product",
            "template": "partials/process/product.html",
            //"content": "Raw denim you probably haven't heard of them jean shorts Austin. Nesciunt tofu stumptown aliqua, retro synth master cleanse. Mustache cliche tempor, williamsburg carles vegan helvetica."
        },
        {
            "title": "The Design",
            "template": "partials/process/engineering.html",
        },
        {
            "title": "The Project",
            "template": "partials/process/project.html",
        }
    ];
    $scope.tabs.activeTab = 0;
}]);