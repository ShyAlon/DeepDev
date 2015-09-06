'use strict';

// declare top-level module which depends on filters,and services
var deepDev = angular.module('deepDev',
    ['DeepDev.filters',
        'DeepDev.directives', // custom directives
        'DeepDev.controllers',
        'DeepDev.services',
        'ngGrid', // angular grid
        'ngSanitize', // for html-bind in ckeditor
        'ngRoute', // routing?
        'ngAnimate',
        'mgcrea.ngStrap',
        'angular-intro',
        'pascalprecht.translate',
        'angular-loading-bar',
        'angular-advanced-searchbox',
        'flow',
        'textAngular',
    ]);


var filters = angular.module('DeepDev.filters', []);
var directives = angular.module('DeepDev.directives', []);
var controllers = angular.module('DeepDev.controllers', []);
var services = angular.module('DeepDev.services', []);

// bootstrap angular
deepDev.config(['$routeProvider', '$locationProvider', '$httpProvider', '$translateProvider', '$tooltipProvider', 'flowFactoryProvider'
    , function ($routeProvider, $locationProvider, $httpProvider, $translateProvider, $tooltipProvider, flowFactoryProvider) {

        /////////////////////////////
        //      Translations       //
        /////////////////////////////
        $translateProvider.translations('en', englishTranslation);
        $translateProvider.preferredLanguage('en');
        // TODO use html5 *no hash) where possible
        // $locationProvider.html5Mode(true);

        /* ToolTips */
        angular.extend($tooltipProvider.defaults, {
            // placement: "auto",
            html: true,
            delay: { show: 1000, hide: 100 }
        });

        var myCustomData = {
            requestVerificationToken: 'xsrf',
            blueElephant: '42'
        };



        flowFactoryProvider.defaults = {
            target: '/Server/api/FileUpload',  //'api/upload',
            permanentErrors: [404, 500, 501],
            maxChunkRetries: 1,
            chunkRetryInterval: 1000,
            simultaneousUploads: 4,
            query: myCustomData,
            
        };
        flowFactoryProvider.on('catchAll', function (event) {
            console.log('catchAll', arguments);
        });


    $routeProvider.when('/', {
        templateUrl: 'partials/home.html',
        controller: 'homeCtrl'
    });

    $routeProvider.when('/home', {
        templateUrl: 'partials/home.html',
        controller: 'homeCtrl'
    });

    $routeProvider.when('/process', {
        templateUrl: 'partials/process.html',
        controller: 'homeCtrl'
    });

    $routeProvider.when('/fs2', {
        templateUrl:'partials/fs2/fs2.html',
        controller: 'FS2Controller'
    });

    $routeProvider.when('/contact', {
        templateUrl:'partials/contact.html'
    });
    $routeProvider.when('/about', {
        templateUrl:'partials/about.html'
    });
    $routeProvider.when('/faq', {
        templateUrl:'partials/faq.html'
    });

    // note that to minimize playground impact on app.js, we
    // are including just this simple route with a parameterized 
    // partial value (see playground.js and playground.html)
    $routeProvider.when('/playground/:widgetName', {
        templateUrl:'playground/playground.html',
        controller:'PlaygroundCtrl'
    });

    ///////////////////////////////////////////////////////////////
    // Actual controllers
    ///////////////////////////////////////////////////////////////

    /* Account */
    $routeProvider.when('/login', { templateUrl: 'partials/user/login.html', controller: 'AccountCtrl' });
    $routeProvider.when('/logout', { templateUrl: 'partials/user/logout.html', controller: 'AccountCtrl' });
    $routeProvider.when('/signup', { templateUrl: 'partials/user/signup.html', controller: 'AccountCtrl' });
    $routeProvider.when('/reset-password', { templateUrl: 'partials/user/reset-password.html', controller: 'AccountCtrl' });
    $routeProvider.when('/set-new-password', { templateUrl: 'partials/user/set-new-password.html', controller: 'AccountCtrl' });
    $routeProvider.when('/edit-user', { templateUrl: 'partials/user/edit.html', controller: 'AccountCtrl' });

    /* Entities */
    $routeProvider.when('/Edit/:entity/:parentId/:entityId', { templateUrl: 'partials/editEntity.html', controller: 'entityCtrl' });

    /* Search */
    $routeProvider.when('/Search', { templateUrl: 'partials/search.html', controller: 'searchCtrl' });

    /* Document*/
    $routeProvider.when('/Document/:projectId/:documentId', { templateUrl: 'partials/document/createDocument.html', controller: 'DocumentCtrl' });

    ///////////////////////////////////////////////////////////////
    // End actual controllers
    ///////////////////////////////////////////////////////////////



    // by default, redirect to site root
    $routeProvider.otherwise({
        redirectTo: '/home'
    });

    }]).factory('authInterceptor', ['$rootScope', '$q', '$window', function ($rootScope, $q, $window) {
    return {
        request: function (config) {
            config.headers = config.headers || {};
            config.headers.Authorization = $rootScope.tokenText;
            return config;
        },
        response: function (response) {
            if (response.status === 401) {
                // handle the case where the user is not authenticated
            }
            return response || $q.when(response);
        }
    };
    }]).config(['$httpProvider',function ($httpProvider) {
    $httpProvider.interceptors.push('authInterceptor');
    delete $httpProvider.defaults.headers.common['X-Requested-With'];
}]);

// this is run after angular is instantiated and bootstrapped
deepDev.run(['$rootScope', '$location', '$http', '$timeout', 'AuthService', function ($rootScope, $location, $http, $timeout, AuthService) {
    $rootScope.authService = AuthService;
    // text input for login/password (only)
    $rootScope.loginInput = 'user@gmail.com';
    $rootScope.passwordInput = 'complexpassword';

    $rootScope.$watch('authService.authorized()', function () {

        // if never logged in, do nothing (otherwise bookmarks fail)
        if ($rootScope.authService.initialState()) {
            // we are public browsing
            return;
        }

        // instantiate and initialize an auth notification manager
        $rootScope.authNotifier = new NotificationManager($rootScope);

        // when user logs in, redirect to home
        if ($rootScope.authService.authorized()) {
            $location.path("/");
            $rootScope.authNotifier.notify('information', 'Welcome ' + $rootScope.authService.currentUser() + "!");
        }

        // when user logs out, redirect to home
        if (!$rootScope.authService.authorized()) {
            $location.path("/");
            $rootScope.authNotifier.notify('information', 'Thanks for visiting.  You have been signed out.');
        }

    }, true);

    // TODO move this out to a more appropriate place
    $rootScope.faq = [
        {key: "What is Angular-Enterprise-Seed?", value: "A starting point for server-agnostic, REST based or static/mashup UI."},
        {key: "What are the pre-requisites for running the seed?", value: "Just an HTTP server.  Add your own backend."},
        {key: "How do I change styling (css)?", value:  "Change Bootstrap LESS and rebuild with the build.sh script.  This will update the appropriate css/image/font files."}
    ];


}]);