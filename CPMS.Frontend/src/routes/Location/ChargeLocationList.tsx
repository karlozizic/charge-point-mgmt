import {useMutation, useQuery, useQueryClient,} from "@tanstack/react-query";
import {chargeLocationsApi} from "../../api/services/chargeLocations.ts";
import {Link} from "react-router-dom";
import {useState} from "react";

function LocationList() {
    const [showModal, setShowModal] = useState(false);
    const [form, setForm] = useState({
        name: '',
        address: '',
        city: '',
        postalCode: '',
        country: 'Croatia'
    });
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
            setShowModal(false);
            setForm({ name: '', address: '', city: '', postalCode: '', country: 'Croatia' });
            setError('');
        },
        onError: () => {
            setError('Failed to create location');
        }
    });

    const handleSubmit = (e: React.FormEvent) => {
        e.preventDefault();
        setError('');

        if (!form.name.trim() || !form.address.trim() || !form.city.trim()) {
            setError('Name, address and city are required');
            return;
        }

        createLocation.mutate(form);
    };

    if (isLoading) {
        return <div className="loading">Loading locations...</div>;
    }

    return (
        <div className="locations-container">
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
                            <button className="close" onClick={() => setShowModal(false)}>&times;</button>
                        </div>
                        <div className="modal-body">
                            {error && <div className="error-msg">{error}</div>}
                            <form onSubmit={handleSubmit}>
                                <div className="form-group">
                                    <label htmlFor="name">Location Name</label>
                                    <input
                                        type="text"
                                        id="name"
                                        value={form.name}
                                        onChange={e => setForm({...form, name: e.target.value})}
                                        placeholder="e.g. Main Street Station"
                                        required
                                    />
                                </div>
                                <div className="form-group">
                                    <label htmlFor="address">Address</label>
                                    <input
                                        type="text"
                                        id="address"
                                        value={form.address}
                                        onChange={e => setForm({...form, address: e.target.value})}
                                        placeholder="e.g. Ilica 10"
                                        required
                                    />
                                </div>
                                <div className="form-group">
                                    <label htmlFor="city">City</label>
                                    <input
                                        type="text"
                                        id="city"
                                        value={form.city}
                                        onChange={e => setForm({...form, city: e.target.value})}
                                        placeholder="Zagreb"
                                        required
                                    />
                                </div>
                                <div className="form-group">
                                    <label htmlFor="postalCode">Postal Code</label>
                                    <input
                                        type="text"
                                        id="postalCode"
                                        value={form.postalCode}
                                        onChange={e => setForm({...form, postalCode: e.target.value})}
                                        placeholder="10000"
                                    />
                                </div>
                                <div className="form-group">
                                    <label htmlFor="country">Country</label>
                                    <select
                                        id="country"
                                        value={form.country}
                                        onChange={e => setForm({...form, country: e.target.value})}
                                    >
                                        <option value="Croatia">Croatia</option>
                                        <option value="Slovenia">Slovenia</option>
                                        <option value="Austria">Austria</option>
                                        <option value="Germany">Germany</option>
                                    </select>
                                </div>
                                <div className="form-buttons">
                                    <button type="button" className="btn btn-gray" onClick={() => setShowModal(false)}>
                                        Cancel
                                    </button>
                                    <button type="submit" className="btn" disabled={createLocation.isPending}>
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