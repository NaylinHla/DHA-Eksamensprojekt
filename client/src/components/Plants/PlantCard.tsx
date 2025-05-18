import React, {useState} from "react";
import {Droplet, X} from "lucide-react";
import PlantIcon from "../../assets/Favicon/Plant.svg?react";
import {JwtAtom, PlantClient} from "../../atoms";
import {useAtom} from "jotai";
import toast from "react-hot-toast";
import {ConfirmModal} from "../index.ts";


const plantClient = new PlantClient(
    import.meta.env.VITE_API_URL ?? "http://localhost:5000"
);


export interface CardPlant {
    id: string;
    name: string;
    nextWaterInDays: number;
    isDead: boolean;
}

interface PlantCardProps {
    plant: CardPlant;
    onWater?: () => void;
    onClick?: (plant: CardPlant) => void;
    onRemoved?: () => void;
    showDead?: boolean;
    hideDelete?: boolean;
    hideWater?: boolean;
}

const PlantCard: React.FC<PlantCardProps> = ({plant, onWater, onClick, onRemoved, showDead = false, hideDelete = false, hideWater = false}) => {
    const [jwt] = useAtom(JwtAtom);

    const [confirmOpen, setConfirmOpen] = useState(false);
    const openConfirm = (e: React.MouseEvent) => {
        e.stopPropagation();
        setConfirmOpen(true);
    };
    const closeConfirm = () => setConfirmOpen(false);

    const confirmDelete = async () => {
        try {
            await plantClient.markPlantAsDead(plant.id, jwt);
            toast.success("Plant removed 🌿");
            onRemoved?.();
        } catch (err: any) {
            toast.error(err?.message ?? "Could not remove plant");
        } finally {
            closeConfirm();
        }
    };

    if (plant.isDead && !showDead) return null;
    const deadStyle = plant.isDead ? "opacity-50" : "";

    const dueText =
        plant.nextWaterInDays === 0
            ? "Today"
            : `In ${plant.nextWaterInDays} day${plant.nextWaterInDays > 1 ? "s" : ""}`;

    return (
        <>
            <button
                onClick={() => onClick?.(plant)}
                className={`
          relative flex flex-col justify-between rounded-2xl border shadow-sm p-3
          bg-[var(--color-surface)] hover:shadow-md transition-shadow card-fluid]
          ${deadStyle}
        `}
            >
                {/* delete */}
                {!hideDelete && (
                    <X
                        onClick={openConfirm}
                        className="absolute right-2 top-2 h-4 w-4 text-muted-foreground"
                    />
                )}

                {/* placeholder / image */}
                <div className="flex-1 flex items-center justify-center">
                    <PlantIcon className="h-[clamp(3rem,4.5vw,5rem)] w-auto text-base-content" />
                </div>

                {/* footer */}
                <div className="flex items-center justify-between text-sm mt-1">
          <span className="font-medium truncate max-w-[60%] text-[clamp(.8rem,1vw,1rem)]">
            {plant.name}
          </span>

                    {!hideWater && (
                        <Droplet
                            onClick={(e) => {
                                e.stopPropagation();
                                onWater?.();
                            }}
                            className="h-4 w-4 cursor-pointer text-blue-500"
                        />
                    )}
                </div>
                <p className="text-[clamp(.6rem,.9vw,.75rem)] text-muted-foreground mt-1">
                    {dueText}
                </p>
            </button>

            <ConfirmModal
                isOpen={confirmOpen}
                title="Remove plant?"
                subtitle={`“${plant.name}” will be moved to dead plants`}
                confirmVariant="error"
                onConfirm={confirmDelete}
                onCancel={() => setConfirmOpen(false)}
            />
        </>
    );
};

export default PlantCard;
