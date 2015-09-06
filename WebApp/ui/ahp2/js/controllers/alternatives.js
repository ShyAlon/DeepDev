angular.module('ahp').controller('alternativesCtrl', [
'$scope', 'ahp',
function($scope, ahp){

  $scope.isAddVisible = false;

  $scope.newAlt = {};
  
  $scope.alternatives = ahp.getAlternatives();
  
  $scope.remove = function(alt) {
    ahp.removeAlternative(alt);
  };
  
  $scope.addOpen = function() {
    $scope.isAddVisible = true;
  };
    
  $scope.addClose = function(){
    $scope.isAddVisible = false;
  };
  
  $scope.add = function() {
    ahp.addAlternative(new ahp.Alternative($scope.newAlt.name, parseFloat($scope.newAlt.price)));
    $scope.newAlt = {};
    $scope.addClose();
  };
  
}]);