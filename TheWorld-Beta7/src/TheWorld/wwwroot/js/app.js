appWorld = {};
appWorld.api = {};
appWorld.api.paths = [
    {
        name: "trips",
        path: "/api/trips"
    },
    {name: "stops",
        path: "/api/stops"
        
    }
];

appWorld.api.trips = GetApiDefinition('trips');
appWorld.api.stops = GetApiDefinition('stops');

if (!Array.prototype.filter) {
    Array.prototype.filter = function (fun /*, thisp*/) {
        var len = this.length >>> 0;
        if (typeof fun != "function")
            throw new TypeError();

        var res = [];
        var thisp = arguments[1];
        for (var i = 0; i < len; i++) {
            if (i in this) {
                var val = this[i]; // in case fun mutates this
                if (fun.call(thisp, val, i, this))
                    res.push(val);
            }
        }
        return res;
    };
}

function GetApiDefinition(apiValue) {
    return appWorld.api.paths.filter
                   (function (el) { return el.name == apiValue; })[0];
}