import React, {useEffect, useMemo, useState} from "react";
import PlantCard, {Plant} from "../../components/Modals/PlantCard.tsx";
import PlantsToolbar from "../../components/Modals/PlantToolbar.tsx";
import AddPlantCard from "../../components/Modals/AddPlantCard.tsx";
import {formatDateTimeForUserTZ} from "../../components";


const mockPlants: Plant[] = [
    { id: "1", name: "Ficus", nextWaterInDays: 0 },
    { id: "2", name: "Dracaena", nextWaterInDays: 3 },
    { id: "3", name: "Tomato", nextWaterInDays: 5 },
    { id: "4", name: "Bonsai", nextWaterInDays: 5 },
    { id: "5", name: "Apple Tree", nextWaterInDays: 5 },
    { id: "6", name: "Azalea", nextWaterInDays: 5 }
];

const PlantsView: React.FC = () => {
    const [plants, setPlants] = useState<Plant[]>(mockPlants);
    const [searchTerm, setSearchTerm] = useState("");
    const [currentTime, setCurrentTime] = useState(new Date());

    const filtered = useMemo(() => {
        const t = searchTerm.trim().toLowerCase();
        return t
            ? plants.filter((p) => p.name.toLowerCase().includes(t))
            : plants;
    }, [plants, searchTerm]);

    useEffect(() => {
        const interval = setInterval(() => {
            setCurrentTime(new Date());
        }, 1000);
        return () => clearInterval(interval);
    }, []);

    return (
        <div className="min-h-[calc(100vh-64px)] flex flex-col bg-[--color-background] text-[--color-primary] font-display">
            {/* Header */}
            <header className="w-full bg-background shadow px-6 py-4 flex justify-between items-center">
                <h1 className="text-2xl font-bold text-[--color-primary]">Plants</h1>
                <span className="text-sm text-gray-600">
                    {formatDateTimeForUserTZ(currentTime)}
                </span>
            </header>
            
            <main className="flex-1 overflow-y-auto px-6 py-4">
                <PlantsToolbar
                    onSearch={setSearchTerm}
                    onWaterAll={() => alert("💧  (hook up the backend here)")}
                />

                {/* Card grid */}
                <div className="grid gap-6 auto-rows-fr grid-cols-[repeat(auto-fill,minmax(12rem,1fr))]">
                    {filtered.map((plant) => (
                        <PlantCard key={plant.id} plant={plant} />
                    ))}

                    {/* Add‑plant tile - Empty for now */}
                    <AddPlantCard onClick={() => {}} />
                </div>
            </main>
        </div>
    );
};

export default PlantsView;
