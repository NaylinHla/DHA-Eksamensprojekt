import React from "react";
import {Plus} from "lucide-react";

interface Props {
    onClick?: () => void;
}

const AddPlantCard: React.FC<Props> = ({onClick}) => (
    <button
        onClick={onClick}
        className="cursor-pointer card-fluid flex items-center justify-center rounded-2xl border-dashed border-2 border-muted/40 bg-[var(--color-surface)] bg-card/50 hover:border-muted transition-all"
    >
        <Plus className="text-fluid-lg text-muted-foreground"/>
    </button>
);

export default AddPlantCard;
