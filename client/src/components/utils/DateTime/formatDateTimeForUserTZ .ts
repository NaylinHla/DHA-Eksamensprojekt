// src/utils/formatDate.ts
export function formatDateTimeForUserTZ(
    date: Date | string | undefined,
    locale = "da-DK",
    timeZone = Intl.DateTimeFormat().resolvedOptions().timeZone
): string {
    if (!date) return "N/A";

    // Force UTC parsing if string has no Z
    let dateObj = typeof date === "string"
        ? new Date(date.endsWith("Z") ? date : date + "Z")
        : date;

    return dateObj.toLocaleString(locale, {
        timeZone,
        year: "numeric",
        month: "short",
        day: "numeric",
        hour: "2-digit",
        minute: "2-digit",
        second: "2-digit"
    });
}
