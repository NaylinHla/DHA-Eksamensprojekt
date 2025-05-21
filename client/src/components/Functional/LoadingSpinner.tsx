// src/components/LoadingSpinner.tsx
import React from "react";

const LoadingSpinner = () => (
    <div className="flex justify-center items-center h-32">
        <svg className="animate-spin h-8 w-8 mr-3 text-gray-500" viewBox="0 0 24 24">
            <circle
                cx="12"
                cy="12"
                r="10"
                stroke="currentColor"
                strokeWidth="4"
                fill="none"
                className="opacity-25"
            />
            <path
                fill="currentColor"
                d="M4 12a8 8 0 018-8v4a4 4 0 00-4 4H4z"
                className="opacity-75"
            />
        </svg>
        <span className="text-gray-500">Loading…</span>
    </div>
);

export default LoadingSpinner;
