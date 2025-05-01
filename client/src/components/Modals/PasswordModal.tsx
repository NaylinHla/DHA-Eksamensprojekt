import React, {useState} from "react";
import {PasswordField} from "../utils/PasswordField/PasswordField.tsx";

export interface PasswordDto {
    oldPassword: string;
    newPassword: string;
    confirm:     string;
}

type Props = {
    open: boolean;
    loading?: boolean;
    onClose: () => void;
    onSubmit: (dto: PasswordDto) => void;
};

const PasswordModal: React.FC<Props> = ({open, loading, onClose, onSubmit}) => {
    const [show, setShow] = useState(false);

    if (!open) return null;

    return (
        <dialog open className="modal modal-middle">
            <form
                onSubmit={e=>{
                    e.preventDefault();
                    const fd = new FormData(e.currentTarget);
                    onSubmit({
                        oldPassword: fd.get("old")  as string,
                        newPassword: fd.get("new")  as string,
                        confirm:     fd.get("confirm") as string
                    });
                }}
                method="dialog"
                className="modal-box max-w-md space-y-5"
            >
                <h3 className="text-center text-xl font-semibold">Change Password</h3>
                <p className="text-center text-gray-500 text-sm -mt-3">
                    Password must be at least 6 characters, and contain both lowercase and uppercase characters,
                    and at least one special character.
                </p>

                {["old", "new", "confirm"].map((name, idx) => (
                    <label key={name} className="block space-y-1">
                        <span className="label-text">
                            {idx === 0 ? "Current password" 
                                : idx === 1 ? "New password" 
                                    : "Repeat new password"}
                        </span>

                        <PasswordField
                            name={name}
                            placeholder={idx === 0 ? "Password" : "Password"}
                            required
                        />
                    </label>
                ))}

                <div className="modal-action mt-6">
                    <button type="button" onClick={onClose} className="btn btn-neutral bg-transparent btn-sm">Cancel</button>
                    <button type="submit" className="btn btn-primary btn-sm">
                        {loading && <span className="loading loading-spinner w-4 mr-1"/>}
                        Confirm
                    </button>
                </div>
            </form>
        </dialog>
    );
};

export default PasswordModal;
