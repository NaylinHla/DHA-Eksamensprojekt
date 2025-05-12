type Props = {
    open: boolean;
    loading?: boolean;
    onClose: () => void;
    onSubmit: (oldMail: string, newMail: string) => void;
};

const EmailModal: React.FC<Props> = ({open, loading, onClose, onSubmit}) => {
    if (!open) return null;

    return (
        <dialog open className="modal modal-middle">
            <form
                onSubmit={e => {
                    e.preventDefault();
                    const fd = new FormData(e.currentTarget);
                    onSubmit(fd.get("old") as string, fd.get("new") as string);
                }}
                method="dialog"
                className="modal-box max-w-md space-y-5"
            >
                <h3 className="text-center text-xl font-semibold">Changing e-mailaddress</h3>
                <p className="text-center text-gray-500 text-sm -mt-3">
                    When you change to a new e-mail address, you need to confirm the address before the change is made.
                </p>

                <label className="block">
                    <span className="label-text mb-1">Current e-mailaddress</span>
                    <input name="old" type="email" className="input input-bordered w-full" placeholder="Email"
                           required/>
                </label>

                <label className="block">
                    <span className="label-text mb-1">New e-mailaddress</span>
                    <input name="new" type="email" className="input input-bordered w-full" placeholder="Email"
                           required/>
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
