import React, {useCallback, useEffect, useMemo, useState} from "react";
import PlantCard, {CardPlant} from "../../components/Modals/PlantCard.tsx";
import AddPlantCard from "../../components/Modals/AddPlantCard.tsx";
import PlantModal from "../../components/Modals/PlantModal.tsx";
import {formatDateTimeForUserTZ} from "../../components";
import {JwtAtom, PlantClient, PlantResponseDto } from "../../atoms";
import {useAtom} from "jotai";
import PlantToolbar from "../../components/Modals/PlantToolbar.tsx";

const plantClient = new PlantClient(
    import.meta.env.VITE_API_URL ?? "http://localhost:5000"
);

const toCard = (dto: PlantResponseDto): CardPlant => {
    const days =
        dto.waterEvery != null && dto.lastWatered
            ? Math.max(
                0,
                dto.waterEvery -
                Math.floor(
                    (Date.now() - new Date(dto.lastWatered).getTime()) / 86_400_000
                )
            )
            : 0;
    return { id: dto.plantId!, name: dto.plantName!, nextWaterInDays: days, isDead: !!dto.isDead };
};

function userIdFromJwt(token: string): string | null {
    try {
        const { sub, Id } = JSON.parse(atob(token.split(".")[1]));
        return (sub || Id) ?? null;
    } catch { return null; }
}

const PlantsView: React.FC = () => {
    const [jwt] = useAtom(JwtAtom);
    const [plants, setPlants] = useState<CardPlant[]>([]);
    const [search, setSearch] = useState("");
    const [loading, setLoading] = useState(true);
    const [err, setErr] = useState<string | null>(null);
    const [now, setNow] = useState(new Date());
    const [modalOpen, setModalOpen]   = useState(false);
    const [selected, setSelected]     = useState<CardPlant | null>(null);
    const [showDead, setShowDead] = useState(false);

    // Clock
    useEffect(() => {
        const id = setInterval(() => setNow(new Date()), 1_000);
        return () => clearInterval(id);
    }, []);

    // Fetch plants
    const fetchPlants = useCallback(async () => {
        if (!jwt) return;
        const uid = userIdFromJwt(jwt);
        if (!uid) { setErr("Invalid token"); return; }

        try {
            setLoading(true);
            const list = await plantClient.getAllPlants(uid, jwt); // <‑‑ raw JWT
            setPlants(list.map(toCard));
            setErr(null);
        } catch (e: any) {
            setErr(e.message ?? "Failed");
        } finally {
            setLoading(false);
        }
    }, [jwt]);

    useEffect(() => { fetchPlants(); }, [fetchPlants]);

    // Water Plants
    const waterAll = async () => {
        try { await plantClient.waterAllPlants(jwt); await fetchPlants(); }
        catch (e:any){ alert(e.message ?? "Failed"); }
    };
    const waterOne = async (id: string) => {
        try { await plantClient.waterPlant(id, jwt); await fetchPlants(); }
        catch (e:any){ alert(e.message ?? "Failed"); }
    };

    
    // Open helpers 
    const openNew   = () => { setSelected(null);   setModalOpen(true); };
    const openDetails  = (p: CardPlant) => { setSelected(p); setModalOpen(true); };
    const closeMod  = () => setModalOpen(false);
    
    // Search filter
    const visible = useMemo(() => {
        const t = search.trim().toLowerCase();

        return plants
            .filter((p) => (showDead ? true : !p.isDead))
            .filter((p) => (t ? p.name.toLowerCase().includes(t) : true));
    }, [plants, search, showDead]);

    if (loading) return <p className="p-6">Loading…</p>;
    if (err)     return <p className="p-6 text-error">{err}</p>;

    return (
        <div className="min-h-[calc(100vh-64px)] flex flex-col font-display">

            {/* header */}
            <header className="w-full bg-[var(--color-surface)] shadow px-6 py-4 flex justify-between">
                <h1 className="text-2xl font-bold">Plants</h1>
                <span className="text-sm text-gray-600">
          {formatDateTimeForUserTZ(now)}
        </span>
            </header>

            {/* content */}
            <main className="flex-1 overflow-y-auto px-6 py-4">
                <PlantToolbar 
                    onSearch={setSearch} 
                    onWaterAll={waterAll}
                    showDead={showDead}
                    onToggleDead={() => setShowDead((d) => !d)}
                />

                <div className="grid gap-6 auto-rows-fr grid-cols-[repeat(auto-fill,minmax(12rem,1fr))]">
                    {visible.map(p => (
                        <PlantCard
                            key={p.id}
                            plant={p}
                            onWater={() => waterOne(p.id)}
                            onClick={openDetails}
                            onRemoved={fetchPlants}
                            showDead={showDead}
                        />
                    ))}
                    <AddPlantCard onClick={openNew} />
                </div>
            </main>

            <PlantModal
                open={modalOpen}
                plant={selected}
                onClose={closeMod}
                onSaved={fetchPlants}
            />
        </div>
    );
};

export default PlantsView;