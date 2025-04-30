import React, { useState, forwardRef } from "react";
import { Eye, EyeOff } from "lucide-react";

export const PasswordField = forwardRef<
    HTMLInputElement,
    React.InputHTMLAttributes<HTMLInputElement>
>(({ className = "", ...props }, ref) => {
    const [visible, setVisible] = useState(false);

    return (
        <div className="relative">
            <input
                {...props}
                ref={ref}
                type={visible ? "text" : "password"}
                className={`input input-bordered input-sm w-full pr-10 text-black ${className}`}
            />

            {/* eye button */}
            <button
                type="button"
                aria-label={visible ? "Hide password" : "Show password"}
                onClick={() => setVisible(v => !v)}
                className="absolute inset-y-0 right-0 flex items-center px-2 z-20 text-black/40 hover:text-black"
                tabIndex={-1}
            >
                {visible ? <EyeOff size={16} /> : <Eye size={16} />}
            </button>
        </div>
    );
});
PasswordField.displayName = "PasswordInput";
