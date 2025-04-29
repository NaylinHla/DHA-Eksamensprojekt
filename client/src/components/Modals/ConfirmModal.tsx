import React from 'react';
import { CancelIcon } from '../import';


interface ConfirmModalProps {
    isOpen: boolean;
    title: string;
    subtitle: string;
    onConfirm: () => void;
    onCancel: () => void;
}

const ConfirmModal: React.FC<ConfirmModalProps> = ({ isOpen, title, subtitle, onConfirm, onCancel }) => {
    if (!isOpen) return null;

    return (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50">
            <div className="relative bg-[var(--color-background)] rounded-lg shadow-lg p-6 w-11/12 max-w-md">
                {/* Cancel/Close Icon */}
                <button
                    onClick={onCancel}
                    className="absolute top-4 right-4 text-gray-500 hover:text-gray-700"
                    aria-label="Close modal"
                >
                    <CancelIcon size={14} color="currentColor" />
                </button>

                {/* Title */}
                <h2 className="text-lg font-bold text-gray-800">{title}</h2>
                {/* Subtitle */}
                <p className="text-gray-600 mt-2">{subtitle}</p>

                {/* Buttons */}
                <div className="flex mt-6 space-x-4">
                    <button
                        onClick={onCancel}
                        className="flex-1 px-6 py-3 bg-transparent border border-black text-[var(--color-textprimary)] rounded hover:bg-gray-100"
                    >
                        No
                    </button>
                    <button
                        onClick={onConfirm}
                        className="flex-1 px-6 py-3 bg-primary text-[var(--color-textsecondary)] rounded hover:bg-[var(--color-primaryhover)]"
                    >
                        Yes, confirm
                    </button>
                </div>
            </div>
        </div>
    );
};

export default ConfirmModal;
