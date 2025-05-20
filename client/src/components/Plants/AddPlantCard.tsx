import React from "react";
import {Plus} from "lucide-react";

interface Props {
    onClick?: () => void;
}

const AddPlantCard: React.FC<Props> = ({onClick}) => (
    <button
        onClick={onClick}
        className="cursor-pointer flex items-center justify-center rounded-2xl border-dashed border-2 border-muted/40 bg-[var(--color-surface)] bg-card/50 hover:border-muted transition-all w-[clamp(10rem,14vw,18rem)] h-[clamp(14rem,20vw,22rem)] p-fluid"
    >
        <Plus className="text-fluid-lg text-muted-foreground"/>
    </button>
);

export default AddPlantCard;
