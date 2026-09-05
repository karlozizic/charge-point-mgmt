import type { ReactNode } from 'react';

interface ModalProps {
    title: string;
    onClose: () => void;
    error?: string;
    children: ReactNode;
}

function Modal({ title, onClose, error, children }: ModalProps) {
    return (
        <div className="overlay">
            <div className="modal">
                <div className="modal-header">
                    <h2>{title}</h2>
                    <button className="close" onClick={onClose}>&times;</button>
                </div>
                <div className="modal-body">
                    {error && <div className="error-msg">{error}</div>}
                    {children}
                </div>
            </div>
        </div>
    );
}

export default Modal;
