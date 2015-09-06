angular.module('ahp').controller('navCtrl', [
'$scope', '$location',
function($scope, $location){

  $scope.isActive = function(page) {
    return $location.path() == page;
  };
  
}]);