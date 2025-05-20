import React from "react";

export interface StatCardProps {
    title: string;
    loading: boolean;
    value: string;
    emphasisClass?: string;
    cls: string;
}

export const StatCard: React.FC<StatCardProps> = ({title, loading, value, emphasisClass = "",}) => (
    <div className="card shadow rounded-xl bg-[var(--color-surface)]">
        <div className="card-body text-center space-y-1 ">
            <p className="text-fluid-header">{title}</p>
            <p
                className={`
          font-bold
          text-[clamp(2.5rem,2.5vw,5rem)]
          ${emphasisClass}
        `}
            >
                {loading ? "–" : value}
            </p>
        </div>
    </div>
);