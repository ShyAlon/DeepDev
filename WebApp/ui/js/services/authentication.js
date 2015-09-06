angular.module('DeepDev.services')
.service('authentication', ['$http', '$q', '$rootScope', '$location', '$window', '$timeout', 'server', 'local', function ($http, $q, $rootScope, $location, $window, $timeout, server, local) {

    var periodicLogin = function () { // Check user every 10 minute
        $timeout(function () {
            local.getCurrentUser(function () {
                //console.log("periodicLogin succeeded at " + (new Date()).toUTCString());
            }, function () {
                //console.log("periodicLogin failed at " + (new Date()).toUTCString());
            });
            periodicLogin();
        }, 1000 * 60 * 10);
    };

    var failedGetUser = function () {
        console.log("Not OK to be not logged in at " + $location.path());
        local.logout();
        $rootScope.returnTo = $location.path();
        $location.path("/login");
    };

    return {
        initialize: function (success, failed) {
            //console.log('Autologin');
            if (!failed) {
                failed = failedGetUser;
            }
            local.getCurrentUser(function () {
                $rootScope.title = 'UIBuildIt';
                if (!$rootScope.periodicLoginStarted) {
                    $rootScope.periodicLoginStarted = true;
                    periodicLogin();
                }
                success();
            }, function () {
                if (failed) {
                    failed();
                }
            });   
        }
    }
}]);