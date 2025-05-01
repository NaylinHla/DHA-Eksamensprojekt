import React, {useEffect, useState} from "react";
import {Pencil, Trash2} from "lucide-react";
import toast from "react-hot-toast";
import {useAtom} from "jotai";
import {JwtAtom, PatchUserEmailDto, PatchUserPasswordDto, UserClient} from "../../atoms";
import {useNavigate} from "react-router";
import EmailModal from "../../components/Modals/EmailModal.tsx";
import PasswordModal, {PasswordDto} from "../../components/Modals/PasswordModal.tsx";
import DeleteAccountModal from "../../components/Modals/DeleteAccountModal.tsx";

type Props = { onChange?: () => void };

const userClient = new UserClient("http://localhost:5000");

const UserSettings: React.FC<Props> = ({ onChange }) => {
    const [jwt, setJwt] = useAtom(JwtAtom);
    const [saving, setSaving] = useState(false);

    const [confirmWater, setConfirmWater] = useState(false);
    const [fahrenheit, setFahrenheit] = useState(false);
    const [darkTheme, setDarkTheme] = useState(false);

    const [openPassword, setOpenPassword] = useState(false);
    const [openEmail, setOpenEmail] = useState(false);
    const [openDelete, setOpenDelete] = useState(false);

    const navigate = useNavigate();;

    // ------

    const spinner = (
        <svg className="animate-spin h-5 w-5" viewBox="0 0 24 24">
            <circle className="opacity-25 animate-gradientSpinner" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" fill="none"/>
            <path className="opacity-75" d="M4 12a8 8 0 018-8v4a4 4 0 00-4 4H4z" fill="currentColor"/>
        </svg>
    );


    async function handleDelete() {
        try {
            setSaving(true);
            await userClient.deleteUser(`Bearer ${jwt}`, {email: ""});
            toast.success("Account deleted – goodbye!");
            localStorage.removeItem("jwt");
            setJwt("");
            navigate("/auth", {replace: true});
        } catch (e: any) {
            toast.error(e.message ?? "Failed");
        } finally { setSaving(false); }
    }

    async function handlePasswordDto(dto: PasswordDto) {
        if (dto.newPassword !== dto.confirm) { toast.error("Passwords don’t match"); return; }
        try {
            setSaving(true);
            await userClient.patchUserPassword(`Bearer ${jwt}`, {
                oldPassword: dto.oldPassword,
                newPassword: dto.newPassword,
            });
            toast.success("Password updated");
            setOpenPassword(false);
        } catch (e:any) { toast.error(e.message ?? "Failed"); } finally { setSaving(false); }
    }

    async function handleEmail(oldMail:string,newMail:string) {
        try {
            setSaving(true);
            await userClient.patchUserEmail(`Bearer ${jwt}`, {oldEmail: oldMail, newEmail: newMail});
            toast.success("E-mail updated – please log in with the new address.");
            setOpenEmail(false);
        } catch (e:any){ toast.error(e.message ?? "Failed"); } finally { setSaving(false); }
    }

    const saveToggles = () => {
        onChange?.();
        toast.success("Visual settings saved (local-only for now)");
    };

    return (
        <div className="min-h-[calc(100vh-64px)] flex flex-col bg-[--color-background] text-[--color-primary] font-display">

            {/* Header */}
            <header className="w-full bg-white shadow px-6 py-4 flex justify-between items-center">
                <h1 className="text-2xl font-bold">User Profile</h1>
            </header>

            {/* Content card */}
            <section className="mx-4 my-6 lg:mx-8 flex flex-1 overflow-hidden rounded-lg">

                {/* LEFT column – actions + toggles */}
                <aside className="w-72 shrink-0 border-r border-gray-200 p-6 flex flex-col gap-4">
                    <h2 className="text-xl font-semibold">Settings</h2>

                    <div className="flex flex-col gap-3">
                        <button onClick={()=>setOpenPassword(true)}   className="btn btn-neutral btn-sm">Change password</button>
                        <button onClick={()=>setOpenEmail(true)} className="btn btn-neutral btn-sm">Change e-mail</button>
                        <button onClick={()=>setOpenDelete(true)} className="btn btn-error btn-sm flex items-center gap-1">
                            Delete my account <Trash2 size={14}/>
                        </button>
                    </div>

                    <div className="divider my-1"/>
                    <ul className="flex-1 flex flex-col gap-2 pr-1 overflow-y-auto">
                        <li className="flex justify-between items-center">
                            <span>Confirm Water Dialog</span>
                            <input type="checkbox" className="toggle toggle-sm"
                                   checked={confirmWater} onChange={()=>setConfirmWater(!confirmWater)}/>
                        </li>
                        <li className="flex justify-between items-center">
                            <span>Fahrenheit</span>
                            <input type="checkbox" className="toggle toggle-sm"
                                   checked={fahrenheit} onChange={()=>setFahrenheit(!fahrenheit)}/>
                        </li>
                        <li className="flex justify-between items-center">
                            <span>Dark Theme</span>
                            <input type="checkbox" className="toggle toggle-sm"
                                   checked={darkTheme} onChange={()=>setDarkTheme(!darkTheme)}/>
                        </li>
                        <li className="flex justify-between"><span>IoT Wait time</span><span className="text-sm opacity-70">12 m</span></li>
                    </ul>

                    <button onClick={saveToggles} className="btn btn-primary btn-sm mt-auto">Save settings</button>
                </aside>

                {/* RIGHT column – user data */}
                <article className="relative flex-1 p-8 overflow-y-auto">
                    <button className="absolute right-8 top-8 text-gray-300 hover:text-[--color-primary]" aria-label="edit">
                        <Pencil size={20}/>
                    </button>

                    <p className="italic text-gray-500">
                        User data will appear here once the <code>/api/User/GetCurrent</code> endpoint is available.
                    </p>
                </article>
            </section>

            {/* Modals */}
            {openPassword && (
                <PasswordModal
                    open={openPassword}
                    loading={saving}
                    onClose={()=>setOpenPassword(false)}
                    onSubmit={handlePasswordDto}
                />
            )}

            <EmailModal
                open={openEmail}
                loading={saving}
                onClose={()=>setOpenEmail(false)}
                onSubmit={handleEmail}
            />

            +<DeleteAccountModal
                open={openDelete}
                loading={saving}
                onCancel={()=>setOpenDelete(false)}
                onConfirm={handleDelete}
            />

            {/* gradient spinner keyframes */}
            <style jsx>{`
        @keyframes gradientSpinner {
          0%   { stroke: var(--color-primary); }
          50%  { stroke: #ff00ff; }
          100% { stroke: var(--color-primary); }
        }
        .animate-gradientSpinner { animation: gradientSpinner 3s ease-in-out infinite; }
      `}</style>
        </div>
    );
};

export default UserSettings;
