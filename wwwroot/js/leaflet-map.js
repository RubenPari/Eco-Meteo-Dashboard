window.mapFunctions = {
    maps: {},
    initMap: function (elementId, centerLat, centerLng, zoom) {
        if (this.maps[elementId]) {
            this.maps[elementId].remove();
        }

        var map = L.map(elementId).setView([centerLat, centerLng], zoom);

        L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
            attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors',
            maxZoom: 18
        }).addTo(map);

        this.maps[elementId] = map;
        return map;
    },
    getMap: function (elementId) {
        return this.maps[elementId];
    },
    addMarker: function (elementId, lat, lng, title, popupContent, iconColor) {
        var map = this.maps[elementId];
        if (!map) return null;

        var icon = L.divIcon({
            className: 'custom-marker',
            html: '<div style="background-color: ' + iconColor + '; width: 20px; height: 20px; border-radius: 50%; border: 2px solid white; box-shadow: 0 2px 5px rgba(0,0,0,0.3);"></div>',
            iconSize: [20, 20],
            iconAnchor: [10, 10]
        });

        var marker = L.marker([lat, lng], { icon: icon }).addTo(map);
        
        if (title) {
            marker.bindTooltip(title);
        }
        
        if (popupContent) {
            marker.bindPopup(popupContent);
        }

        return marker;
    },
    addClickHandler: function (elementId, dotNetHelper) {
        var map = this.maps[elementId];
        if (!map) return;

        map.on('click', function (e) {
            dotNetHelper.invokeMethodAsync('OnMapClick', e.latlng.lat, e.latlng.lng);
        });
    },
    setView: function (elementId, lat, lng, zoom) {
        var map = this.maps[elementId];
        if (!map) return;
        map.setView([lat, lng], zoom);
    },
    addLayerGroup: function (elementId, layerId) {
        var map = this.maps[elementId];
        if (!map) return null;
        
        var layerGroup = L.layerGroup();
        layerGroup.addTo(map);
        
        if (!this.layerGroups) {
            this.layerGroups = {};
        }
        this.layerGroups[layerId] = layerGroup;
        
        return layerGroup;
    },
    removeLayer: function (elementId, layerId) {
        var map = this.maps[elementId];
        if (!map || !this.layerGroups || !this.layerGroups[layerId]) return;
        
        map.removeLayer(this.layerGroups[layerId]);
        delete this.layerGroups[layerId];
    },
    getUserLocation: function () {
        return new Promise(function (resolve, reject) {
            if (!navigator.geolocation) {
                reject(new Error('Geolocation is not supported'));
                return;
            }
            
            navigator.geolocation.getCurrentPosition(
                function (position) {
                    resolve({
                        lat: position.coords.latitude,
                        lng: position.coords.longitude
                    });
                },
                function (error) {
                    reject(error);
                }
            );
        });
    }
};
