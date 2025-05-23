import React from "react";

export const CircleStatGrid: React.FC<React.PropsWithChildren> = ({ children }) => (
    <div
        className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-x-6 gap-y-6 w-full max-w-full place-items-center"
    >
        {children}
    </div>
);
