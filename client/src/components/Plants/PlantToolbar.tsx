import React, {useState} from "react";
import { SearchBar, ConfirmModal } from "../index.ts";
import {UserSettingsAtom} from "../../atoms";
import {useAtom} from "jotai";

interface Props {
    onSearch: (term: string) => void;
    onWaterAll: () => void;
    showDead: boolean;
    onToggleDead: () => void;
}

const PlantsToolbar: React.FC<Props> = ({ onSearch, onWaterAll, showDead, onToggleDead }) => {
    const [searchTerm, setSearchTerm] = useState("");
    const [userSettings] = useAtom(UserSettingsAtom);

    const [confirmOpen, setConfirmOpen] = useState(false);
    const [loading, setLoading] = useState(false);

    const handleWaterAllClick = () => {
        if (userSettings?.confirmDialog) {
            setConfirmOpen(true);
        } else {
            onWaterAll();
        }
    };

    const handleConfirm = () => {
        setLoading(true);
        setConfirmOpen(false);
        onWaterAll();
        setLoading(false);
    };

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
                onClick={handleWaterAllClick}
                className="btn border-neutral bg-transparent btn-lg hover:text-white hover:bg-neutral self-start lg:self-auto"
            >
                Water all plants
            </button>

            <ConfirmModal
                isOpen={confirmOpen}
                title="Water All Plants"
                subtitle="This action will water all your plants. Are you sure?"
                confirmVariant="primary"
                loading={loading}
                onCancel={() => setConfirmOpen(false)}
                onConfirm={handleConfirm}
            />
        </div>
    );
};

export default PlantsToolbar;