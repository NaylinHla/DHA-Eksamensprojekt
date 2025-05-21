import React, {useCallback, useEffect, useMemo, useState} from "react";
import {AddPlantCard, PlantCard, PlantModal, PlantToolbar, TitleTimeHeader} from "../../components";
import {JwtAtom, PlantResponseDto, UserIdAtom, UserSettingsAtom} from "../../atoms";
import {useAtom} from "jotai";
import {plantClient} from "../../apiControllerClients.ts";
import {CardPlant} from "../../components/Plants/PlantCard.tsx";
import ConfirmModal from "../../components/Modals/ConfirmModal";


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
    return {id: dto.plantId!, name: dto.plantName!, nextWaterInDays: days, isDead: !!dto.isDead};
};

const PlantsView: React.FC = () => {
    const [jwt] = useAtom(JwtAtom);
    const [userId] = useAtom(UserIdAtom);
    const [plants, setPlants] = useState<CardPlant[]>([]);
    const [search, setSearch] = useState("");
    const [loading, setLoading] = useState(true);
    const [err, setErr] = useState<string | null>(null);
    const [modalOpen, setModalOpen] = useState(false);
    const [selected, setSelected] = useState<CardPlant | null>(null);
    const [showDead, setShowDead] = useState(false);

    const [userSettings] = useAtom(UserSettingsAtom);
    const [confirmModalOpen, setConfirmModalOpen] = useState(false);
    const [pendingWaterId, setPendingWaterId] = useState<string | null>(null);
    const [confirmLoading, setConfirmLoading] = useState(false);


    // Fetch plants
    const fetchPlants = useCallback(async () => {
        try {
            setLoading(true);
            const list = await plantClient.getAllPlants(userId, jwt); // <‑‑ raw JWT
            setPlants(list.map(toCard));
            setErr(null);
        } catch (e: any) {
            setErr(e.message ?? "Failed");
        } finally {
            setLoading(false);
        }
    }, [jwt]);

    useEffect(() => {
        fetchPlants();
    }, [fetchPlants]);

    // Water Plants
    const waterAll = async () => {
        try {
            await plantClient.waterAllPlants(userId, jwt);
            await fetchPlants();
        } catch (e: any) {
            alert(e.message ?? "Failed");
        }
    };

    const waterOne = async (id: string) => {
        if (userSettings?.confirmDialog) {
            setPendingWaterId(id);
            setConfirmModalOpen(true);
            return;
        }

        try {
            await plantClient.waterPlant(id, jwt);
            await fetchPlants();
        } catch (e: any) {
            alert(e.message ?? "Failed");
        }
    };


    // Confirm modal
    const confirmWatering = async () => {
        if (!pendingWaterId) return;
        setConfirmLoading(true);

        try {
            await plantClient.waterPlant(pendingWaterId, jwt);
            await fetchPlants();
        } catch (e: any) {
            alert(e.message ?? "Failed");
        } finally {
            setConfirmLoading(false);
            setPendingWaterId(null);
            setConfirmModalOpen(false);
        }
    };

    // Open helpers 
    const openNew = () => {
        setSelected(null);
        setModalOpen(true);
    };
    const openDetails = (p: CardPlant) => {
        setSelected(p);
        setModalOpen(true);
    };
    const closeMod = () => setModalOpen(false);

    // Search filter
    const visible = useMemo(() => {
        const t = search.trim().toLowerCase();

        return plants
            .filter((p) => (showDead ? true : !p.isDead))
            .filter((p) => (t ? p.name.toLowerCase().includes(t) : true));
    }, [plants, search, showDead]);

    if (loading) return <p className="p-6">Loading…</p>;
    if (err) return <p className="p-6 text-error">{err}</p>;

    return (
        <div className="min-h-[calc(100vh-64px)] flex flex-col font-display">

            {/* header */}
            <TitleTimeHeader title="Plants"/>

            {/* content */}
            <main className="flex-1 overflow-y-auto px-6 py-4">
                <PlantToolbar
                    onSearch={setSearch}
                    onWaterAll={waterAll}
                    showDead={showDead}
                    onToggleDead={() => setShowDead((d) => !d)}
                />

                <div
                    className="grid auto-rows-fr gap-fluid-lg grid-cols-[repeat(auto-fill,minmax(clamp(12rem,20vw,16rem),1fr))]">
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
                    <AddPlantCard onClick={openNew}/>
                </div>
            </main>

            <PlantModal
                open={modalOpen}
                plant={selected}
                onClose={closeMod}
                onSaved={fetchPlants}
            />

            <ConfirmModal
                isOpen={confirmModalOpen}
                title="Confirm Watering"
                subtitle="Do you really want to water this plant now?"
                onCancel={() => setConfirmModalOpen(false)}
                onConfirm={confirmWatering}
                loading={confirmLoading}
            />
        </div>
    );
};

export default PlantsView;