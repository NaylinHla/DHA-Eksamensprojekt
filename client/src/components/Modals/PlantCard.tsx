import React from "react";
import { Droplet, X } from "lucide-react";
import PlantIcon from "../../assets/Favicon/Plant.svg?react";

export interface CardPlant {
    id: string;
    name: string;
    nextWaterInDays: number;
}

interface PlantCardProps {
    plant: CardPlant;
    onWater?: () => void;
    onClick?: (plant: CardPlant) => void;
}

const PlantCard: React.FC<PlantCardProps> = ({ plant, onWater, onClick }) => {
    const dueText =
        plant.nextWaterInDays === 0
            ? "Today"
            : `In ${plant.nextWaterInDays} day${plant.nextWaterInDays > 1 ? "s" : ""}`;

    return (
        <button
            onClick={() => onClick?.(plant)}
            className="relative flex flex-col justify-between rounded-2xl bg-card bg-[var(--color-surface)] shadow-sm p-3 w-48 h-56 hover:shadow-md transition-shadow"
        >
            {/* Delete ‑ not wired yet */}
            <X className="absolute right-2 top-2 h-4 w-4 text-muted-foreground" />

            {/* placeholder image */}
            <div className="flex-1 flex items-center justify-center">
                <PlantIcon className="h-16 w-16 text-base-content" />
            </div>

            {/* Footer */}
            <div className="flex items-center justify-between text-sm mt-1">
                <span className="font-medium truncate max-w-[60%]">{plant.name}</span>
                <Droplet onClick={e => { e.stopPropagation(); onWater?.(); }}
                         className="h-4 w-4 cursor-pointer text-blue-500"/>
            </div>
            <p className="text-xs text-muted-foreground mt-1">{dueText}</p>
        </button>
    );
};

export default PlantCard;
