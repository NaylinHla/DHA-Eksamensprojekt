import ConfirmModal from "./ConfirmModal";
import React from "react";

type Props = {
    open: boolean;
    loading?: boolean;
    onCancel: () => void;
    onConfirm: () => void;
};

const DeleteAccountModal: React.FC<Props> = ({
                                                 open,
                                                 loading,
                                                 onCancel,
                                                 onConfirm,
                                             }) => (
    <ConfirmModal
        isOpen={open}
        title="Delete account"
        subtitle="This action is irreversible. All your data will be permanently removed."
        confirmVariant="error"
        loading={loading}
        onCancel={onCancel}
        onConfirm={onConfirm}
    />
);

export default DeleteAccountModal;
