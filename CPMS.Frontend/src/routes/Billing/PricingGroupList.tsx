import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { billingApi } from '../../api/services/billing';
import { chargePointsApi } from '../../api/services/chargePoints';
import type { CreatePricingGroupCommand } from '../../types/billing';
import './PricingGroupList.css';

function PricingGroupList() {
    const [showModal, setShowModal] = useState(false);
    const [showAssignModal, setShowAssignModal] = useState(false);
    const [selectedGroupId, setSelectedGroupId] = useState<string>('');
    const [formData, setFormData] = useState({
        name: '',
        basePrice: '',
        pricePerKwh: '',
        currency: 'EUR'
    });
    const [error, setError] = useState('');

    const queryClient = useQueryClient();

    const { data: pricingGroups = [], isLoading } = useQuery({
        queryKey: ['pricingGroups'],
        queryFn: billingApi.getPricingGroups,
    });

    const { data: chargePoints = [] } = useQuery({
        queryKey: ['chargePoints'],
        queryFn: chargePointsApi.getAll,
    });

    const createGroup = useMutation({
        mutationFn: (command: CreatePricingGroupCommand) => billingApi.createPricingGroup(command),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['pricingGroups'] });
            setShowModal(false);
            setFormData({ name: '', basePrice: '', pricePerKwh: '', currency: 'EUR' });
            setError('');
        },
        onError: () => {
            setError('Failed to create pricing group');
        }
    });

    const assignChargePoint = useMutation({
        mutationFn: ({ groupId, chargePointId }: { groupId: string, chargePointId: string }) =>
            billingApi.assignChargePointToPricingGroup(groupId, chargePointId),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['pricingGroups'] });
            setShowAssignModal(false);
        }
    });

    const handleSubmit = (e: React.FormEvent) => {
        e.preventDefault();

        if (!formData.name || !formData.basePrice || !formData.pricePerKwh) {
            setError('All fields are required');
            return;
        }

        createGroup.mutate({
            name: formData.name,
            basePrice: parseFloat(formData.basePrice),
            pricePerKwh: parseFloat(formData.pricePerKwh),
            currency: formData.currency
        });
    };

    const getAssignedChargePoints = (groupId: string) => {
        const group = pricingGroups.find(g => g.id === groupId);
        return chargePoints.filter(cp => group?.chargePointIds.includes(cp.id));
    };

    const getUnassignedChargePoints = () => {
        const assignedIds = pricingGroups.flatMap(g => g.chargePointIds);
        return chargePoints.filter(cp => !assignedIds.includes(cp.id));
    };

    if (isLoading) return <div className="loading">Loading pricing groups...</div>;

    return (
        <div className="pricing-groups">
            <div className="flex-between">
                <h1>Pricing Groups</h1>
                <button className="btn" onClick={() => setShowModal(true)}>
                    Add Pricing Group
                </button>
            </div>

            <div className="pricing-groups-grid">
                {pricingGroups.map(group => (
                    <div key={group.id} className="pricing-group-card">
                        <h3>{group.name}</h3>
                        <div className="pricing-details">
                            <div>Base: {group.basePrice} {group.currency}</div>
                            <div>Per kWh: {group.pricePerKwh} {group.currency}</div>
                        </div>
                        <div className="charge-points">
                            <strong>ChargePoints ({getAssignedChargePoints(group.id).length})</strong>
                            {getAssignedChargePoints(group.id).map(cp => (
                                <div key={cp.id} className="charge-point-item">
                                    {cp.ocppChargerId}
                                </div>
                            ))}
                        </div>
                        <button
                            className="btn btn-gray"
                            onClick={() => {
                                setSelectedGroupId(group.id);
                                setShowAssignModal(true);
                            }}
                        >
                            Assign ChargePoint
                        </button>
                    </div>
                ))}
            </div>

            {/* Create Pricing Group Modal */}
            {showModal && (
                <div className="overlay">
                    <div className="modal">
                        <div className="modal-header">
                            <h2>Create Pricing Group</h2>
                            <button className="close" onClick={() => setShowModal(false)}>&times;</button>
                        </div>
                        <div className="modal-body">
                            {error && <div className="error-msg">{error}</div>}
                            <form onSubmit={handleSubmit}>
                                <div className="form-group">
                                    <label>Group Name</label>
                                    <input
                                        type="text"
                                        value={formData.name}
                                        onChange={(e) => setFormData({...formData, name: e.target.value})}
                                        required
                                    />
                                </div>
                                <div className="form-group">
                                    <label>Base Price (session start fee)</label>
                                    <input
                                        type="number"
                                        step="0.01"
                                        value={formData.basePrice}
                                        onChange={(e) => setFormData({...formData, basePrice: e.target.value})}
                                        required
                                    />
                                </div>
                                <div className="form-group">
                                    <label>Price per kWh</label>
                                    <input
                                        type="number"
                                        step="0.01"
                                        value={formData.pricePerKwh}
                                        onChange={(e) => setFormData({...formData, pricePerKwh: e.target.value})}
                                        required
                                    />
                                </div>
                                <div className="form-group">
                                    <label>Currency</label>
                                    <select
                                        value={formData.currency}
                                        onChange={(e) => setFormData({...formData, currency: e.target.value})}
                                    >
                                        <option value="EUR">EUR</option>
                                        <option value="USD">USD</option>
                                        <option value="GBP">GBP</option>
                                    </select>
                                </div>
                                <div className="form-buttons">
                                    <button type="button" className="btn btn-gray" onClick={() => setShowModal(false)}>
                                        Cancel
                                    </button>
                                    <button type="submit" className="btn" disabled={createGroup.isPending}>
                                        {createGroup.isPending ? 'Creating...' : 'Create'}
                                    </button>
                                </div>
                            </form>
                        </div>
                    </div>
                </div>
            )}

            {/* Assign ChargePoint Modal */}
            {showAssignModal && (
                <div className="overlay">
                    <div className="modal">
                        <div className="modal-header">
                            <h2>Assign ChargePoint</h2>
                            <button className="close" onClick={() => setShowAssignModal(false)}>&times;</button>
                        </div>
                        <div className="modal-body">
                            <div className="unassigned-charge-points">
                                {getUnassignedChargePoints().length === 0 ? (
                                    <p>No unassigned ChargePoints available</p>
                                ) : (
                                    getUnassignedChargePoints().map(cp => (
                                        <div key={cp.id} className="charge-point-option">
                                            <span>{cp.ocppChargerId}</span>
                                            <button
                                                className="btn btn-sm"
                                                onClick={() => assignChargePoint.mutate({
                                                    groupId: selectedGroupId,
                                                    chargePointId: cp.id
                                                })}
                                                disabled={assignChargePoint.isPending}
                                            >
                                                Assign
                                            </button>
                                        </div>
                                    ))
                                )}
                            </div>
                        </div>
                    </div>
                </div>
            )}
        </div>
    );
}

export default PricingGroupList;