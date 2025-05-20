import { useAtom } from "jotai";
import { UserSettingsAtom } from "../atoms";

// Hook for temperature conversion & unit
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

export function useConvertTemperatureInSentence() {
    const [settings] = useAtom(UserSettingsAtom);
    const useCelsius = settings?.celsius ?? true;

    const convertValue = (celsius: number) =>
        useCelsius ? celsius : celsius * 9 / 5 + 32;

    const formatNumber = (num: number) =>
        num.toFixed(1).replace(".", ",");

    function convertTemperatureInSentence(sentence: string): string {
        // Check if the sentence contains any °C temperature
        const hasCelsius = /-?\d+[.,]?\d*°C/.test(sentence);
        if (!hasCelsius) return sentence;

        // Convert all temperatures with °C
        let convertedSentence = sentence.replace(/(-?\d+[.,]?\d*)°C/g, (match, numStr) => {
            const num = parseFloat(numStr.replace(",", "."));
            if (isNaN(num)) return match;

            const converted = convertValue(num);
            const formatted = formatNumber(converted);
            const unit = useCelsius ? "°C" : "°F";

            return `${formatted}${unit}`;
        });

        // Convert all other numbers (without unit) but no unit added
        convertedSentence = convertedSentence.replace(
            /(-?\d+[.,]?\d*)(?!°C)(?!°F)(?!ppm)(?!hPa)/g,
            (match) => {
                if (match.match(/[a-zA-Z]/)) return match;

                const num = parseFloat(match.replace(",", "."));
                if (isNaN(num)) return match;

                const converted = convertValue(num);
                return formatNumber(converted);
            }
        );

        return convertedSentence;
    }

    return { convertTemperatureInSentence };
}