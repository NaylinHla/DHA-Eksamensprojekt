import React, {ReactNode} from "react";
import {CancelIcon} from "../import";

export interface ConfirmModalProps {
    isOpen: boolean;
    title: string;
    subtitle?: string;
    onConfirm: () => void;
    onCancel: () => void;
    confirmVariant?: "primary" | "error";
    loading?: boolean;
    children?: ReactNode;
}

const ConfirmModal: React.FC<ConfirmModalProps> = ({
                                                       isOpen,
                                                       title,
                                                       subtitle,
                                                       onConfirm,
                                                       onCancel,
                                                       confirmVariant = "primary",
                                                       loading = false,
                                                       children,
                                                   }) => {
    if (!isOpen) return null;

    return (
        <dialog open className="modal modal-middle">
            <div className="modal-box max-w-md space-y-5 relative bg-[var(--color-cream)] border border-primary">

                {/* Close icon */}
                <button
                    type="button"
                    onClick={onCancel}
                    className="absolute top-4 right-4 text-gray-400 hover:text-gray-600"
                    aria-label="Close modal"
                >
                    <CancelIcon size={14}/>
                </button>

                {/* Heading */}
                <h3 className="text-center text-xl font-semibold">{title}</h3>
                {subtitle && (
                    <p className="text-center text-[--color-primary] text-sm -mt-3">{subtitle}</p>
                )}

                {/* Custom content */}
                {children}

                {/* Actions */}
                <div className="modal-action mt-6">
                    <button
                        type="button"
                        onClick={onCancel}
                        className="btn btn-neutral bg-transparent btn-sm"
                    >
                        Cancel
                    </button>
                    <button
                        type="button"
                        onClick={onConfirm}
                        className={`btn btn-sm ${
                            confirmVariant === "error" ? "btn-error" : "btn-primary"
                        } flex items-center gap-2`}
                    >
                        {loading && <span className="loading loading-spinner w-4"/>}
                        Confirm
                    </button>
                </div>

                {loading && (
                    <div className="absolute inset-0 flex items-center justify-center bg-black/20 rounded-lg">
                        <span className="loading loading-spinner w-6 h-6 text-white"/>
                    </div>
                )}
            </div>
        </dialog>
    );
};

export default ConfirmModal;