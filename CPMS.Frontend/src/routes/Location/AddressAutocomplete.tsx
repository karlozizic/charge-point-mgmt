import React, { useState, useEffect, useRef } from 'react';

interface AddressData {
    address: string;
    city: string;
    country: string;
    latitude: number;
    longitude: number;
}

interface Suggestion {
    id: string;
    place_name: string;
    center: [number, number];
    context?: Array<{ id: string; text: string }>;
}

interface Props {
    onSelect: (data: AddressData) => void;
}

const AddressAutocomplete: React.FC<Props> = ({ onSelect }) => {
    const [query, setQuery] = useState('');
    const [suggestions, setSuggestions] = useState<Suggestion[]>([]);
    const [isLoading, setIsLoading] = useState(false);
    const [showDropdown, setShowDropdown] = useState(false);

    const timeoutRef = useRef<number>(0);
    const token = import.meta.env.VITE_MAPBOX_ACCESS_TOKEN;

    useEffect(() => {
        if (!token || query.length < 3) {
            setSuggestions([]);
            setShowDropdown(false);
            return;
        }

        if (timeoutRef.current) {
            clearTimeout(timeoutRef.current);
        }

        timeoutRef.current = setTimeout(async () => {
            setIsLoading(true);
            try {
                const url = `https://api.mapbox.com/geocoding/v5/mapbox.places/${encodeURIComponent(query)}.json?access_token=${token}&types=address&limit=5`;
                const response = await fetch(url);
                const data = await response.json();

                setSuggestions(data.features || []);
                setShowDropdown(true);
            } catch (error) {
                console.error('Search error:', error);
                setSuggestions([]);
            }
            setIsLoading(false);
        }, 300);
    }, [query, token]);

    const handleSelect = (suggestion: Suggestion) => {
        const context = suggestion.context || [];
        let city = '';
        let country = '';

        context.forEach(item => {
            if (item.id.includes('place')) city = item.text;
            if (item.id.includes('country')) country = item.text;
        });

        let address = suggestion.place_name;
        if (city) address = address.replace(`, ${city}`, '');
        if (country) address = address.replace(`, ${country}`, '');

        setQuery(suggestion.place_name);
        setShowDropdown(false);

        onSelect({
            address: address.trim(),
            city: city || '',
            country: country || '',
            latitude: suggestion.center[1],
            longitude: suggestion.center[0]
        });
    };

    if (!token) {
        return (
            <input
                type="text"
                placeholder="Mapbox token required"
                disabled
            />
        );
    }


    return (
        <div className="autocomplete">
            <input
                type="text"
                value={query}
                onChange={(e) => setQuery(e.target.value)}
                onBlur={() => setTimeout(() => setShowDropdown(false), 200)}
                placeholder="Search for an address..."
                autoComplete="off"
            />

            {isLoading && <div className="loading-text">Searching...</div>}

            {showDropdown && suggestions.length > 0 && (
                <div className="suggestions">
                    {suggestions.map((suggestion) => (
                        <div
                            key={suggestion.id}
                            onClick={() => handleSelect(suggestion)}
                            className="suggestion"
                        >
                            {suggestion.place_name}
                        </div>
                    ))}
                </div>
            )}
        </div>
    );
};

export default AddressAutocomplete;