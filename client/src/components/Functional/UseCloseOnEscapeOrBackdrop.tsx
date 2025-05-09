import {useEffect} from "react";

export function UseCloseOnEscapeOrBackdrop(open: boolean, onClose: () => void, backdropRef: React.RefObject<HTMLElement>) {
    useEffect(() => {
        if (!open) return;

        const onKey = (e: KeyboardEvent) => {
            if (e.key === "Escape") onClose();
        };

        const onClick = (e: MouseEvent) => {
            if (e.target === backdropRef.current) onClose();
        };

        window.addEventListener("keydown", onKey);
        backdropRef.current?.addEventListener("click", onClick);

        return () => {
            window.removeEventListener("keydown", onKey);
            backdropRef.current?.removeEventListener("click", onClick);
        };
    }, [open, onClose, backdropRef]);
}

export default UseCloseOnEscapeOrBackdrop;