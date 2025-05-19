import React, { useEffect, useState } from "react";
import { Pencil, Trash2 } from "lucide-react";
import toast from "react-hot-toast";
import { useAtom, useSetAtom } from "jotai";
import { JwtAtom, UserSettingsAtom } from "../../atoms";
import { useNavigate } from "react-router";
import EmailModal from "../../components/Modals/EmailModal";
import PasswordModal, { PasswordDto } from "../../components/Modals/PasswordModal";
import DeleteAccountModal from "../../components/Modals/DeleteAccountModal";
import { TitleTimeHeader, useLogout } from "../../components";
import { userClient, userSettingsClient } from "../../apiControllerClients";

type Props = { onChange?: () => void };
const LOCAL_KEY = "theme";

const UserSettings: React.FC<Props> = ({ onChange }) => {
    const [jwt, setJwt] = useAtom(JwtAtom);
    const [settings] = useAtom(UserSettingsAtom);
    const setUserSettings = useSetAtom(UserSettingsAtom);

    const [saving, setSaving] = useState(false);
    const [openPassword, setOpenPassword] = useState(false);
    const [openEmail, setOpenEmail] = useState(false);
    const [openDelete, setOpenDelete] = useState(false);

    const navigate = useNavigate();
    const { logout } = useLogout();

    useEffect(() => {
        const theme = settings?.darkTheme ? "dark" : "light";
        document.documentElement.setAttribute("data-theme", theme);
        localStorage.setItem(LOCAL_KEY, theme);
    }, [settings?.darkTheme]);

    async function patchSetting(name: string, value: boolean) {
        try {
            if (!jwt) return;
            await userSettingsClient.patchSetting(name, { value }, jwt);
        } catch (e: any) {
            console.error(e);
        }
    }

    async function handleDelete() {
        try {
            setSaving(true);
            if (!jwt) return;
            await userClient.deleteUser(jwt);
            toast.success("Account deleted – goodbye!");
            localStorage.removeItem("jwt");
            setJwt("");
            logout();
        } catch (e: any) {
            toast.error(e.message ?? "Failed");
        } finally {
            setSaving(false);
        }
    }

    async function handlePasswordDto(dto: PasswordDto) {
        if (dto.newPassword !== dto.confirm) {
            toast.error("Passwords don’t match");
            return;
        }
        try {
            setSaving(true);
            if (!jwt) return;
            await userClient.patchUserPassword(jwt, {
                oldPassword: dto.oldPassword,
                newPassword: dto.newPassword,
            });
            toast.success("Password updated");
            setOpenPassword(false);
        } catch (e: any) {
            toast.error(e.message ?? "Failed");
        } finally {
            setSaving(false);
        }
    }

    async function handleEmail(oldMail: string, newMail: string) {
        try {
            setSaving(true);
            if (!jwt) return;
            await userClient.patchUserEmail(jwt, {
                oldEmail: oldMail,
                newEmail: newMail,
            });
            toast.success("E-mail updated – please log in with the new address.");
            setOpenEmail(false);
        } catch (e: any) {
            toast.error(e.message ?? "Failed");
        } finally {
            setSaving(false);
        }
    }

    if (!settings) {
        return <p className="p-6">Loading user settings...</p>;
    }

    return (
        <div className="min-h-[calc(100vh-64px)] flex flex-col bg-[--color-background] text-[--color-primary] font-display">
            <TitleTimeHeader title="User Profile" />
            <section className="mx-4 my-6 lg:mx-8 flex flex-1 overflow-hidden rounded-lg">
                <aside className="w-72 shrink-0 border-r border-gray-200 p-6 flex flex-col gap-4">
                    <h2 className="text-xl font-semibold">Settings</h2>
                    <div className="flex flex-col gap-3">
                        <button onClick={() => setOpenEmail(true)} className="btn btn-neutral bg-transparent btn-sm">
                            Change e-mail
                        </button>
                        <button onClick={() => setOpenPassword(true)} className="btn btn-neutral bg-transparent btn-sm">
                            Change password
                        </button>
                        <button onClick={() => setOpenDelete(true)} className="btn btn-error btn-sm flex items-center gap-1">
                            <Trash2 size={14} /> Delete my account
                        </button>
                    </div>

                    <div className="divider my-1" />
                    <ul className="flex-1 flex flex-col gap-2 pr-1 overflow-y-auto">
                        <li className="flex justify-between items-center">
                            <span>Celsius</span>
                            <input
                                type="checkbox"
                                className="toggle toggle-sm"
                                checked={settings.celsius}
                                onChange={(e) => {
                                    const value = e.target.checked;
                                    patchSetting("celsius", value);
                                    setUserSettings((prev) => ({
                                        ...prev!,
                                        celsius: value,
                                    }));
                                }}
                            />
                        </li>
                        <li className="flex justify-between items-center">
                            <span>Dark Theme</span>
                            <input
                                type="checkbox"
                                className="toggle toggle-sm"
                                checked={settings.darkTheme}
                                onChange={(e) => {
                                    const value = e.target.checked;
                                    patchSetting("darktheme", value);
                                    setUserSettings((prev) => ({
                                        ...prev!,
                                        darkTheme: value,
                                    }));
                                }}
                            />
                        </li>
                        <li className="flex justify-between items-center">
                            <span>Confirm Water Dialog</span>
                            <input
                                type="checkbox"
                                className="toggle toggle-sm"
                                checked={settings.confirmDialog}
                                onChange={(e) => {
                                    const value = e.target.checked;
                                    patchSetting("confirmdialog", value);
                                    setUserSettings((prev) => ({
                                        ...prev!,
                                        confirmDialog: value,
                                    }));
                                }}
                            />
                        </li>
                    </ul>
                </aside>
                <article className="relative flex-1 p-8 overflow-y-auto">
                    <button className="absolute right-8 top-8 text-gray-300 hover:text-[--color-primary]" aria-label="edit">
                        <Pencil size={20} />
                    </button>
                    <p className="italic text-gray-500">
                        User data will appear here once the <code>/api/User/GetCurrent</code> endpoint is available.
                    </p>
                </article>
            </section>

            {openPassword && (
                <PasswordModal
                    open={openPassword}
                    loading={saving}
                    onClose={() => setOpenPassword(false)}
                    onSubmit={handlePasswordDto}
                />
            )}
            <EmailModal
                open={openEmail}
                loading={saving}
                onClose={() => setOpenEmail(false)}
                onSubmit={handleEmail}
            />
            <DeleteAccountModal
                open={openDelete}
                loading={saving}
                onCancel={() => setOpenDelete(false)}
                onConfirm={handleDelete}
            />
        </div>
    );
};

export default UserSettings;
