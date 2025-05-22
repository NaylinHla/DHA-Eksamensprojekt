import React, {useState} from "react";
import {SearchBar} from "../index.ts";


interface Props {
    onSearch: (term: string) => void;
    onWaterAll: () => void;
    showDead: boolean;
    onToggleDead: () => void;
}

const PlantsToolbar: React.FC<Props> = ({ onSearch, onWaterAll, showDead, onToggleDead }) => {
    const [searchTerm, setSearchTerm] = useState("");

    return (
        <div className="flex flex-col gap-4 lg:flex-row lg:items-center lg:justify-between mb-6">
            <div className="flex flex-col sm:flex-row sm:items-center gap-4">
                {/* Search box */}
                <div className="flex flex-wrap justify-between items-center gap-4">
                    <SearchBar 
                        searchTerm={searchTerm} 
                        onSearch={term => {
                            setSearchTerm(term);
                            onSearch(term);
                        }}
                        />
                </div>

                <label className="inline-flex items-center gap-2 text-[clamp(0.8rem,1vw,1.25rem)]">
                    <input
                        type="checkbox"
                        className="checkbox"
                        checked={showDead}
                        onChange={onToggleDead}
                    />
                    View dead
                </label>
            </div>

            <button
                onClick={onWaterAll}
                className="btn border-neutral bg-transparent btn-lg hover:text-white hover:bg-neutral self-start lg:self-auto"
            >
                Water all plants
            </button>
        </div>
    );
};

export default PlantsToolbar;