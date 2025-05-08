import React, { useEffect, useState } from "react";
import { formatDateTimeForUserTZ } from "../import";

const DashboardPage = () => {
    const [currentTime, setCurrentTime] = useState(new Date());

    useEffect(() => {
        const interval = setInterval(() => {
            setCurrentTime(new Date());
        }, 1000);
        return () => clearInterval(interval);
    }, []);

    return (
        <div
            className="min-h-[calc(100vh-64px)] flex flex-col bg-[--color-background] text-[--color-primary] font-display overflow-hidden">
            {/* Header */}
            <header className="w-full bg-background shadow px-6 py-4 flex justify-between items-center">
                <h1 className="text-2xl font-bold text-[--color-primary]">Dashboard</h1>
                <span className="text-sm text-gray-600">
                    {formatDateTimeForUserTZ(currentTime)}
                </span>
            </header>

            {/* Full-height Loading Spinner */}
            <main className="flex-1 flex items-center justify-center overflow-hidden">
                <div className="flex flex-col items-center text-gray-500">
                    <svg
                        className="animate-spin h-8 w-8 mb-4"
                        viewBox="0 0 24 24"
                    >
                        {/* Outer Circle with gradient animation */}
                        <circle
                            className="opacity-25 animate-gradientSpinner"
                            cx="12"
                            cy="12"
                            r="10"
                            stroke="currentColor"
                            strokeWidth="4"
                            fill="none"
                        />
                        {/* Inner Path (This is the spinning "dash" effect) */}
                        <path
                            className="opacity-75"
                            fill="none"
                            stroke="var(--color-primary)" // Use the primary color for the inner dash
                            strokeWidth="4"
                            d="M4 12a8 8 0 018-8v4a4 4 0 00-4 4H4z"
                        />
                    </svg>
                    <p className="text-[--color-primary]">Loading...</p>
                </div>
            </main>

            {/* Inline Keyframe Animation for Gradient */}
            <style jsx>{`
                @keyframes gradientSpinner {
                    0% {
                        stroke: var(--color-primary); /* Start with primary color */
                    }
                    50% {
                        stroke: #ff00ff; /* A softer color for the transition */
                    }
                    100% {
                        stroke: var(--color-primary); /* Back to primary color */
                    }
                }

                .animate-gradientSpinner {
                    animation: gradientSpinner 3s ease-in-out infinite;
                }
            `}</style>
        </div>
    )
};

export default DashboardPage;
