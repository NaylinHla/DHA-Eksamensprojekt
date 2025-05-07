import React, { useState } from "react";
import { Search, SlidersHorizontal } from "lucide-react";


interface Props {
    onSearch: (term: string) => void;
    onWaterAll: () => void;
}

const PlantsToolbar: React.FC<Props> = ({ onSearch, onWaterAll }) => {
    const [term, setTerm] = useState("");

    return (
        <div className="flex flex-col gap-4 lg:flex-row lg:items-center lg:justify-between mb-6">
            {/* Search */}
            <div className="bg-[var(--color-surface)] relative w-full max-w-xs rounded-2xl">
                <Search className="absolute h-4 w-4 left-3 top-1/2 -translate-y-1/2 text-muted-foreground" />
                <input
                    placeholder="Search"
                    value={term}
                    onChange={(e) => {
                        setTerm(e.target.value);
                        onSearch(e.target.value);
                    }}
                    className="pl-9"
                />
                <SlidersHorizontal className="absolute h-4 w-4 right-3 top-1/2 -translate-y-1/2 text-muted-foreground" />
            </div>

            {/* Water all */}
            <button onClick={onWaterAll} className="Btn Btn-neutral self-start lg:self-auto">
                Water all plants
            </button>
        </div>
    );
};

export default PlantsToolbar;
