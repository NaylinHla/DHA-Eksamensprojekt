import React, {useState} from "react";
import {Search} from "lucide-react";


interface Props {
    onSearch: (term: string) => void;
    onWaterAll: () => void;
    showDead: boolean;
    onToggleDead: () => void;
}

const PlantsToolbar: React.FC<Props> = ({onSearch, onWaterAll, showDead, onToggleDead}) => {
    const [term, setTerm] = useState("");

    return (
        <div className="flex flex-col gap-4 lg:flex-row lg:items-center lg:justify-between mb-6">
            <div className="flex flex-col sm:flex-row sm:items-center gap-4">
                {/* Search box */}
                <div className="relative bg-[var(--color-surface)] rounded-2xl w-full sm:w-64">
                    <Search className="absolute h-4 w-4 left-3 top-1/2 -translate-y-1/2 text-muted-foreground"/>
                    <input
                        placeholder="Search"
                        value={term}
                        onChange={(e) => {
                            setTerm(e.target.value);
                            onSearch(e.target.value);
                        }}
                        className="pl-9 w-full"
                    />
                </div>

                <label className="inline-flex items-center gap-2 text-sm">
                    <input
                        type="checkbox"
                        className="checkbox checkbox-xs"
                        checked={showDead}
                        onChange={onToggleDead}
                    />
                    View dead
                </label>
            </div>

            <button
                onClick={onWaterAll}
                className="Btn btn-neutral bg-transparent rounded-2xl self-start lg:self-auto"
            >
                Water all plants
            </button>
        </div>
    );
};

export default PlantsToolbar;