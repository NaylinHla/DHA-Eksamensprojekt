import React, { useEffect, useState } from "react";
import { Pencil, Trash2 } from "lucide-react";
import toast from "react-hot-toast";
import {useAtom, useSetAtom} from "jotai";
import { JwtAtom } from "../../atoms";
import { useNavigate } from "react-router";
import EmailModal from "../../components/Modals/EmailModal";
import PasswordModal, { PasswordDto } from "../../components/Modals/PasswordModal";
import DeleteAccountModal from "../../components/Modals/DeleteAccountModal";
import { TitleTimeHeader, useLogout } from "../../components";
import { userClient, userSettingsClient } from "../../apiControllerClients";
import { UserSettingsAtom } from '../../atoms'

type Props = { onChange?: () => void };
const LOCAL_KEY = "theme";

const UserSettings: React.FC<Props> = ({ onChange }) => {
    const [jwt, setJwt] = useAtom(JwtAtom);
    const [saving, setSaving] = useState(false);

    const [confirmWater, setConfirmWater] = useState(false);
    const [celsius, setCelsius] = useState(true);

    const [darkTheme, setDarkTheme] = useState(() => {
        const stored = localStorage.getItem(LOCAL_KEY);
        return stored === "dark";
    });

    const [openPassword, setOpenPassword] = useState(false);
    const [openEmail, setOpenEmail] = useState(false);
    const [openDelete, setOpenDelete] = useState(false);
    const [settings] = useAtom(UserSettingsAtom);


    const navigate = useNavigate();
    const { logout } = useLogout();

    useEffect(() => {
        const theme = darkTheme ? "dark" : "light";
        document.documentElement.setAttribute("data-theme", theme);
        localStorage.setItem(LOCAL_KEY, theme);
    }, [darkTheme]);

    // I put a fake timeout on it, cause it loads without a jwt token if you don't :(
    const setUserSettings = useSetAtom(UserSettingsAtom);

    useEffect(() => {
        if (!jwt || jwt.trim() === "") return;

        const timeout = setTimeout(() => {
            const fetchSettings = async () => {
                try {
                    const data = await userSettingsClient.getAllSettings(jwt);

                    // Update local component state (optional)
                    setConfirmWater(data.confirmDialog ?? false);
                    setCelsius(data.celsius ?? false);
                    setDarkTheme(data.darkTheme ?? false);

                    setUserSettings({
                        celsius: data.celsius ?? false,
                        darkTheme: data.darkTheme ?? false,
                        confirmDialog: data.confirmDialog ?? false,
                        secretMode: data.secretMode ?? false,
                    });
                } catch (e: any) {
                    toast.error("Could not load user settings");
                    console.error(e);
                }
            };

            fetchSettings();
        }, 500);

        return () => clearTimeout(timeout);
    }, [jwt]);

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
                                checked={celsius}
                                onChange={(e) => {
                                    const value = e.target.checked;
                                    setCelsius(value);
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
                                checked={darkTheme}
                                onChange={(e) => {
                                    const value = e.target.checked;
                                    setDarkTheme(value);
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
                                checked={confirmWater}
                                onChange={(e) => {
                                    const value = e.target.checked;
                                    setConfirmWater(value);
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
