
angular.module('DeepDev.controllers')
.controller('AccountCtrl',['$scope', '$rootScope', '$http', '$window', '$location', '$timeout', 'server', 'local',
function ($scope, $rootScope, $http, $window, $location, $timeout, server, local) { //, $timeout, $routeParams, $location, $http, $rootScope, ngDialog, Geo, Flickr, data, Platform, apiService, localAuth, storageService) {

    /* Initialization */
    $scope.user = { Name: '', Email: '', Password: '' };
    $scope.message = '';
    $rootScope.hide = false;
    $rootScope.title = 'Account';

    $scope.$on('$viewContentLoaded', function () {
        //console.log('Autologin');
        //local.getCurrentUser(gotUser, failedGetUser);
        $rootScope.data = null;
        local.initialize(null, $scope, gotUser);
        $scope.user = { Name: $rootScope.userName, Email: $rootScope.userEmail, Password: '', Id: $rootScope.userId, Organization: $rootScope.tokenOrganization };
    });


    var failedGetUser = function () {
        local.logout();
        if ($location.path() == "/login" || $location.path() == "/logout" || $location.path() == "/signup") {
            //console.log("OK to be not logged in at " + $location.path());
        } else {
            //console.log("Not OK to be not logged in at " + $location.path());
            $rootScope.returnTo = $location.path();
            $location.path("/login");
        }
    }

    var gotUser = function () {
        //console.log('Got user');
        $scope.user = { Name: $rootScope.userName, Email: $rootScope.userEmail, Password: '', Id: $rootScope.userId, Organization: $rootScope.tokenOrganization };
        // $location.path("/login");
    }

    /* API */
    $scope.executeLogin = function () {
        $scope.loading = true;
        server.login($scope.user, loginSuccess, loginSignupFail);
    };

    $scope.executeSignup = function () {
        $scope.loading = true;
        $scope.user.Id = -1;
        server.signup($scope.user, signupSuccess, loginSignupFail);
    };

    $scope.executeUpdate = function (user) {
        $scope.loading = true;
        server.updateUser({ Name: user.Name, Email: user.Email, Password: '', Id: $rootScope.userId, Organization: user.Organization}, loginSuccess, loginSignupFail);
    }

    $scope.executeLogout = function () {
        token = local.getToken();
        $scope.loading = true;
        server.logout(token, logoutSuccess, logoutFail);
    };

    $scope.executeReset = function () {
        $scope.loading = true;
        server.resetPassword($scope.user.Email, function () { $location.path("/set-new-password"); }, loginSignupFail);
    };

    $scope.executeSetNewPassword = function () {
        $scope.loading = true;
        server.newPassword($scope.user, loginSuccess, loginSignupFail);
    };

    /* Event handlers */

    var signupSuccess = function (data) {
        loginSuccess(data);
    }

    var loginSuccess = function (data) {
        concludeEvent('Login success', false);
        local.setCurrentUser(data);
        $scope.message = 'Welcome ' + data.UserName;
    }

    var loginSignupFail = function (error) {
        concludeEvent(error.Text, true);
    }

    var logoutSuccess = function (data) {
        concludeEvent("Logged out", true);
        /* Only when the user willfully logs out clear the local storage*/
        local.clearLocalUser();
        $location.path('/home');
    }

    var logoutFail = function () {
        concludeEvent("Error: failed to logout", true);
    }

    var concludeEvent = function(text, clear){
        $scope.loading = false;
        //console.log(text);
        $scope.message = text;
        if (clear) {
            local.logout();
        } else {
            redirectToMain();
        }
        
        
    }

    var redirectToMain = function () {
        if ($rootScope.returnTo) {
            $timeout(function () {
                //console.log("returning to " + $rootScope.returnTo);
                $location.path($rootScope.returnTo);
            }, 100);
        } else {
            $timeout(function () {
                //console.log("going to main page");
                $location.path("/");
            }, 100);
        }
    }
}]);