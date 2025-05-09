import {useEffect, useRef, useState} from "react";

export default function useDropdown() {
    const [open, setOpen] = useState(false);
    const ref = useRef<HTMLDivElement>(null);

    // close when you click outside
    useEffect(() => {
        if (!open) return;

        function handler(e: MouseEvent) {
            if (!ref.current?.contains(e.target as Node)) setOpen(false);
        }

        document.addEventListener("pointerdown", handler);
        return () => document.removeEventListener("pointerdown", handler);
    }, [open]);

    return {open, setOpen, ref};
}
