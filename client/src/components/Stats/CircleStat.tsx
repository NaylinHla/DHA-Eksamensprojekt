import React from "react";
import { cssVar } from "../utils/Theme/theme";

export interface CircleStatProps {
    label: string;
    value: number | null;
    unit: string;
    colorToken: string;
}

export const CircleStat: React.FC<CircleStatProps> = ({label, value, unit, colorToken,}) => {
    const color = cssVar(`--color-${colorToken}`);

    return (
        <div className="flex flex-col items-center">
            <div
                className="
          relative aspect-square
          w-[clamp(7rem,9vw,14rem)]
          rounded-full border-[clamp(4px,0.6vw,8px)]
          flex items-center justify-center
        "
                style={{ borderColor: color }}
            >
        <span className="font-bold select-none text-[clamp(1rem,1.6vw,2rem)]">
          {value == null ? "—" : `${value.toFixed(1)}${unit}`}
        </span>
            </div>
            <p className="mt-2 text-sm text-center text-fluid text-[--color-primary]">{label}</p>
        </div>
    );
};