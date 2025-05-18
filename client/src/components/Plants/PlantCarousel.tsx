import React, { useCallback, useEffect, useRef, useState } from "react";
import { ChevronLeft, ChevronRight } from "lucide-react";
import { PlantCard } from "../index.ts";
import { CardPlant } from "./PlantCard.tsx";

export type PlantStatus = CardPlant & { needsWater: boolean };

interface Props {
    plants: PlantStatus[];
    className?: string;
}

const CARD_FULL_PX = 300;
const H_GAP_PX = 16;

const PlantCarousel: React.FC<Props> = ({ plants, className = "" }) => {
    const [pageSize, setPageSize] = useState(1);
    
    const pageSizeSetter = useRef(setPageSize);
    pageSizeSetter.current = setPageSize;
    
    const resizeObsRef = useRef<ResizeObserver>();
    
    const setContainerRef = useCallback((node: HTMLDivElement | null) => {
        // Clean up any existing observer
        resizeObsRef.current?.disconnect();
        resizeObsRef.current = undefined;

        if (!node) return; // Component is unmounting

        resizeObsRef.current = new ResizeObserver((entries) => {
            if (!entries.length) return;
            const { width } = entries[0].contentRect;
            const cards = Math.max(
                1,
                Math.floor((width + H_GAP_PX) / CARD_FULL_PX),
            );
            pageSizeSetter.current(cards);
        });

        resizeObsRef.current.observe(node);
    }, []);
    
    const [page, setPage] = useState(0);
    const maxPage = Math.max(0, Math.ceil(plants.length / pageSize) - 1);
    
    useEffect(() => {
        if (page > maxPage) setPage(maxPage);
    }, [maxPage, page]);
    
    if (!plants.length) {
        return (
            <div
                className={`w-full card bg-base-100 shadow flex items-center justify-center ${className}`}
            >
                <p className="text-gray-500">No Plants Registered!</p>
            </div>
        );
    }
    
    const start = page * pageSize;
    const visible = plants.slice(start, start + pageSize);

    return (
        <div
            ref={setContainerRef}
            className={`w-full card rounded-xl bg-[var(--color-surface)] shadow flex flex-col ${className}`}
        >
            <div className="card-body pb-4">
                <div className="flex items-center gap-4">
                    <button
                        onClick={() => setPage((p) => Math.max(p - 1, 0))}
                        disabled={page === 0}
                        className="btn btn-sm btn-ghost disabled:opacity-40"
                    >
                        <ChevronLeft />
                    </button>

                    {/* plant cards */}
                    <div className="flex-1 flex justify-center gap-4 flex-wrap">
                        {visible.map((p) => (
                            <PlantCard
                                key={p.id}
                                plant={p}
                                showDead
                                hideDelete
                                hideWater
                            />
                        ))}
                    </div>
                    
                    <button
                        onClick={() => setPage((p) => Math.min(p + 1, maxPage))}
                        disabled={page === maxPage}
                        className="btn btn-sm btn-ghost disabled:opacity-40"
                    >
                        <ChevronRight />
                    </button>
                </div>
            </div>
        </div>
    );
};

export default PlantCarousel;
