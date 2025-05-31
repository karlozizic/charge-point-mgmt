import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { chargeLocationsApi } from "../../api/services/chargeLocations.ts";
import { Link } from "react-router-dom";
import { useState } from "react";
import AddressAutocomplete from "./AddressAutocomplete.tsx";

interface AddressData {
    address: string;
    city: string;
    country: string;
    latitude: number;
    longitude: number;
}

function LocationList() {
    const [showModal, setShowModal] = useState(false);
    const [name, setName] = useState('');
    const [selectedAddress, setSelectedAddress] = useState<AddressData | null>(null);
    const [error, setError] = useState('');

    const queryClient = useQueryClient();
    const { data: locations = [], isLoading } = useQuery({
        queryKey: ['locations'],
        queryFn: chargeLocationsApi.getAll,
    });

    const createLocation = useMutation({
        mutationFn: chargeLocationsApi.create,
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['locations'] });
            resetForm();
        },
        onError: () => {
            setError('Failed to create location');
        }
    });

    const resetForm = () => {
        setName('');
        setSelectedAddress(null);
        setError('');
        setShowModal(false);
    };

    const handleSubmit = (e: React.FormEvent) => {
        e.preventDefault();

        if (!name.trim()) {
            setError('Location name is required');
            return;
        }

        if (!selectedAddress) {
            setError('Please select an address from suggestions');
            return;
        }

        createLocation.mutate({
            name: name.trim(),
            address: selectedAddress.address,
            city: selectedAddress.city,
            postalCode: '',
            country: selectedAddress.country,
            latitude: selectedAddress.latitude,
            longitude: selectedAddress.longitude
        });
    };

    if (isLoading) return <div className="loading">Loading locations...</div>;

    return (
        <div className="cp-list">
            <div className="flex-between">
                <h1>Locations</h1>
                <button className="btn" onClick={() => setShowModal(true)}>
                    Add Location
                </button>
            </div>

            {locations.length === 0 ? (
                <div className="empty">
                    <p>No locations found. Add your first location to get started.</p>
                </div>
            ) : (
                <table>
                    <thead>
                    <tr>
                        <th>Name</th>
                        <th>Address</th>
                        <th>City</th>
                        <th>Country</th>
                        <th>ChargePoints</th>
                        <th>Actions</th>
                    </tr>
                    </thead>
                    <tbody>
                    {locations.map(location => (
                        <tr key={location.id}>
                            <td>{location.name}</td>
                            <td>{location.address}</td>
                            <td>{location.city}</td>
                            <td>{location.country}</td>
                            <td>{location.totalChargePoints || 0}</td>
                            <td>
                                <Link to={`/locations/${location.id}`} className="btn btn-gray">
                                    Details
                                </Link>
                            </td>
                        </tr>
                    ))}
                    </tbody>
                </table>
            )}

            {showModal && (
                <div className="overlay">
                    <div className="modal">
                        <div className="modal-header">
                            <h2>Add New Location</h2>
                            <button className="close" onClick={resetForm}>&times;</button>
                        </div>
                        <div className="modal-body">
                            {error && <div className="error-msg">{error}</div>}

                            <form onSubmit={handleSubmit}>
                                <div className="form-group">
                                    <label htmlFor="name">Location Name</label>
                                    <input
                                        type="text"
                                        id="name"
                                        value={name}
                                        onChange={(e) => setName(e.target.value)}
                                        placeholder="e.g. Main Street Station"
                                        required
                                    />
                                </div>

                                <div className="form-group">
                                    <label>Address Search</label>
                                    <AddressAutocomplete onSelect={setSelectedAddress} />
                                    <small>Type at least 3 characters and select from suggestions</small>
                                </div>

                                {selectedAddress && (
                                    <div className="selected-address">
                                        <strong>Selected:</strong> {selectedAddress.address}, {selectedAddress.city}, {selectedAddress.country}
                                        <button
                                            type="button"
                                            onClick={() => setSelectedAddress(null)}
                                            className="btn-clear"
                                        >
                                            Clear
                                        </button>
                                    </div>
                                )}

                                <div className="form-buttons">
                                    <button type="button" className="btn btn-gray" onClick={resetForm}>
                                        Cancel
                                    </button>
                                    <button
                                        type="submit"
                                        className="btn"
                                        disabled={createLocation.isPending || !selectedAddress}
                                    >
                                        {createLocation.isPending ? 'Saving...' : 'Save'}
                                    </button>
                                </div>
                            </form>
                        </div>
                    </div>
                </div>
            )}
        </div>
    );
}

export default LocationList;