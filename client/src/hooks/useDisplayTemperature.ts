import { useAtom } from "jotai";
import { UserSettingsAtom } from "../atoms";

export function useDisplayTemperature() {
    const [settings] = useAtom(UserSettingsAtom);
    const useCelsius = settings?.celsius ?? true;

    const convert = (celsius: number | null | undefined): number | null => {
        if (celsius == null) return null;
        return useCelsius ? celsius : celsius * 9 / 5 + 32;
    };

    const unit = useCelsius ? "°C" : "°F";

    return { convert, unit, useCelsius };
}
