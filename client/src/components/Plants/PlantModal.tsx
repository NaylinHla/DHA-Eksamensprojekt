import React, {Fragment, useEffect, useRef, useState} from "react";
import {Check, Droplet, Pencil, X,} from "lucide-react";
import type {CardPlant} from "./PlantCard";
import {JwtAtom, PlantClient, PlantCreateDto, PlantEditDto, PlantResponseDto,} from "../../atoms";
import {useAtom} from "jotai";
import {format} from "date-fns";
import toast from "react-hot-toast";

const plantClient = new PlantClient(
    import.meta.env.VITE_API_URL ?? "http://localhost:5000"
);

interface Props {
    open: boolean;
    plant: CardPlant | null;
    onClose: () => void;
    onSaved: () => void;
}

const toEditDto = (p: PlantResponseDto): PlantEditDto => ({
    plantName: p.plantName ?? "",
    plantType: p.plantType ?? "",
    plantNotes: p.plantNotes ?? "",
    planted: p.planted,
    lastWatered: p.lastWatered,
    waterEvery: p.waterEvery,
    isDead: p.isDead,
});

const emptyCreate: PlantCreateDto = {
    plantName: "",
    plantType: "",
    plantNotes: "",
    planted: new Date(),
    waterEvery: 7,
    isDead: false,
};

