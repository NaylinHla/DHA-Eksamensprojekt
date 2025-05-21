import React, { useEffect, useState } from "react";

type Props = {
    open: boolean;
    loading?: boolean;
    onClose: () => void;
    onSubmit: (oldMail: string, newMail: string) => void;
    externalErrors?: { old?: string; new?: string };
};

const EmailModal: React.FC<Props> = ({ open, loading, onClose, onSubmit, externalErrors }) => {
    const [errors, setErrors] = useState<{ old?: string; new?: string }>({});
    const [oldEmail, setOldEmail] = useState("");
    const [newEmail, setNewEmail] = useState("");

    useEffect(() => {
        if (open) {
            setErrors(externalErrors || {});
        }
    }, [externalErrors, open]);

    useEffect(() => {
        if (!open) {
            setOldEmail("");
            setNewEmail("");
            setErrors({});
        }
    }, [open]);

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

        const newErrors: { old?: string; new?: string } = {};

        if (!oldEmail) newErrors.old = "Current email is required";
        if (!newEmail) {
            newErrors.new = "New email is required";
        } else if (!/^[\w-.]+@([\w-]+\.)+[\w-]{2,4}$/.test(newEmail)) {
            newErrors.new = "Enter a valid email address";
        } else if (oldEmail.trim() === newEmail.trim()) {
            newErrors.new = "New email must be different from the current email";
        }

        setErrors(newErrors);

        if (Object.keys(newErrors).length === 0) {
            onSubmit(oldEmail.trim(), newEmail.trim());
        }
    };


    if (!open) return null;

    return (
        <dialog open className="modal modal-middle">
            <form
                onSubmit={handleSubmit}
                method="dialog"
                className="modal-box max-w-md space-y-5"
                noValidate
            >
                <h3 className="text-center text-xl font-semibold">Changing e-mail address</h3>
                <p className="text-center text-gray-500 text-sm -mt-3">
                    When you change to a new e-mail address, you need to confirm the address before the change is made.
                </p>

                <label className="block space-y-1">
                    <span className="label-text mb-1">Current e-mail address</span>
                    <input
                        name="old"
                        type="email"
                        value={oldEmail}
                        onChange={(e) => setOldEmail(e.target.value)}
                        className={`input input-bordered input-sm w-full bg-white text-black pr-10 ${errors.old ? "border-red-500" : ""}`}
                        placeholder="Current e-mail"
                    />
                    {requiredHint(errors.old)}
                </label>

                <label className="block space-y-1">
                    <span className="label-text mb-1">New e-mail address</span>
                    <input
                        name="new"
                        type="email"
                        value={newEmail}
                        onChange={(e) => setNewEmail(e.target.value)}
                        className={`input input-bordered input-sm w-full bg-white text-black pr-10 ${errors.old ? "border-red-500" : ""}`}
                        placeholder="New e-mail"
                    />
                    {requiredHint(errors.new)}
                </label>

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

export default EmailModal;
