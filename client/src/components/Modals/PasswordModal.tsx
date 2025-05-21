import React, {useEffect, useState} from "react";
import {PasswordField} from "../utils/PasswordField/PasswordField.tsx";

export interface PasswordDto {
    oldPassword: string;
    newPassword: string;
    confirm: string;
}

type Props = {
    open: boolean;
    loading?: boolean;
    onClose: () => void;
    onSubmit: (dto: PasswordDto) => void;
    externalErrors?: Partial<PasswordDto>;
};

const PasswordModal: React.FC<Props> = ({open, loading, onClose, onSubmit, externalErrors,}) => {
    const [errors, setErrors] = useState<Partial<PasswordDto>>({});

    useEffect(() => {
        if (open) {
            setErrors(externalErrors || {});
        }
    }, [externalErrors, open]);

    if (!open) return null;

    const requiredHint = (msg?: string) => (
        <p
            className={`text-red-500 text-xs text-left transition-opacity duration-200 h-[0.3rem] ${
                msg ? "opacity-100" : "opacity-0"
            }`}
        >
            {msg || " "}
        </p>
    );

    const handleSubmit = (e: React.FormEvent<HTMLFormElement>) => {
        e.preventDefault();
        const fd = new FormData(e.currentTarget);
        const oldPassword = fd.get("old") as string;
        const newPassword = fd.get("new") as string;
        const confirm = fd.get("confirm") as string;

        const passwordRegex = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*]).{6,}$/;
        const newErrors: Partial<PasswordDto> = {};

        if (!oldPassword) {
            newErrors.oldPassword = "Current password is required";
        } else if (!passwordRegex.test(oldPassword)) {
            newErrors.oldPassword =
                "Current password must be ≥6 chars, include uppercase, lowercase, number & special character";
        }


        if (!newPassword) {
            newErrors.newPassword = "New password is required";
        } else if (!passwordRegex.test(newPassword)) {
            newErrors.newPassword =
                "At least 6 chars, including uppercase, lowercase, number & special character";
        }

        if (confirm !== newPassword) {
            newErrors.confirm = "Passwords do not match";
        }

        setErrors(newErrors);

        if (Object.keys(newErrors).length === 0) {
            onSubmit({oldPassword, newPassword, confirm});
        }
    };

    return (
        <dialog open className="modal modal-middle">
            <form
                onSubmit={handleSubmit}
                method="dialog"
                noValidate
                className="modal-box max-w-md space-y-5"
            >
                <h3 className="text-center text-xl font-semibold">Change Password</h3>
                <p className="text-center text-gray-500 text-sm -mt-3">
                    Password must be at least 6 characters, and contain both lowercase and uppercase characters,
                    and at least one special character.
                </p>

                {["old", "new", "confirm"].map((name, idx) => {
                    const label =
                        idx === 0 ? "Current password" : idx === 1 ? "New password" : "Repeat new password";

                    const errorKey =
                        name === "old" ? "oldPassword" : name === "new" ? "newPassword" : "confirm";

                    return (
                        <label key={name} className="block space-y-1">
                            <span className="label-text">{label}</span>
                            <PasswordField
                                name={name}
                                placeholder="Password"
                                className={errors[errorKey] ? "border-red-500" : ""}
                            />
                            {requiredHint(errors[errorKey])}
                        </label>
                    );
                })}

                <div className="modal-action mt-6">
                    <button type="button" onClick={onClose} className="btn btn-neutral bg-transparent btn-sm">Cancel
                    </button>
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
