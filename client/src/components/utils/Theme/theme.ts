export const cssVar = (name: string) =>
    getComputedStyle(document.documentElement)
        .getPropertyValue(name)
        .trim();
