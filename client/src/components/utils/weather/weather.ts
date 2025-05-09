export const iconFromCode = (code: number) => {
    if ([0].includes(code)) return "☀️";
    if ([1,2].includes(code)) return "🌤️";
    if ([3].includes(code)) return "☁️";
    if ([45,48].includes(code)) return "🌫️";
    if (code >= 51 && code <= 67) return "🌦️";
    if (code >= 80 && code <= 99) return "🌧️";
    return "❓";
};

export const cToF = (c: number) => c * 9/5 + 32;
