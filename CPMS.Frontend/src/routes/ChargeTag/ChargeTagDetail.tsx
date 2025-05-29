import { useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { chargeTagsApi } from '../../api/services/chargeTags.ts';
import type { UpdateChargeTagCommand, UpdateChargeTagExpiryCommand } from '../../types/chargeTag.ts';

function ChargeTagDetail() {
    const { id } = useParams<{ id: string }>();
    const nav = useNavigate();
    const qc = useQueryClient();

    const [showEditModal, setShowEditModal] = useState(false);
    const [showExpiryModal, setShowExpiryModal] = useState(false);

    const [tagId, setTagId] = useState('');
    const [expiryDate, setExpiryDate] = useState<string | null>(null);
    const [errorMsg, setErrorMsg] = useState('');

    const { data: tag, isLoading } = useQuery({
        queryKey: ['chargeTag', id],
        queryFn: () => id ? chargeTagsApi.getById(id) : Promise.reject('No ID provided'),
        enabled: !!id
    });

    const updateTag = useMutation({
        mutationFn: (command: UpdateChargeTagCommand) =>
            chargeTagsApi.update(id || '', command),
        onSuccess: () => {
            qc.invalidateQueries({ queryKey: ['chargeTag', id] });
            qc.invalidateQueries({ queryKey: ['chargeTags'] });
            setShowEditModal(false);
        },
        onError: (err: any) => {
            console.error('Error updating tag:', err);
            setErrorMsg(`Failed to update tag: ${err.message || 'Unknown error'}`);
        }
    });

    const updateExpiry = useMutation({
        mutationFn: (command: UpdateChargeTagExpiryCommand) =>
            chargeTagsApi.updateExpiry(id || '', command),
        onSuccess: () => {
            qc.invalidateQueries({ queryKey: ['chargeTag', id] });
            qc.invalidateQueries({ queryKey: ['chargeTags'] });
            setShowExpiryModal(false);
        },
        onError: (err: any) => {
            console.error('Error updating expiry date:', err);
            setErrorMsg(`Failed to update expiry date: ${err.message || 'Unknown error'}`);
        }
    });

    const blockTag = useMutation({
        mutationFn: (id: string) => chargeTagsApi.block(id),
        onSuccess: () => {
            qc.invalidateQueries({ queryKey: ['chargeTag', id] });
            qc.invalidateQueries({ queryKey: ['chargeTags'] });
        }
    });

    const unblockTag = useMutation({
        mutationFn: (id: string) => chargeTagsApi.unblock(id),
        onSuccess: () => {
            qc.invalidateQueries({ queryKey: ['chargeTag', id] });
            qc.invalidateQueries({ queryKey: ['chargeTags'] });
        }
    });

    function handleUpdateTag(e: React.FormEvent) {
        e.preventDefault();

        if (!tagId) {
            setErrorMsg('Tag ID is required');
            return;
        }

        if (id) {
            updateTag.mutate({
                id,
                tagId,
                expiryDate: tag?.expiryDate || null
            });
        }
    }

    function handleUpdateExpiry(e: React.FormEvent) {
        e.preventDefault();

        if (id) {
            updateExpiry.mutate({
                id,
                expiryDate: expiryDate
            });
        }
    }

    function handleToggleBlock() {
        if (id && tag) {
            if (tag.blocked) {
                unblockTag.mutate(id);
            } else {
                blockTag.mutate(id);
            }
        }
    }

    function showEditTagModal() {
        setTagId(tag?.tagId || '');
        setShowEditModal(true);
    }

    function showExpiryDateModal() {
        if (tag?.expiryDate) {
            const date = new Date(tag.expiryDate);
            const formattedDate = date.toISOString().split('T')[0]; // YYYY-MM-DD format
            setExpiryDate(formattedDate);
        } else {
            setExpiryDate(null);
        }
        setShowExpiryModal(true);
    }

    if (isLoading) return <div className="loading">Loading...</div>;
    if (!tag) return <div className="empty">Charge tag not found</div>;

    return (
        <div className="charge-tag-detail">
            <button className="btn btn-gray back" onClick={() => nav(-1)}>
                Back to List
            </button>

            <h1>Charge Tag Details</h1>

            <div className="card">
                <div className="card-header">
                    <h2>{tag.tagId}</h2>
                </div>

                <div className="card-body">
                    <div className="info-grid">
                        <div className="info-item">
                            <span className="label">ID:</span>
                            <span className="value">{tag.id}</span>
                        </div>
                        <div className="info-item">
                            <span className="label">Tag ID:</span>
                            <span className="value">{tag.tagId}</span>
                        </div>
                        <div className="info-item">
                            <span className="label">Expiry Date:</span>
                            <span className="value">
                {tag.expiryDate
                    ? new Date(tag.expiryDate).toLocaleDateString()
                    : 'No expiry date'}
              </span>
                        </div>
                        <div className="info-item">
                            <span className="label">Status:</span>
                            <span className="value" style={{ color: tag.blocked ? 'red' : 'green', fontWeight: 'bold' }}>
                {tag.blocked ? 'Blocked' : 'Active'}
              </span>
                        </div>
                    </div>

                    <div className="actions">
                        <button className="btn" onClick={showEditTagModal}>
                            Edit Tag ID
                        </button>
                        <button className="btn btn-gray" onClick={showExpiryDateModal}>
                            Update Expiry Date
                        </button>
                        <button
                            className={tag.blocked ? "btn" : "btn btn-red"}
                            onClick={handleToggleBlock}
                            disabled={blockTag.isPending || unblockTag.isPending}
                        >
                            {tag.blocked ? 'Unblock Tag' : 'Block Tag'}
                        </button>
                    </div>
                </div>
            </div>

            {/* Edit Tag ID Modal */}
            {showEditModal && (
                <div className="overlay">
                    <div className="modal">
                        <div className="modal-header">
                            <h2>Edit Tag ID</h2>
                            <button className="close" onClick={() => setShowEditModal(false)}>&times;</button>
                        </div>
                        <div className="modal-body">
                            {errorMsg && <div className="error-msg">{errorMsg}</div>}
                            <form onSubmit={handleUpdateTag}>
                                <div className="form-group">
                                    <label htmlFor="tagId">Tag ID</label>
                                    <input
                                        type="text"
                                        id="tagId"
                                        value={tagId}
                                        onChange={(e) => setTagId(e.target.value)}
                                        required
                                    />
                                </div>
                                <div className="form-buttons">
                                    <button
                                        type="button"
                                        className="btn btn-gray"
                                        onClick={() => setShowEditModal(false)}
                                    >
                                        Cancel
                                    </button>
                                    <button
                                        type="submit"
                                        className="btn"
                                        disabled={updateTag.isPending}
                                    >
                                        {updateTag.isPending ? 'Saving...' : 'Save'}
                                    </button>
                                </div>
                            </form>
                        </div>
                    </div>
                </div>
            )}

            {/* Update Expiry Date Modal */}
            {showExpiryModal && (
                <div className="overlay">
                    <div className="modal">
                        <div className="modal-header">
                            <h2>Update Expiry Date</h2>
                            <button className="close" onClick={() => setShowExpiryModal(false)}>&times;</button>
                        </div>
                        <div className="modal-body">
                            {errorMsg && <div className="error-msg">{errorMsg}</div>}
                            <form onSubmit={handleUpdateExpiry}>
                                <div className="form-group">
                                    <label htmlFor="expiryDate">Expiry Date</label>
                                    <input
                                        type="date"
                                        id="expiryDate"
                                        value={expiryDate || ''}
                                        onChange={(e) => setExpiryDate(e.target.value || null)}
                                    />
                                    <p className="helper-text">Leave empty to remove expiry date</p>
                                </div>
                                <div className="form-buttons">
                                    <button
                                        type="button"
                                        className="btn btn-gray"
                                        onClick={() => setShowExpiryModal(false)}
                                    >
                                        Cancel
                                    </button>
                                    <button
                                        type="submit"
                                        className="btn"
                                        disabled={updateExpiry.isPending}
                                    >
                                        {updateExpiry.isPending ? 'Saving...' : 'Save'}
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

export default ChargeTagDetail;