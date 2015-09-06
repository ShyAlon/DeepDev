var app = angular.module('ahp', [
  'ng',
  'ngRoute',
  'angularify.semantic.modal',
  'DWand.nw-fileDialog',
  'xeditable'
]);

app.config(['$routeProvider', '$locationProvider', function($routeProvider, $locationProvider) {
  $locationProvider.html5Mode(false);
  $locationProvider.hashPrefix('!');
  
  $routeProvider
    .when('/alternatives', {
      templateUrl: 'view/alternatives.htm',
      controller: 'alternativesCtrl'
    })
    .when('/criteria', {
      templateUrl: 'view/criteria.htm',
      controller: 'criteriaCtrl'
    })
    .when('/criteria/configure/:index', {
      templateUrl: 'view/criteria-configure.htm',
      controller: 'criteriaConfigureCtrl'
    })
    .when('/criteria/compare', {
      templateUrl: 'view/criteria-compare.htm',
      controller: 'criteriaCompareCtrl'
    })
    .when('/result', {
      templateUrl: 'view/result.htm',
      controller: 'resultCtrl'
    })
    .otherwise({redirectTo: '/alternatives'});
}]);


app.run([function(){
  Array.prototype.remove = function(element) {
    for (var i = this.length - 1; i > -1; i--) {
      if (this[i] === element) {
        this.splice(i, 1);
      }
    }
  };
  
  Array.prototype.normalize = function() {
    var sum = 0;
    for (var i = 0, len = this.length; i < len; i++) {
      sum += this[i];
    }
    for (var i = 0, len = this.length; i < len; i++) {
      this[i] /= sum;
    }
  };
}]);


app.run(['$location', function($location){
  $location.path('/');
}]);

/*
// For debugging
app.run([function(){
  var gui = require('nw.gui');
  var win = gui.Window.get();

  function detectspecialkeys(e){
    var evt = window.event? event : e
    if (evt.ctrlKey) {
      switch (evt.keyCode) {
        case 4:
          // Ctrl + D
          win.showDevTools();
        break;
        
        case 18:
          // Ctrl + R
          win.reload();
        break;
      }
    }
  }
  document.onkeypress=detectspecialkeys;
}]);
*/

// For debugging
app.run([
'ahp',
function(ahp){
  window.ahp = ahp;
}]);


app.run([
'editableThemes',
function(editableThemes){
  editableThemes['default'].formTpl = '<form name="editForm" class="ui form editable-wrap"></form>';
  editableThemes['default'].controlsTpl = '<span class="ui input editable-controls"></span>';
}
]);