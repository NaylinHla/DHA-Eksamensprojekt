// hooks/useApplyThemeFromSettings.ts
import { useEffect } from "react";
import { useAtomValue } from "jotai";
import { UserSettingsAtom } from "../../atoms";

export const useApplyThemeFromSettings = () => {
    const settings = useAtomValue(UserSettingsAtom);

    useEffect(() => {
        if (settings?.darkTheme === true) {
            document.documentElement.setAttribute("data-theme", "dark");
            localStorage.setItem("theme", "dark");
        } else if (settings?.darkTheme === false) {
            document.documentElement.setAttribute("data-theme", "light");
            localStorage.setItem("theme", "light");
        }
    }, [settings?.darkTheme]);
};
