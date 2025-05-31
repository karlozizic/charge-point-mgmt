import { useState } from 'react';
import { Link } from 'react-router-dom';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { chargeTagsApi } from '../../api/services/chargeTags.ts';
import type {CreateChargeTagCommand} from '../../types/chargeTag.ts';

function ChargeTagList() {
    const [showModal, setShowModal] = useState(false);
    const [formData, setFormData] = useState({
        tagId: '',
        expiryDate: null as Date | null
    });
    const [errorMsg, setErrorMsg] = useState('');

    const queryClient = useQueryClient();

    const { data: tags, isLoading } = useQuery({
        queryKey: ['chargeTags'],
        queryFn: () => chargeTagsApi.getAll(),
    });

    const createTag = useMutation({
        mutationFn: (command: CreateChargeTagCommand) => chargeTagsApi.create(command),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['chargeTags'] });
            setShowModal(false);
            setFormData({ tagId: '', expiryDate: null });
        },
        onError: (err: any) => {
            console.error('Error creating tag:', err);
            setErrorMsg(`Failed to create tag: ${err.message || 'Unknown error'}`);
        }
    });

    const blockTag = useMutation({
        mutationFn: (id: string) => chargeTagsApi.block(id),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['chargeTags'] });
        }
    });

    const unblockTag = useMutation({
        mutationFn: (id: string) => chargeTagsApi.unblock(id),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['chargeTags'] });
        }
    });

    function handleTagIdChange(e: React.ChangeEvent<HTMLInputElement>) {
        setFormData({
            ...formData,
            tagId: e.target.value
        });
    }

    function handleDateChange(e: React.ChangeEvent<HTMLInputElement>) {
        setFormData({
            ...formData,
            expiryDate: e.target.value ? new Date(e.target.value) : null
        });
    }

    function handleSubmit(e: React.FormEvent) {
        e.preventDefault();

        if (!formData.tagId) {
            setErrorMsg('Tag ID is required');
            return;
        }

        createTag.mutate({
            tagId: formData.tagId,
            expiryDate: formData.expiryDate ? formData.expiryDate.toISOString() : null
        });
    }

    function handleToggleBlock(id: string, isBlocked: boolean) {
        if (isBlocked) {
            unblockTag.mutate(id);
        } else {
            blockTag.mutate(id);
        }
    }

    if (isLoading) return <div className="loading">Loading...</div>;

    return (
        <div className="charge-tags-container">
            <div className="flex-between">
                <h1>Charging Tags</h1>
                <button className="btn" onClick={() => setShowModal(true)}>
                    Add New Tag
                </button>
            </div>

            {!tags || tags.length === 0 ? (
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
                            <td>{tag.id.substring(0, 8)}...</td>
                            <td>{tag.tagId}</td>
                            <td>
                                {tag.expiryDate
                                    ? new Date(tag.expiryDate).toLocaleDateString()
                                    : 'No expiry date'}
                            </td>
                            <td>
                  <span style={{ color: tag.blocked ? 'red' : 'green' }}>
                    {tag.blocked ? 'Blocked' : 'Active'}
                  </span>
                            </td>
                            <td>
                                <div className="action-buttons">
                                    <Link to={`/charge-tags/${tag.id}`} className="btn btn-gray">
                                        Details
                                    </Link>
                                    <button
                                        className={tag.blocked ? "btn" : "btn btn-red"}
                                        onClick={() => handleToggleBlock(tag.id, tag.blocked)}
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

            {/* Modal */}
            {showModal && (
                <div className="overlay">
                    <div className="modal">
                        <div className="modal-header">
                            <h2>Add New Charging Tag</h2>
                            <button className="close" onClick={() => setShowModal(false)}>&times;</button>
                        </div>
                        <div className="modal-body">
                            {errorMsg && <div className="error-msg">{errorMsg}</div>}
                            <form onSubmit={handleSubmit}>
                                <div className="form-group">
                                    <label htmlFor="tagId">Tag ID</label>
                                    <input
                                        type="text"
                                        id="tagId"
                                        value={formData.tagId}
                                        onChange={handleTagIdChange}
                                        required
                                    />
                                </div>
                                <div className="form-group">
                                    <label htmlFor="expiryDate">Expiry Date</label>
                                    <input
                                        type="date"
                                        id="expiryDate"
                                        onChange={handleDateChange}
                                    />
                                </div>
                                <div className="form-buttons">
                                    <button
                                        type="button"
                                        className="btn btn-gray"
                                        onClick={() => setShowModal(false)}
                                    >
                                        Cancel
                                    </button>
                                    <button
                                        type="submit"
                                        className="btn"
                                        disabled={createTag.isPending}
                                    >
                                        {createTag.isPending ? 'Saving...' : 'Save'}
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

export default ChargeTagList;