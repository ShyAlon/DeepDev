angular.module('ahp').controller('criteriaCtrl', [
'$scope', 'ahp',
function($scope, ahp){

  $scope.isAddVisible = false;

  $scope.newCriteria = {};
  
  $scope.criteria = ahp.getCriteria();

  $scope.remove = function(criteria) {
    ahp.removeCriteria(criteria);
  };

   $scope.addOpen = function() {
    $scope.isAddVisible = true;
  };
    
  $scope.addClose = function(){
    $scope.isAddVisible = false;
  };
  
  $scope.add = function() {
    ahp.addCriteria(new ahp.Criteria($scope.newCriteria.name));
    $scope.newCriteria = {};
    $scope.addClose();
  };
  
}]);