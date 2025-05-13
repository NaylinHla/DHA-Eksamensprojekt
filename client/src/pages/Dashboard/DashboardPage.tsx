import { useEffect } from "react";
import { useAtom, useSetAtom } from "jotai";
import { JwtAtom, UserSettingsAtom } from "../../atoms";
import toast from "react-hot-toast";
import { TitleTimeHeader } from "../import";
import { useApplyThemeFromSettings } from "./UseApplyThemeFromSettings";
import { userSettingsClient } from "../../apiControllerClients";

const DashboardPage = () => {
    const [jwt] = useAtom(JwtAtom);
    const setUserSettings = useSetAtom(UserSettingsAtom);

    useApplyThemeFromSettings();

    useEffect(() => {
        const fetchSettings = async () => {
            if (!jwt) return;

            try {
                const settings = await userSettingsClient.getAllSettings(`Bearer ${jwt ?? ""}`);
                setUserSettings({
                    celsius: settings.celsius ?? false,
                    darkTheme: settings.darkTheme ?? false,
                    confirmDialog: settings.confirmDialog ?? false,
                    secretMode: settings.secretMode ?? false,
                });
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
