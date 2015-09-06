angular.module('ahp').provider('ahp', [
function(){
  function Alternative(name, price) {
    this.name = name;
    this.price = price;
    this.priceNormalized = 1;
    this.utility = 1;
  }
  
  function Criteria(name) {
    this.name = name;
    this.type = Criteria.TYPE_NUMERIC;
    this.ratings = [];
    this.map = [];
    this.normalized = [];
    this.priority = 1;
    
    for (var i = 0, len = alternatives.length; i < len; i++) {
      this.addAlternative();
    }
  }
  Criteria.TYPE_NUMERIC = 0;
  Criteria.TYPE_COMPARE = 1;
  Criteria.prototype.addAlternative = function() {
    this.ratings.push(1);
    
    for (var i = 0, len = this.ratings.length - 1; i < len; i++) {
      this.map[i].push(1);
    }
    this.map.push([]);
    for (var i = 0, len = this.ratings.length; i < len; i++) {
      this.map[len - 1].push(1);
    }
  };
  Criteria.prototype.removeAlternative = function(index) {
    this.ratings.splice(index, 1);
    this.map.splice(index, 1);
    for (var i = 0, len = this.map.length; i < len; i++) {
      this.map[i].splice(index, 1);
    }
  };
  Criteria.prototype.normalize = function(){
    this.normalized = [];
    if (this.type == Criteria.TYPE_NUMERIC) {
      for (var i = 0, len = this.ratings.length; i < len; i++) {
        this.normalized.push(this.ratings[i]);
      }
    } else {
      var sum;
      for (var i = 0, len = this.ratings.length; i < len; i++) {
        sum = 0;
        for (var j = 0; j < len; j++) {
          sum += this.map[i][j];
        }
        this.normalized.push(sum);
      }
    }
    this.normalized.normalize();
  };
  

  var alternatives = [],
      criteria = [],
      criteriaComparison = [];

  this.Alternative = Alternative;
  this.Criteria = Criteria;
      
  this.addAlternative = function(item) {
    alternatives.push(item);
    for (var i = 0, len = criteria.length; i < len; i++) {
      criteria[i].addAlternative();
    }
  };
  
  this.removeAlternative = function(item) {
    var index = alternatives.indexOf(item);
    if (index < 0) return;
    
    for (var i = 0, len = criteria.length; i < len; i++) {
      criteria[i].removeAlternative(index);
    }
    
    alternatives.remove(item);
  };
  
  this.getAlternatives = function() {
    return alternatives;
  };
  
  this.addCriteria = function(item) {
    criteria.push(item);
    
    for (var i = 0, len = criteria.length - 1; i < len; i++) {
      criteriaComparison[i].push(1);
    }
    criteriaComparison.push([]);
    for (var i = 0, len = criteria.length; i < len; i++) {
      criteriaComparison[len - 1].push(1);
    } 
  };
  
  this.removeCriteria = function(item) {
    var index = criteria.indexOf(item);
    if (index < 0) return;
    
    criteriaComparison.splice(index, 1);
    for (var i = 0, len = criteriaComparison.length; i < len; i++) {
      criteriaComparison[i].splice(index, 1);
    }
    
    criteria.remove(item);
  };
  
  this.getCriteria = function() {
    return criteria;
  };
  
  this.getCriteriaComparison = function() {
    return criteriaComparison;
  };
  
  function calcCriteriaPriorities() {
    var priorities = [];
    var sum;
    for (var i = 0, len = criteria.length; i < len; i++) {
      sum = 0;
      for (var j = 0; j < len; j++) {
        sum += criteriaComparison[i][j];
      }
      priorities.push(sum);
    }
    priorities.normalize();
    for (var i = 0, len = criteria.length; i < len; i++) {
      criteria[i].priority = priorities[i];
    }
  }
  
  function calcAlternativesUtility() {
    var utility;
    for (var i = 0, altLen = alternatives.length; i < altLen; i++) {
      utility = 0;
      for (var j = 0, crLen = criteria.length; j < crLen; j++) {
        utility += criteria[j].normalized[i] * criteria[j].priority;
      }
      alternatives[i].utility = utility;
    }
  }
  
  function normalizeAlternativesPrice() {
    var price = [];
    for (var i = 0, len = alternatives.length; i < len; i++) {
      price.push(alternatives[i].price);
    }
    price.normalize();
    for (var i = 0, len = alternatives.length; i < len; i++) {
      alternatives[i].priceNormalized = price[i];
    }
  }
  
  var self = this;
  this.$get = [function(){
    return {
      Alternative : Alternative,
      Criteria : Criteria,
      
      addAlternative : self.addAlternative,
      removeAlternative : self.removeAlternative,
      getAlternatives : self.getAlternatives,
      
      addCriteria : self.addCriteria,
      removeCriteria : self.removeCriteria,
      getCriteria : self.getCriteria,
      
      getCriteriaComparison : self.getCriteriaComparison,
      
      getSolution : function() {
        for (var i = 0, len = criteria.length; i < len; i++) {
          criteria[i].normalize();
        }
        calcCriteriaPriorities();
        calcAlternativesUtility();
        normalizeAlternativesPrice();
        var result = [];
        for (var i = 0, len = alternatives.length; i < len; i++) {
          result.push({
            alternative : alternatives[i],
            rating : alternatives[i].utility / alternatives[i].priceNormalized
          });
        }
        return result;
      },
      
      serialize : function() {
        return {
          alternatives : alternatives,
          criteria : criteria,
          criteriaComparison : criteriaComparison
        };
      },
      
      deserialize : function(ahpObj) {
        var obj, tmp;
        alternatives = [];
        for (var i = 0, len = ahpObj.alternatives.length; i < len; i++) {
          obj = ahpObj.alternatives[i];
          tmp = new Alternative(obj.name, obj.price);
          alternatives.push(tmp);
        }
        
        criteria = [];
        for (var i = 0, len = ahpObj.criteria.length; i < len; i++) {
          obj = ahpObj.criteria[i];
          tmp = new Criteria(obj.name);
          tmp.type = obj.type;
          tmp.ratings = obj.ratings;
          tmp.map = obj.map;
          criteria.push(tmp);
        }
        
        criteriaComparison = ahpObj.criteriaComparison;
      }
    };
  }];
}]);