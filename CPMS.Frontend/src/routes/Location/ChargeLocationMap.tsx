import {useEffect, useRef} from "react";
import type {ChargeLocation} from "../../types/chargeLocation.ts";

interface ChargeLocationMapProps {
    locations: ChargeLocation[];
}

interface LeafletMap {
    remove: () => void;
    setView: (center: [number, number], zoom: number) => LeafletMap;
    fitBounds: (bounds: LeafletBounds, options?: { padding?: number }) => void;
}

interface LeafletMarker {
    addTo: (map: LeafletMap) => LeafletMarker;
    bindPopup: (content: string) => LeafletMarker;
}

interface LeafletTileLayer {
    addTo: (map: LeafletMap) => void;
}

interface LeafletBounds {
    pad: (value: number) => LeafletBounds;
}

interface LeafletFeatureGroup {
    getBounds: () => LeafletBounds;
}

interface LeafletLibrary {
    map: (element: HTMLElement) => LeafletMap;
    tileLayer: (url: string, options: { attribution: string }) => LeafletTileLayer;
    marker: (coords: [number, number]) => LeafletMarker;
    featureGroup: (markers: LeafletMarker[]) => LeafletFeatureGroup;
}

declare global {
    interface Window {
        L: LeafletLibrary;
    }
}

function ChargeLocationMap({ locations }: ChargeLocationMapProps) {
    const mapRef = useRef<HTMLDivElement>(null);
    const mapInstance = useRef<LeafletMap | null>(null);

    useEffect(() => {
        if (!mapRef.current) return;

        const initMap = () => {
            const L = window.L;
            if (!L) {
                const link = document.createElement('link');
                link.rel = 'stylesheet';
                link.href = 'https://unpkg.com/leaflet@1.9.4/dist/leaflet.css';
                document.head.appendChild(link);

                const script = document.createElement('script');
                script.src = 'https://unpkg.com/leaflet@1.9.4/dist/leaflet.js';
                script.onload = () => initMapWithData();
                document.head.appendChild(script);
            } else {
                initMapWithData();
            }
        };

        const initMapWithData = () => {
            const L = window.L;
            if (!L || !mapRef.current) return;

            const defaultCenter: [number, number] = [45.8150, 15.9819];

            if (mapInstance.current) {
                mapInstance.current.remove();
            }

            mapInstance.current = L.map(mapRef.current).setView(defaultCenter, 7);

            L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
                attribution: ''
            }).addTo(mapInstance.current);

            const markers: LeafletMarker[] = [];

            locations
                .filter(loc => loc.latitude && loc.longitude)
                .forEach(location => {
                    if (!mapInstance.current) return;

                    const marker = L.marker([location.latitude!, location.longitude!])
                        .addTo(mapInstance.current);

                    marker.bindPopup(`
                        <strong>${location.name}</strong><br>
                        ${location.address}<br>
                        ${location.city}, ${location.country}<br>
                        <small>${location.totalChargePoints} ChargePoints</small>
                    `);

                    markers.push(marker);
                });

            if (markers.length > 0 && mapInstance.current) {
                const group = L.featureGroup(markers);
                mapInstance.current.fitBounds(group.getBounds().pad(0.1));
            }
        };

        initMap();

        return () => {
            if (mapInstance.current) {
                mapInstance.current.remove();
                mapInstance.current = null;
            }
        };
    }, [locations]);

    return (
        <div className="map-container">
            <div ref={mapRef} style={{ height: '400px', width: '100%' }} />
        </div>
    );
}

export default ChargeLocationMap;