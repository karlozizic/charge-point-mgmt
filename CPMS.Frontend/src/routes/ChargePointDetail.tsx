import { useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { chargePointsApi } from '../api/services/chargePoints';
import './ChargePointDetail.css';

const ChargePointDetail = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const queryClient = useQueryClient();

  const [showAddConnectorModal, setShowAddConnectorModal] = useState(false);
  const [showUpdateStatusModal, setShowUpdateStatusModal] = useState(false);
  const [selectedConnector, setSelectedConnector] = useState<number | null>(null);

  const [newConnector, setNewConnector] = useState({
    name: ''
  });

  const [connectorStatus, setConnectorStatus] = useState({
    status: 'Available',
    errorCode: '',
    info: ''
  });

  const [error, setError] = useState('');

  const { data: chargePoint, isLoading } = useQuery({
    queryKey: ['chargePoint', id],
    queryFn: () => chargePointsApi.getById(id || ''),
    enabled: !!id
  });

  const addConnectorMutation = useMutation({
    mutationFn: (data: { ocppChargerId: string, name: string }) =>
        chargePointsApi.addConnector(data.ocppChargerId, {
          ocppChargerId: data.ocppChargerId,
          name: data.name
        }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['chargePoint', id] });
      setShowAddConnectorModal(false);
      setNewConnector({ name: '' });
    },
    onError: (err) => {
      console.error('Error adding connector:', err);
      setError('Failed to add connector');
    }
  });

  const updateStatusMutation = useMutation({
    mutationFn: (data: {
      chargePointId: string,
      connectorId: number,
      status: string,
      errorCode: string,
      info: string
    }) => chargePointsApi.updateConnectorStatus(
        data.chargePointId,
        data.connectorId,
        {
          ocppChargerId: chargePoint?.ocppChargerId || '',
          chargePointId: data.chargePointId,
          connectorId: data.connectorId,
          status: data.status,
          errorCode: data.errorCode,
          info: data.info,
          timestamp: new Date().toISOString()
        }
    ),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['chargePoint', id] });
      setShowUpdateStatusModal(false);
      setSelectedConnector(null);
      setConnectorStatus({
        status: 'Available',
        errorCode: '',
        info: ''
      });
    },
    onError: (err) => {
      console.error('Error updating status:', err);
      setError('Failed to update connector status');
    }
  });

  const handleAddConnector = (e: React.FormEvent) => {
    e.preventDefault();

    if (!newConnector.name) {
      setError('Connector name is required');
      return;
    }

    if (chargePoint) {
      addConnectorMutation.mutate({
        ocppChargerId: chargePoint.ocppChargerId,
        name: newConnector.name
      });
    }
  };

  const handleUpdateStatus = (e: React.FormEvent) => {
    e.preventDefault();

    if (!id || selectedConnector === null) return;

    updateStatusMutation.mutate({
      chargePointId: id,
      connectorId: selectedConnector,
      status: connectorStatus.status,
      errorCode: connectorStatus.errorCode,
      info: connectorStatus.info
    });
  };

  const openUpdateStatusModal = (connectorId: number) => {
    const connector = chargePoint?.connectors?.find(c => c.id === connectorId);
    setSelectedConnector(connectorId);
    setConnectorStatus({
      status: connector?.status || 'Available',
      errorCode: '',
      info: ''
    });
    setShowUpdateStatusModal(true);
  };

  if (isLoading) return <div className="loading">Loading charge point details...</div>;
  if (!chargePoint) return <div className="empty">Charge point not found</div>;

  return (
      <div className="cp-detail">
        <button className="btn btn-gray back" onClick={() => navigate(-1)}>
          Back to List
        </button>

        <h1>Charge Point Details</h1>

        <div className="card">
          <div className="card-header">
            <h2>{chargePoint.ocppChargerId}</h2>
          </div>

          <div className="card-body">
            <div className="info-grid">
              <div className="info-item">
                <span className="label">ID:</span>
                <span className="value">{chargePoint.id}</span>
              </div>
              <div className="info-item">
                <span className="label">OCPP ID:</span>
                <span className="value">{chargePoint.ocppChargerId}</span>
              </div>
              <div className="info-item">
                <span className="label">Location:</span>
                <span className="value">{chargePoint.locationId}</span>
              </div>
              <div className="info-item">
                <span className="label">Max Power:</span>
                <span className="value">
                {chargePoint.maxPower ? `${chargePoint.maxPower} kW` : 'N/A'}
              </span>
              </div>
              <div className="info-item">
                <span className="label">Current Power:</span>
                <span className="value">
                {chargePoint.currentPower ? `${chargePoint.currentPower} kW` : 'N/A'}
              </span>
              </div>
            </div>
          </div>
        </div>

        <div className="connectors-section">
          <div className="section-header">
            <h2>Connectors</h2>
            <button className="btn" onClick={() => setShowAddConnectorModal(true)}>
              Add Connector
            </button>
          </div>

          {chargePoint.connectors?.length === 0 ? (
              <div className="empty">
                <p>No connectors available. Add a connector to get started.</p>
              </div>
          ) : (
              <table>
                <thead>
                <tr>
                  <th>ID</th>
                  <th>Name</th>
                  <th>Status</th>
                  <th>Actions</th>
                </tr>
                </thead>
                <tbody>
                {chargePoint.connectors?.map(connector => (
                    <tr key={connector.id}>
                      <td>{connector.id}</td>
                      <td>{connector.name}</td>
                      <td>
                      <span className={`status-badge ${connector.status?.toLowerCase() || 'available'}`}>
                        {connector.status || 'Available'}
                      </span>
                      </td>
                      <td>
                        <button
                            className="btn btn-gray"
                            onClick={() => openUpdateStatusModal(connector.id)}
                        >
                          Update Status
                        </button>
                      </td>
                    </tr>
                ))}
                </tbody>
              </table>
          )}
        </div>

        {/* Add Connector Modal */}
        {showAddConnectorModal && (
            <div className="overlay">
              <div className="modal">
                <div className="modal-header">
                  <h2>Add New Connector</h2>
                  <button className="close" onClick={() => setShowAddConnectorModal(false)}>&times;</button>
                </div>
                <div className="modal-body">
                  {error && <div className="error-msg">{error}</div>}
                  <form onSubmit={handleAddConnector}>
                    <div className="form-group">
                      <label htmlFor="connectorName">Connector Name</label>
                      <input
                          type="text"
                          id="connectorName"
                          value={newConnector.name}
                          onChange={(e) => setNewConnector({ name: e.target.value })}
                      />
                    </div>
                    <div className="form-buttons">
                      <button
                          type="button"
                          className="btn btn-gray"
                          onClick={() => setShowAddConnectorModal(false)}
                      >
                        Cancel
                      </button>
                      <button
                          type="submit"
                          className="btn"
                          disabled={addConnectorMutation.isPending}
                      >
                        {addConnectorMutation.isPending ? 'Saving...' : 'Save'}
                      </button>
                    </div>
                  </form>
                </div>
              </div>
            </div>
        )}

        {/* Update Status Modal */}
        {showUpdateStatusModal && selectedConnector !== null && (
            <div className="overlay">
              <div className="modal">
                <div className="modal-header">
                  <h2>Update Connector Status</h2>
                  <button className="close" onClick={() => setShowUpdateStatusModal(false)}>&times;</button>
                </div>
                <div className="modal-body">
                  {error && <div className="error-msg">{error}</div>}
                  <form onSubmit={handleUpdateStatus}>
                    <div className="form-group">
                      <label htmlFor="status">Status</label>
                      <select
                          id="status"
                          value={connectorStatus.status}
                          onChange={(e) => setConnectorStatus({...connectorStatus, status: e.target.value})}
                      >
                        <option value="Available">Available</option>
                        <option value="Preparing">Preparing</option>
                        <option value="Charging">Charging</option>
                        <option value="SuspendedEVSE">Suspended (EVSE)</option>
                        <option value="SuspendedEV">Suspended (EV)</option>
                        <option value="Finishing">Finishing</option>
                        <option value="Reserved">Reserved</option>
                        <option value="Unavailable">Unavailable</option>
                        <option value="Faulted">Faulted</option>
                      </select>
                    </div>
                    <div className="form-group">
                      <label htmlFor="errorCode">Error Code (optional)</label>
                      <input
                          type="text"
                          id="errorCode"
                          value={connectorStatus.errorCode}
                          onChange={(e) => setConnectorStatus({...connectorStatus, errorCode: e.target.value})}
                      />
                    </div>
                    <div className="form-group">
                      <label htmlFor="info">Additional Info (optional)</label>
                      <textarea
                          id="info"
                          rows={3}
                          value={connectorStatus.info}
                          onChange={(e) => setConnectorStatus({...connectorStatus, info: e.target.value})}
                      />
                    </div>
                    <div className="form-buttons">
                      <button
                          type="button"
                          className="btn btn-gray"
                          onClick={() => setShowUpdateStatusModal(false)}
                      >
                        Cancel
                      </button>
                      <button
                          type="submit"
                          className="btn"
                          disabled={updateStatusMutation.isPending}
                      >
                        {updateStatusMutation.isPending ? 'Updating...' : 'Update'}
                      </button>
                    </div>
                  </form>
                </div>
              </div>
            </div>
        )}
      </div>
  );
};

export default ChargePointDetail;