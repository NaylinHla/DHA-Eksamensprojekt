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
    const livePlants = plants.filter(p => !p.isDead);

    const [pageSize, setPageSize] = useState(1);
    const pageSizeSetter = useRef(setPageSize);
    pageSizeSetter.current = setPageSize;
    const resizeObsRef = useRef<ResizeObserver>();

    const setContainerRef = useCallback((node: HTMLDivElement | null) => {
        resizeObsRef.current?.disconnect();
        resizeObsRef.current = undefined;
        if (!node) return;
        resizeObsRef.current = new ResizeObserver(entries => {
            if (!entries.length) return;
            const { width } = entries[0].contentRect;
            const rawCount = Math.floor((width + H_GAP_PX) / CARD_FULL_PX);
            const cards = Math.min(4, Math.max(1, rawCount));
            pageSizeSetter.current(cards);
        });
        resizeObsRef.current.observe(node);
    }, []);

    const [page, setPage] = useState(0);
    const maxPage = Math.max(0, Math.ceil(livePlants.length / pageSize) - 1);

    useEffect(() => {
        if (page > maxPage) setPage(maxPage);
    }, [maxPage, page]);

    if (!livePlants.length) {
        return (
            <div className={`w-full card bg-[var(--color-surface)] shadow flex items-center justify-center ${className}`}>
                <p className="text-[--color-primary]">No Plants Registered!</p>
            </div>
        );
    }

    const start = page * pageSize;
    const visible = livePlants.slice(start, start + pageSize);

    return (
        <div
            ref={setContainerRef}
            className={`w-full card rounded-xl bg-[var(--color-surface)] shadow flex flex-col ${className}`}
        >
            <h2 className="text-fluid-header text-center">Your Plants:</h2>
            <div className="card-body p-fluid pb-0">
                <div className="flex items-center gap-fluid">
                    <button
                        onClick={() => setPage(p => Math.max(p - 1, 0))}
                        disabled={page === 0}
                        className="btn btn-sm btn-ghost disabled:opacity-40 text-fluid px-fluid py-fluid"
                    >
                        <ChevronLeft />
                    </button>

                    <div className="flex-1 flex justify-center gap-fluid flex-wrap">
                        {visible.map(p => (
                            <PlantCard
                                key={p.id}
                                plant={p}
                                hideDelete
                                hideWater
                            />
                        ))}
                    </div>

                    <button
                        onClick={() => setPage(p => Math.min(p + 1, maxPage))}
                        disabled={page === maxPage}
                        className="btn btn-sm btn-ghost disabled:opacity-40 text-fluid px-fluid py-fluid"
                    >
                        <ChevronRight />
                    </button>
                </div>
            </div>
        </div>
    );
};

export default PlantCarousel;
