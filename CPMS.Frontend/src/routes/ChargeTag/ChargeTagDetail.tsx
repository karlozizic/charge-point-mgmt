import { useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { chargeTagsApi } from '../../api/services/chargeTags';
import Modal from '../../components/common/Modal';

function ChargeTagDetail() {
    const { id } = useParams<{ id: string }>();
    const navigate = useNavigate();
    const queryClient = useQueryClient();

    const [showEditModal, setShowEditModal] = useState(false);
    const [showExpiryModal, setShowExpiryModal] = useState(false);
    const [tagId, setTagId] = useState('');
    const [expiryDate, setExpiryDate] = useState('');
    const [errorMsg, setErrorMsg] = useState('');

    const { data: tag, isLoading } = useQuery({
        queryKey: ['chargeTag', id],
        queryFn: () => chargeTagsApi.getById(id!),
        enabled: !!id,
    });

    const invalidate = () => {
        queryClient.invalidateQueries({ queryKey: ['chargeTag', id] });
        queryClient.invalidateQueries({ queryKey: ['chargeTags'] });
    };

    const updateTag = useMutation({
        mutationFn: (newTagId: string) =>
            chargeTagsApi.update(id!, { id: id!, tagId: newTagId, expiryDate: tag?.expiryDate || null }),
        onSuccess: () => {
            invalidate();
            setShowEditModal(false);
        },
        onError: (err: Error) => setErrorMsg(`Failed to update tag: ${err.message || 'Unknown error'}`),
    });

    const updateExpiry = useMutation({
        mutationFn: (newExpiry: string | null) => chargeTagsApi.updateExpiry(id!, { id: id!, expiryDate: newExpiry }),
        onSuccess: () => {
            invalidate();
            setShowExpiryModal(false);
        },
        onError: (err: Error) => setErrorMsg(`Failed to update expiry date: ${err.message || 'Unknown error'}`),
    });

    const blockTag = useMutation({ mutationFn: chargeTagsApi.block, onSuccess: invalidate });
    const unblockTag = useMutation({ mutationFn: chargeTagsApi.unblock, onSuccess: invalidate });

    function handleUpdateTag(e: React.FormEvent) {
        e.preventDefault();
        if (!tagId) {
            setErrorMsg('Tag ID is required');
            return;
        }
        updateTag.mutate(tagId);
    }

    function handleUpdateExpiry(e: React.FormEvent) {
        e.preventDefault();
        updateExpiry.mutate(expiryDate || null);
    }

    function openEditModal() {
        setTagId(tag?.tagId || '');
        setErrorMsg('');
        setShowEditModal(true);
    }

    function openExpiryModal() {
        setExpiryDate(tag?.expiryDate ? new Date(tag.expiryDate).toISOString().split('T')[0] : '');
        setErrorMsg('');
        setShowExpiryModal(true);
    }

    if (isLoading) return <div className="loading">Loading...</div>;
    if (!tag) return <div className="empty">Tag not found</div>;

    return (
        <div className="charge-tag-detail">
            <button className="btn btn-gray back" onClick={() => navigate(-1)}>Back to List</button>

            <h1>Tag Details</h1>

            <div className="card">
                <div className="card-header">
                    <h2>{tag.tagId}</h2>
                </div>
                <div className="card-body">
                    <div className="info-grid">
                        <div className="info-item"><span className="label">ID:</span><span className="value">{tag.id}</span></div>
                        <div className="info-item"><span className="label">Tag ID:</span><span className="value">{tag.tagId}</span></div>
                        <div className="info-item">
                            <span className="label">Expiry Date:</span>
                            <span className="value">{tag.expiryDate ? new Date(tag.expiryDate).toLocaleDateString() : 'No expiry date'}</span>
                        </div>
                        <div className="info-item">
                            <span className="label">Status:</span>
                            <span className="value" style={{ color: tag.blocked ? 'red' : 'green', fontWeight: 'bold' }}>
                                {tag.blocked ? 'Blocked' : 'Active'}
                            </span>
                        </div>
                    </div>

                    <div className="actions">
                        <button className="btn" onClick={openEditModal}>Edit Tag ID</button>
                        <button className="btn btn-gray" onClick={openExpiryModal}>Update Expiry Date</button>
                        <button
                            className={tag.blocked ? 'btn' : 'btn btn-red'}
                            onClick={() => (tag.blocked ? unblockTag : blockTag).mutate(tag.id)}
                            disabled={blockTag.isPending || unblockTag.isPending}
                        >
                            {tag.blocked ? 'Unblock Tag' : 'Block Tag'}
                        </button>
                    </div>
                </div>
            </div>

            {showEditModal && (
                <Modal title="Edit Tag ID" onClose={() => setShowEditModal(false)} error={errorMsg}>
                    <form onSubmit={handleUpdateTag}>
                        <div className="form-group">
                            <label htmlFor="tagId">Tag ID</label>
                            <input type="text" id="tagId" value={tagId} onChange={(e) => setTagId(e.target.value)} required />
                        </div>
                        <div className="form-buttons">
                            <button type="button" className="btn btn-gray" onClick={() => setShowEditModal(false)}>Cancel</button>
                            <button type="submit" className="btn" disabled={updateTag.isPending}>
                                {updateTag.isPending ? 'Saving...' : 'Save'}
                            </button>
                        </div>
                    </form>
                </Modal>
            )}

            {showExpiryModal && (
                <Modal title="Update Expiry Date" onClose={() => setShowExpiryModal(false)} error={errorMsg}>
                    <form onSubmit={handleUpdateExpiry}>
                        <div className="form-group">
                            <label htmlFor="expiryDate">Expiry Date</label>
                            <input type="date" id="expiryDate" value={expiryDate} onChange={(e) => setExpiryDate(e.target.value)} />
                            <p className="helper-text">Leave empty to remove expiry date</p>
                        </div>
                        <div className="form-buttons">
                            <button type="button" className="btn btn-gray" onClick={() => setShowExpiryModal(false)}>Cancel</button>
                            <button type="submit" className="btn" disabled={updateExpiry.isPending}>
                                {updateExpiry.isPending ? 'Saving...' : 'Save'}
                            </button>
                        </div>
                    </form>
                </Modal>
            )}
        </div>
    );
}

export default ChargeTagDetail;
