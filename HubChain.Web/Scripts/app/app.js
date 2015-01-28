(function() {
  requirejs.config({
    baseUrl: '../scripts',
    map: {
      '*': {
        ko: 'knockout'
      }
    },
    shim: {
      bootstrap: ['jquery'],
      breeze: ['Q', 'jquery', 'knockout'],
      sinonie: ['sinon'],
      sigr: ["jquery"],
      "flot.time": ["flot"],
      jqueryFileDownload: ["jquery"]
    },
    paths: {
      sigr: "jquery.signalR-2.1.2",
      jquery: "jquery-2.1.3",
      knockout: "knockout-3.2.0",
      "knockout.punches": "knockout.punches.min",
      "linq": "linqjs-amd"
    }
  });

  require(["jquery", "linq", "sigr", "knockout", "knockout.punches"], function($, linq, signalr, ko) {
    var count, crossConnection, crossProxy, join, leave, starter, toggleJoin, toggleLeave, update, viewModel;
    crossConnection = $.hubConnection("/signalr");
    crossProxy = crossConnection.createHubProxy("SuperBatch");
    viewModel = {
      values: {},
      latest: ko.observableArray()
    };
    count = 0;
    update = function(message) {
      if (viewModel.values[message.Id]) {
        viewModel.values[message.Id].value(message.Value);
        viewModel.values[message.Id].index(message.Index);
        return viewModel.values[message.Id].exception(message.Exception);
      }
    };
    crossProxy.on("UpdateItem", function(message) {
      return update(message);
    });
    starter = crossConnection.start();
    leave = function(groups) {
      return crossProxy.invoke("UnsubscribeItem", groups.map(function(g) {
        return {
          Id: g
        };
      })).then(function() {
        var group, _i, _len, _results;
        _results = [];
        for (_i = 0, _len = groups.length; _i < _len; _i++) {
          group = groups[_i];
          viewModel.latest.remove(viewModel.values[group]);
          _results.push(delete viewModel.values[group]);
        }
        return _results;
      });
    };
    join = function(groups) {
      var group, _base, _i, _len;
      for (_i = 0, _len = groups.length; _i < _len; _i++) {
        group = groups[_i];
        (_base = viewModel.values)[group] || (_base[group] = (function() {
          var ret;
          ret = {
            id: group,
            value: ko.observable('subscribing from client'),
            index: ko.observable(-999),
            exception: ko.observable()
          };
          viewModel.latest.push(ret);
          return ret;
        })());
      }
      return crossProxy.invoke("SubscribeItem", groups.map(function(g) {
        return {
          Id: g
        };
      })).then(function(data) {
        return data.map(function(d) {
          return update(d);
        });
      });
    };
    toggleJoin = function(joins, time) {
      return setTimeout(function() {
        return join(joins).then(function() {
          return setTimeout(function() {
            return toggleLeave(joins, time);
          }, Math.random() * time);
        }).fail(function() {
          return setTimeout(function() {
            return toggleJoin(joins, time);
          }, 1000);
        });
      }, 1000 * Math.random());
    };
    toggleLeave = function(leaves, time) {
      return leave(leaves).then(function() {
        return setTimeout(function() {
          return toggleJoin(leaves, time);
        }, Math.random() * time);
      }).fail(function() {
        return setTimeout(function() {
          return toggleLeave(leaves, time);
        }, 1000);
      });
    };
    starter.done(function() {
      toggleJoin([1876], 5000);
      toggleJoin([1801], 5000);
      toggleJoin([1876], 20000);
      toggleJoin([1855576], 20000);
      toggleJoin(["anwothersss"], 5000);
      return toggleJoin([1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20], 5000);
    });
    return $(document).ready(function() {
      ko.punches.enableAll();
      return ko.applyBindings(viewModel);
    });
  });

}).call(this);
