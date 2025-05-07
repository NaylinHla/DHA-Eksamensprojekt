import { useRef } from "react";

export function useThrottle<T extends any[]>(fn: (...args: T) => void, delay: number) {
    const last = useRef(0);
    return (...args: T) => {
        const now = Date.now();
        if (now - last.current > delay) {
            last.current = now;
            fn(...args);
        }
    };
}