const PlantModal: React.FC<Props> = ({open, plant, onClose, onSaved}) => {
    const [jwt] = useAtom(JwtAtom);
    const [isEditing, setEditing] = useState(false);

    // Form State
    const [data, setData] = useState<PlantEditDto>(emptyCreate);
    const [full, setFull] = useState<PlantResponseDto | null>(null);
    const [saving, setSaving] = useState(false);
    const [loading, setLoading] = useState(false);

    const backdrop = useRef<HTMLDivElement>(null);

    // fetch selected plant into card
    useEffect(() => {
        if (!open || !plant) return;

        const run = async () => {
            try {
                setLoading(true);
                const p = await plantClient.getPlant(plant.id, jwt);
                setFull(p);
                setData(toEditDto(p));
            } finally {
                setLoading(false);
            }
        };
        run();
    }, [open, plant, jwt]);

    useEffect(() => {
        if (open) {
            setEditing(plant === null);
        }
    }, [open, plant]);

    // Reset state when off modal
    useEffect(() => {
        if (!open) {
            setEditing(plant === null);
            setFull(null);
            setData(emptyCreate);
        }
    }, [open, plant]);

    // Close modal on outside click or esc
    useEffect(() => {
        if (!open) return;
        const onKey = (e: KeyboardEvent) => e.key === "Escape" && onClose();
        const onClick = (e: MouseEvent) =>
            e.target === backdrop.current && onClose();
        window.addEventListener("keydown", onKey);
        backdrop.current?.addEventListener("click", onClick);
        return () => {
            window.removeEventListener("keydown", onKey);
            backdrop.current?.removeEventListener("click", onClick);
        };
    }, [open, onClose]);

    // Helpers
    const upd = <K extends keyof PlantEditDto>(k: K, v: PlantEditDto[K]) =>
        setData((d) => ({...d, [k]: v}));

    // Handlers
    const save = async () => {
        try {
            setSaving(true);

            /* ────────────────── EDIT EXISTING ────────────────── */
            if (plant) {
                await plantClient.editPlant(plant.id, data, jwt);

                onSaved();

                const latest = await plantClient.getPlant(plant.id, jwt);
                setFull(latest);
                setData(toEditDto(latest));

                setEditing(false);
                toast.success("Plant updated");
            }

            // Create New
            else {
                const newDto = {
                    ...data,
                    planted: data.planted ?? new Date()
                };

                await plantClient.createPlant(newDto, jwt);
                onSaved();

                toast.success("Plant added 🌱");
                onClose();
            }
        } catch (err: any) {
            toast.error(err?.message ?? "Something went wrong");
        } finally {
            setSaving(false);
        }
    };

    const waterNow = async () => {
        if (!plant) return;
        try {
            setSaving(true);
            await plantClient.waterPlant(plant.id, jwt);
            onSaved();
            setFull((p) =>
                p ? {...p, lastWatered: new Date()} : p
            );
        } finally {
            setSaving(false);
        }
    };

    const goBack = () => {
        if (full) setData(toEditDto(full));
        setEditing(false);
    };

    const Pill: React.FC<{ children: React.ReactNode; className?: string }> = ({children, className = "",}) => (
        <div className={`rounded-xl px-4 py-2 shadow-sm bg-[var(--color-surface)] ${className}`}>
            {children}
        </div>
    );

    const renderDetails = () => {
        if (loading || !full) return <p>Loading…</p>;
        return (
            <Fragment>
                <Pill className="flex justify-between items-center text-fluid">
                    <span>{full.plantName}</span>
                </Pill>

                {full.plantType && <Pill className="text-fluid">Plant Type: {full.plantType}</Pill>}

                <Pill className="flex justify-between items-center text-fluid">
                  <span>
                    Last watered:{" "}
                      {full.lastWatered
                          ? format(new Date(full.lastWatered), "dd/MM/yyyy")
                          : "never"}
                  </span>
                    <button onClick={waterNow} title="Water now">
                        <Droplet size={18} className="text-blue-500"/>
                    </button>
                </Pill>

                {full.planted && (
                    <Pill className="text-fluid">Planted: {format(new Date(full.planted), "dd/MM/yyyy")}</Pill>
                )}

                {full.waterEvery && (
                    <Pill className="text-fluid">Water every: {full.waterEvery} days</Pill>
                )}

                {full.plantNotes && <Pill className="text-fluid">Notes: {full.plantNotes}</Pill>}
            </Fragment>
        );
    };

    const renderEdit = () => (
        <Fragment>
            <label className="text-fluid flex flex-col gap-1">
                <span>Name:</span>
                <input
                    placeholder="Plant Name"
                    value={data.plantName}
                    onChange={(e) => upd("plantName", e.target.value)}
                    className="text-fluid rounded-xl px-4 py-1 shadow-sm bg-[var(--color-surface)]"
                />
            </label>

            <label className="text-fluid flex flex-col gap-1">
                <span>Type / species:</span>
                <input
                    placeholder="Plant Type"
                    value={data.plantType ?? ""}
                    onChange={(e) => upd("plantType", e.target.value)}
                    className="text-fluid rounded-xl px-4 py-1 shadow-sm bg-[var(--color-surface)]"
                />
            </label>

            <label className="text-fluid flex flex-col gap-1">
                <span>Water every (days):</span>
                <input
                    type="number"
                    min={1}
                    value={data.waterEvery ?? ""}
                    onChange={(e) => upd("waterEvery", Number(e.target.value))}
                    className="text-fluid text-fluid rounded-xl px-4 py-1 shadow-sm bg-[var(--color-surface)]"
                />
            </label>

            <label className="text-fluid flex flex-col gap-1">
                <span>Notes</span>
                <textarea
                    placeholder="Notes go here"
                    rows={3}
                    value={data.plantNotes ?? ""}
                    onChange={(e) => upd("plantNotes", e.target.value)}
                    className="text-fluid rounded-xl px-4 py-1 shadow-sm bg-[var(--color-surface)]"
                />
            </label>
        </Fragment>
    );

    if (!open) return null;
    return (
        <div
            ref={backdrop}
            className="fixed inset-0 bg-black/40 flex items-center justify-center z-50 px-4"
        >
            <div className="bg-[var(--color-cream)] rounded-2xl w-full max-w-md p-6 relative flex flex-col gap-4">
                <button
                    className="absolute right-4 top-4 text-muted-foreground"
                    onClick={onClose}
                >
                    <X size={20}/>
                </button>

                {/* Title */}
                <h2 className="font-semibold text-fluid">
                    {plant
                        ? isEditing
                            ? "Edit plant"
                            : "Plant details"
                        : "Add plant"}
                </h2>

                {/* Details / Edit */}
                <div className="flex flex-col gap-4 overflow-y-auto max-h-[70vh] pr-1">
                    {isEditing ? renderEdit() : renderDetails()}
                </div>

                {/* Footer */}
                <div className="flex justify-between pt-4">
                    {isEditing ? (
                        <>
                            {plant && (
                                <button
                                    className="btn border-neutral flex items-center gap-1"
                                    onClick={goBack}
                                    disabled={saving}
                                >
                                    Go back
                                </button>
                            )}

                            <button
                                className="btn btn-primary ml-auto flex items-center gap-1"
                                onClick={save}
                                disabled={saving}
                            >
                                <Check size={14}/>
                                {saving ? "Saving…" : "Save"}
                            </button>
                        </>
                    ) : (
                        <button
                            className="btn btn-primary ml-auto flex items-center gap-1"
                            onClick={() => setEditing(true)}
                        >
                            <Pencil size={14}/>
                            Edit
                        </button>
                    )}
                </div>
            </div>
        </div>
    );
};

export default PlantModal;
