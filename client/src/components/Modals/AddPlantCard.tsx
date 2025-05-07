import React from "react";
import { Plus } from "lucide-react";

interface Props {
    onClick?: () => void;
}

const AddPlantCard: React.FC<Props> = ({ onClick }) => (
    <button
        onClick={onClick}
        className="flex items-center justify-center rounded-2xl border-dashed border-2 border-muted/40 bg-[var(--color-surface)] bg-card/50 w-48 h-56 hover:border-muted transition-all"
    >
        <Plus className="h-10 w-10 text-muted-foreground" />
    </button>
);

export default AddPlantCard;
