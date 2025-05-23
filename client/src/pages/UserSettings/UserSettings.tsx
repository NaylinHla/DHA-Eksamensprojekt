import React, { useEffect, useState } from "react";
import { Trash2 } from "lucide-react";
import toast from "react-hot-toast";
import { useAtom, useSetAtom } from "jotai";
import {JwtAtom, User, UserSettingsAtom} from "../../atoms";
import EmailModal from "../../components/Modals/EmailModal";
import PasswordModal, { PasswordDto } from "../../components/Modals/PasswordModal";
import DeleteAccountModal from "../../components/Modals/DeleteAccountModal";
import { TitleTimeHeader, useLogout } from "../../components";
import { userClient, userSettingsClient } from "../../apiControllerClients";

type Props = { onChange?: () => void };
const LOCAL_KEY = "theme";

const UserSettings: React.FC<Props> = () => {
    const [jwt, setJwt] = useAtom(JwtAtom);
    const [settings] = useAtom(UserSettingsAtom);
    const setUserSettings = useSetAtom(UserSettingsAtom);

    const [saving, setSaving] = useState(false);
    const [openPassword, setOpenPassword] = useState(false);
    const [openEmail, setOpenEmail] = useState(false);
    const [openDelete, setOpenDelete] = useState(false);
    const [passwordErrors, setPasswordErrors] = useState<Partial<PasswordDto>>({});
    const [emailErrors, setEmailErrors] = useState<{ old?: string; new?: string }>({});
    const [user, setUser] = useState<User | null>(null);

    const { logout } = useLogout();

    useEffect(() => {
        const theme = settings?.darkTheme ? "dark" : "light";
        document.documentElement.setAttribute("data-theme", theme);
        localStorage.setItem("theme", theme);
    }, [settings?.darkTheme]);

    useEffect(() => {
        if (!jwt) return;
        (async () => {
            try {
                const u = await userClient.getUser(jwt);
                setUser(u);
            } catch (error) {
                console.error(error);
                toast.error("Couldn't load user info", { id: "loadUserInfo-failed" });
            }
        })();
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
            toast.success("Account deleted – goodbye!", { id: "accountDeleted-succes" });
            localStorage.removeItem("jwt");
            setJwt("");
            logout();
        } catch (e: any) {
            toast.error(e.message ?? "Failed", { id: "accountDeleted-failed" });
        } finally {
            setSaving(false);
        }
    }

    async function handlePasswordDto(dto: PasswordDto) {
        if (dto.newPassword !== dto.confirm) {
            setPasswordErrors({ confirm: "Passwords don’t match" });
            return;
        }
        try {
            setSaving(true);
            if (!jwt) return;
            await userClient.patchUserPassword(jwt, {
                oldPassword: dto.oldPassword,
                newPassword: dto.newPassword,
            });
            toast.success("Password updated", { id: "passwordChange-succes" });
            setOpenPassword(false);
            setPasswordErrors({});
        } catch (e: any) {
            const status = e.response?.status ?? e.status ?? (e.response && e.response.statusCode);
            const errorMessage =
                (e.response && (e.response.title || e.response.message)) || e.message || "Unknown error";

            if (status === 401 || /invalid/i.test(errorMessage)) {
                setPasswordErrors({ oldPassword: "Old password is incorrect" });
            } else {
                toast.error(errorMessage || "Failed to change password", { id: "passwordChange-failed" });
            }
        } finally {
            setSaving(false);
        }
    }

    async function handleEmail(oldMail: string, newMail: string) {
        if (oldMail !== user?.email) {
            setEmailErrors({ old: "This is not your current email" });
            return;
        }

        try {
            setSaving(true);
            if (!jwt) return;
            await userClient.patchUserEmail(jwt, {
                oldEmail: oldMail,
                newEmail: newMail,
            });
            setOpenEmail(false);
            logout();
        } catch (e: any) {
            const resp = typeof e.response === "string"
                ? JSON.parse(e.response) || {}
                : e.response || {};

            const status = resp.status ?? e.status;
            const title  = (resp.title ?? "").replace(/[\r\n]+/g, " ");

            if (status === 400 && /email already used/i.test(title)) {
                setEmailErrors({ new: "This email is already in use by another user" });
            } else {
                toast.error(title || e.message || "Failed to update email");
            }

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

            {/* desktop → row | mobile → column */}
            <section className="flex flex-1 flex-col md:flex-row overflow-hidden rounded-lg p-fluid">

                {/* ───────────── settings (left / top) ───────────── */}
                <aside
                    className="
                        w-full md:w-[clamp(12rem,20vw,25rem)]
                        shrink-0
                        border-b md:border-b-0 md:border-r
                        border-primary
                        p-fluid flex flex-col gap-fluid
                    ">
                    <h2 className="text-fluid font-semibold">Settings:</h2>

                    <div className="flex flex-col gap-3">
                        <button
                            onClick={() => setOpenEmail(true)}
                            className="btn btn-lg border-neutral bg-transparent hover:text-white hover:bg-neutral text-fluid py-[clamp(0.5rem,1vw,0.75rem)]">
                            Change e-mail
                        </button>
                        <button
                            onClick={() => setOpenPassword(true)}
                            className="btn btn-lg border-neutral bg-transparent hover:text-white hover:bg-neutral text-fluid py-[clamp(0.5rem,1vw,0.75rem)]">
                            Change password
                        </button>
                        <button
                            onClick={() => setOpenDelete(true)}
                            className="btn btn-lg btn-error flex items-center gap-[clamp(0.25rem,0.8vw,0.5rem)] text-fluid px-fluid py-[clamp(0.5rem,1vw,0.75rem)]">
                            <Trash2 className="icon-fluid" /> Delete my account
                        </button>
                    </div>

                    {/* vertical on ≥md, horizontal on mobile */}
                    <div className="divider divider-horizontal md:divider-vertical my-1" />

                    <ul className="flex-1 flex flex-col gap-2 pr-1 overflow-y-auto">
                        <li className="flex justify-between items-center">
                            <span className="text-fluid">Celsius</span>
                            <input
                                type="checkbox"
                                className="toggle"
                                checked={settings.celsius}
                                onChange={(e) => {
                                    const value = e.target.checked;
                                    patchSetting("celsius", value).then();
                                    setUserSettings((prev) => ({
                                        ...prev!,
                                        celsius: value,
                                    }));
                                }}
                            />
                        </li>
                        <li className="flex justify-between items-center">
                            <span className="text-fluid">Dark Theme</span>
                            <input
                                type="checkbox"
                                className="toggle"
                                checked={settings.darkTheme}
                                onChange={(e) => {
                                    const value = e.target.checked;
                                    patchSetting("darktheme", value).then();
                                    setUserSettings((prev) => ({
                                        ...prev!,
                                        darkTheme: value,
                                    }));
                                }}
                            />
                        </li>
                        <li className="flex justify-between items-center">
                            <span className="text-fluid">Confirm Water Dialog</span>
                            <input
                                type="checkbox"
                                className="toggle"
                                checked={settings.confirmDialog}
                                onChange={(e) => {
                                    const value = e.target.checked;
                                    patchSetting("confirmdialog", value).then();
                                    setUserSettings((prev) => ({
                                        ...prev!,
                                        confirmDialog: value,
                                    }));
                                }}
                            />
                        </li>
                    </ul>
                </aside>

                {/* user info (right / bottom) */}
                <article className="relative flex-1 p-fluid overflow-y-auto">
                    {!user ? (
                        <p className="italic text-fluid">Loading profile…</p>
                    ) : (
                        <div className="space-y-2 ">
                            <h3 className="text-lg md:text-xl lg:text-2xl font-semibold">Account details</h3>

                            <p>
                                <span className="font-medium">Name:</span>{" "}
                                {user.firstName} {user.lastName}
                            </p>

                            <p>
                                <span className="font-medium">E-mail:</span> {user.email}
                            </p>

                            {user.birthday && (
                                <p>
                                    <span className="font-medium">Birthday:</span>{" "}
                                    {new Date(user.birthday).toLocaleDateString()}
                                </p>
                            )}

                            {user.country && (
                                <p>
                                    <span className="font-medium">Country:</span> {user.country}
                                </p>
                            )}
                        </div>
                    )}
                </article>
            </section>

            {/* ───────────── modals ───────────── */}
            {openPassword && (
                <PasswordModal
                    open={openPassword}
                    loading={saving}
                    onClose={() => {
                        setOpenPassword(false);
                        setPasswordErrors({});
                    }}
                    onSubmit={handlePasswordDto}
                    externalErrors={passwordErrors}
                />
            )}

            <EmailModal
                open={openEmail}
                loading={saving}
                onClose={() => {
                    setOpenEmail(false);
                    setEmailErrors({});
                }}
                onSubmit={handleEmail}
                externalErrors={emailErrors}
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