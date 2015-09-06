'use strict';

/* Filters */

angular.module('DeepDev.filters', []).
  filter('interpolate', ['version', function(version) {
    return function(text) {
      return String(text).replace(/\%VERSION\%/mg, version);
    }
  }]).filter('since', function () {
      return function (timestampMillisec, uppercase) {
          var agoStr;
          var currentTime = new Date().getTime();
          var timeDiffSec = Math.floor((currentTime - timestampMillisec) / 1000);

          if (timeDiffSec == 1) {
              agoStr = "1 second ago";
          } else if (timeDiffSec < 60) {
              agoStr = timeDiffSec + " seconds ago";
          } else {
              timeDiffSec = Math.floor(timeDiffSec / 60);
              if (timeDiffSec == 1) {
                  agoStr = "1 minute ago";
              } else if (timeDiffSec < 60) {
                  agoStr = timeDiffSec + " minutes ago";
              } else {
                  timeDiffSec = Math.floor(timeDiffSec / 60);
                  if (timeDiffSec == 1) {
                      agoStr = "1 hour ago";
                  } else if (timeDiffSec < 24) {
                      agoStr = timeDiffSec + " hours ago";
                  } else {
                      timeDiffSec = Math.floor(timeDiffSec / 24);
                      if (timeDiffSec == 1) {
                          agoStr = "1 day ago";
                      } else if (timeDiffSec < 24) {
                          agoStr = timeDiffSec + " days ago";
                      } else {
                          timeDiffSec = Math.floor(timeDiffSec / 30);

                          if (timeDiffSec == 1) {
                              agoStr = "1 month ago";
                          } else {
                              agoStr = timeDiffSec + " months ago";
                          }
                      }
                  }
              }
          }
          return agoStr;
      }
  }).filter('complexity', function () {
      return function (val) {
          alert(val);
          if (val == 0) {
              return 'Simple';
          } else if (val == 1) {
              return 'Integrated';
          } else if (val == 2) {
              return 'Enterprise';
          }
          return 'Invalid'; 
      }
  }).filter('truncate', function () {
      return function (text, length, end) {
          if (isNaN(length))
              length = 10;

          if (!end)
              end = "...";

          if (!text)
              return "...";

          if (text.length <= length || text.length - end.length <= length) {
              return text;
          }
          else {
              return String(text).substring(0, length - end.length) + end;
          }

      };
  }).filter('ordinal', function() {
      return function (input) {
          if (!input) {
              return '';
          }
          input = input.toString();
          var result = input.charAt(0).toUpperCase() + input.substr(1).replace(/([a-z])([A-Z])/g, '$1 $2');
          return result;
      }
  }).filter('typeToSingle', function () {
      return function (input) {
          if (!input) {
              return '';
          }
          var result = input.charAt(0).toUpperCase() + input.substr(1).replace(/([a-z])([A-Z])/g, '$1 $2');
          result = result.toLowerCase();
          if(result.charAt(result.length-1) == "s"){
              result = result.slice(0, -1);
          }
          return result;
      }
  }).filter('htmlToPlaintext', function() {
      return function(text) {
          return String(text).replace(/<[^>]+>/gm, '');
      }
  }
);
