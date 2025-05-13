import { useEffect } from "react";
import { useAtom, useSetAtom } from "jotai";
import { JwtAtom } from "../../atoms";
import { UserSettingsAtom } from "../../atoms";
import toast from "react-hot-toast";
import { TitleTimeHeader } from "../import";
import { useApplyThemeFromSettings } from "./UseApplyThemeFromSettings.ts";

const DashboardPage = () => {
    const [jwt] = useAtom(JwtAtom);
    const setUserSettings = useSetAtom(UserSettingsAtom);

    useApplyThemeFromSettings();

    useEffect(() => {
        const fetchSettings = async () => {
            if (!jwt) return;

            try {
                const res = await fetch("http://localhost:5000/api/usersettings", {
                    headers: {
                        Authorization: `Bearer ${jwt}`,
                    },
                });

                if (!res.ok) {
                    const msg = await res.text();
                    throw new Error(msg);
                }

                const settings = await res.json();
                setUserSettings(settings);
            } catch (err: any) {
                toast.error("Failed to load user settings");
                console.error(err);
            }
        };

        fetchSettings();
    }, [jwt, setUserSettings]);

    return (
        <div className="min-h-[calc(100vh-64px)] flex flex-col bg-[--color-background] text-[--color-primary] font-display overflow-hidden">
            <TitleTimeHeader title="Dashboard" />
            <main className="flex-1 flex items-center justify-center overflow-hidden">
                <p className="text-[--color-primary]">Loading...</p>
            </main>
        </div>
    );
};

export default DashboardPage;
