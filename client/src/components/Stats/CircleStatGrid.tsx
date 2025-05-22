import React from "react";

export const CircleStatGrid: React.FC<React.PropsWithChildren> = ({ children }) => (
    <div
        className="
      grid gap-x-8 gap-y-6
      sm:grid-cols-[repeat(auto-fit,minmax(11rem,1fr))]
      place-items-center
    "
    >
        {children}
    </div>
);
