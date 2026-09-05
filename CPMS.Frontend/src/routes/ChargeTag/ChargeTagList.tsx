import { useState } from 'react';
import { Link } from 'react-router-dom';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { chargeTagsApi } from '../../api/services/chargeTags';
import type { CreateChargeTagCommand } from '../../types/chargeTag';
import Modal from '../../components/common/Modal';
import { shortId } from '../../utils/format';

function ChargeTagList() {
    const [showModal, setShowModal] = useState(false);
    const [tagId, setTagId] = useState('');
    const [expiryDate, setExpiryDate] = useState('');
    const [errorMsg, setErrorMsg] = useState('');

    const queryClient = useQueryClient();
    const invalidate = () => queryClient.invalidateQueries({ queryKey: ['chargeTags'] });

    const { data: tags = [], isLoading } = useQuery({
        queryKey: ['chargeTags'],
        queryFn: chargeTagsApi.getAll,
    });

    const createTag = useMutation({
        mutationFn: (command: CreateChargeTagCommand) => chargeTagsApi.create(command),
        onSuccess: () => {
            invalidate();
            closeModal();
        },
        onError: (err: Error) => setErrorMsg(`Failed to create tag: ${err.message || 'Unknown error'}`),
    });

    const blockTag = useMutation({ mutationFn: chargeTagsApi.block, onSuccess: invalidate });
    const unblockTag = useMutation({ mutationFn: chargeTagsApi.unblock, onSuccess: invalidate });

    const closeModal = () => {
        setShowModal(false);
        setTagId('');
        setExpiryDate('');
        setErrorMsg('');
    };

    function handleSubmit(e: React.FormEvent) {
        e.preventDefault();

        if (!tagId) {
            setErrorMsg('Tag ID is required');
            return;
        }

        createTag.mutate({
            tagId,
            expiryDate: expiryDate ? new Date(expiryDate).toISOString() : null,
        });
    }

    if (isLoading) return <div className="loading">Loading...</div>;

    return (
        <div className="charge-tags-container">
            <div className="flex-between">
                <h1>Charging Tags</h1>
                <button className="btn" onClick={() => setShowModal(true)}>Add New Tag</button>
            </div>

            {tags.length === 0 ? (
                <div className="empty">
                    <p>No Tags found. Add a new tag to get started.</p>
                </div>
            ) : (
                <table>
                    <thead>
                    <tr>
                        <th>ID</th>
                        <th>Tag ID</th>
                        <th>Expiry Date</th>
                        <th>Status</th>
                        <th>Actions</th>
                    </tr>
                    </thead>
                    <tbody>
                    {tags.map(tag => (
                        <tr key={tag.id}>
                            <td>{shortId(tag.id)}</td>
                            <td>{tag.tagId}</td>
                            <td>{tag.expiryDate ? new Date(tag.expiryDate).toLocaleDateString() : 'No expiry date'}</td>
                            <td>
                                <span style={{ color: tag.blocked ? 'red' : 'green' }}>
                                    {tag.blocked ? 'Blocked' : 'Active'}
                                </span>
                            </td>
                            <td>
                                <div className="action-buttons">
                                    <Link to={`/charge-tags/${tag.id}`} className="btn btn-gray">Details</Link>
                                    <button
                                        className={tag.blocked ? 'btn' : 'btn btn-red'}
                                        onClick={() => (tag.blocked ? unblockTag : blockTag).mutate(tag.id)}
                                        disabled={blockTag.isPending || unblockTag.isPending}
                                    >
                                        {tag.blocked ? 'Unblock' : 'Block'}
                                    </button>
                                </div>
                            </td>
                        </tr>
                    ))}
                    </tbody>
                </table>
            )}

            {showModal && (
                <Modal title="Add New Charging Tag" onClose={closeModal} error={errorMsg}>
                    <form onSubmit={handleSubmit}>
                        <div className="form-group">
                            <label htmlFor="tagId">Tag ID</label>
                            <input type="text" id="tagId" value={tagId} onChange={(e) => setTagId(e.target.value)} required />
                        </div>
                        <div className="form-group">
                            <label htmlFor="expiryDate">Expiry Date</label>
                            <input type="date" id="expiryDate" value={expiryDate} onChange={(e) => setExpiryDate(e.target.value)} />
                        </div>
                        <div className="form-buttons">
                            <button type="button" className="btn btn-gray" onClick={closeModal}>Cancel</button>
                            <button type="submit" className="btn" disabled={createTag.isPending}>
                                {createTag.isPending ? 'Saving...' : 'Save'}
                            </button>
                        </div>
                    </form>
                </Modal>
            )}
        </div>
    );
}

export default ChargeTagList;
