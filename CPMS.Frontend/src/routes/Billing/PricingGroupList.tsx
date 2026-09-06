import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { billingApi } from '../../api/services/billing';
import { chargePointsApi } from '../../api/services/chargePoints';
import type { CreatePricingGroupCommand } from '../../types/billing';
import Modal from '../../components/common/Modal';
import './PricingGroupList.css';

const emptyForm = { name: '', basePrice: '', pricePerKwh: '', currency: 'EUR' };

function PricingGroupList() {
    const [showCreateModal, setShowCreateModal] = useState(false);
    const [assignToGroupId, setAssignToGroupId] = useState<string | null>(null);
    const [form, setForm] = useState(emptyForm);
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
            closeCreateModal();
        },
        onError: () => setError('Failed to create pricing group'),
    });

    const assignChargePoint = useMutation({
        mutationFn: ({ groupId, chargePointId }: { groupId: string; chargePointId: string }) =>
            billingApi.assignChargePointToPricingGroup(groupId, chargePointId),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['pricingGroups'] });
            setAssignToGroupId(null);
        },
    });

    const closeCreateModal = () => {
        setShowCreateModal(false);
        setForm(emptyForm);
        setError('');
    };

    const handleSubmit = (e: React.FormEvent) => {
        e.preventDefault();

        if (!form.name || !form.basePrice || !form.pricePerKwh) {
            setError('All fields are required');
            return;
        }

        createGroup.mutate({
            name: form.name,
            basePrice: parseFloat(form.basePrice),
            pricePerKwh: parseFloat(form.pricePerKwh),
            currency: form.currency,
        });
    };

    const assignedIds = new Set(pricingGroups.flatMap(g => g.chargePointIds));
    const unassignedChargePoints = chargePoints.filter(cp => !assignedIds.has(cp.id));

    if (isLoading) return <div className="loading">Loading pricing groups...</div>;

    return (
        <div className="pricing-groups">
            <div className="flex-between">
                <h1>Pricing Groups</h1>
                <button className="btn" onClick={() => setShowCreateModal(true)}>Add Pricing Group</button>
            </div>

            <div className="pricing-groups-grid">
                {pricingGroups.map(group => {
                    const assigned = chargePoints.filter(cp => group.chargePointIds.includes(cp.id));
                    return (
                        <div key={group.id} className="pricing-group-card">
                            <h3>{group.name}</h3>
                            <div className="pricing-details">
                                <div>Base: {group.basePrice} {group.currency}</div>
                                <div>Per kWh: {group.pricePerKwh} {group.currency}</div>
                            </div>
                            <div className="charge-points">
                                <strong>ChargePoints ({assigned.length})</strong>
                                {assigned.map(cp => (
                                    <div key={cp.id} className="charge-point-item">{cp.ocppChargerId}</div>
                                ))}
                            </div>
                            <button className="btn btn-gray" onClick={() => setAssignToGroupId(group.id)}>
                                Assign ChargePoint
                            </button>
                        </div>
                    );
                })}
            </div>

            {showCreateModal && (
                <Modal title="Create Pricing Group" onClose={closeCreateModal} error={error}>
                    <form onSubmit={handleSubmit}>
                        <div className="form-group">
                            <label>Group Name</label>
                            <input
                                type="text"
                                value={form.name}
                                onChange={(e) => setForm({ ...form, name: e.target.value })}
                                required
                            />
                        </div>
                        <div className="form-group">
                            <label>Base Price (session start fee)</label>
                            <input
                                type="number"
                                step="0.01"
                                value={form.basePrice}
                                onChange={(e) => setForm({ ...form, basePrice: e.target.value })}
                                required
                            />
                        </div>
                        <div className="form-group">
                            <label>Price per kWh</label>
                            <input
                                type="number"
                                step="0.01"
                                value={form.pricePerKwh}
                                onChange={(e) => setForm({ ...form, pricePerKwh: e.target.value })}
                                required
                            />
                        </div>
                        <div className="form-group">
                            <label>Currency</label>
                            <select value={form.currency} onChange={(e) => setForm({ ...form, currency: e.target.value })}>
                                <option value="EUR">EUR</option>
                                <option value="USD">USD</option>
                                <option value="GBP">GBP</option>
                            </select>
                        </div>
                        <div className="form-buttons">
                            <button type="button" className="btn btn-gray" onClick={closeCreateModal}>Cancel</button>
                            <button type="submit" className="btn" disabled={createGroup.isPending}>
                                {createGroup.isPending ? 'Creating...' : 'Create'}
                            </button>
                        </div>
                    </form>
                </Modal>
            )}

            {assignToGroupId && (
                <Modal title="Assign ChargePoint" onClose={() => setAssignToGroupId(null)}>
                    <div className="unassigned-charge-points">
                        {unassignedChargePoints.length === 0 ? (
                            <p>No unassigned ChargePoints available</p>
                        ) : (
                            unassignedChargePoints.map(cp => (
                                <div key={cp.id} className="charge-point-option">
                                    <span>{cp.ocppChargerId}</span>
                                    <button
                                        className="btn btn-sm"
                                        onClick={() => assignChargePoint.mutate({ groupId: assignToGroupId, chargePointId: cp.id })}
                                        disabled={assignChargePoint.isPending}
                                    >
                                        Assign
                                    </button>
                                </div>
                            ))
                        )}
                    </div>
                </Modal>
            )}
        </div>
    );
}

export default PricingGroupList;
