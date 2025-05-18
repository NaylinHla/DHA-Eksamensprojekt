import React, { useEffect, useState } from "react";
import { ChevronLeft, ChevronRight } from "lucide-react";
import {PlantCard} from "../index.ts";
import {CardPlant} from "./PlantCard.tsx";

export type PlantStatus = CardPlant & { needsWater: boolean };

interface Props {
    plants: PlantStatus[];
    className?: string;
}

const PlantCarousel: React.FC<Props> = ({ plants, className = "" }) => {
    const getPageSize = () =>
        window.innerWidth >= 1024 ? 3 : window.innerWidth >= 640 ? 2 : 1;

    const [pageSize, setPageSize] = useState(getPageSize());

    useEffect(() => {
        const onResize = () => setPageSize(getPageSize());
        window.addEventListener("resize", onResize);
        return () => window.removeEventListener("resize", onResize);
    }, []);

    const [page, setPage] = useState(0);
    const maxPage = Math.max(0, Math.ceil(plants.length / pageSize) - 1);

    useEffect(() => {
        if (page > maxPage) setPage(maxPage);
    }, [page, maxPage]);

    if (!plants.length) {
        return (
            <div className={`w-full card bg-base-100 shadow flex items-center justify-center ${className}`}>
                <p className="text-gray-500">No Plants Registered!</p>
            </div>
        );
    }

    const start = page * pageSize;
    const visible = plants.slice(start, start + pageSize);

    return (
        <div className={`w-full card rounded-xl bg-[var(--color-surface)] shadow flex flex-col ${className}`}>
            <div className="card-body pb-4">
                <div className="flex items-center gap-4">
                    {/* ← button */}
                    <button
                        onClick={() => setPage(p => Math.max(p - 1, 0))}
                        disabled={page === 0}
                        className="btn btn-sm btn-ghost disabled:opacity-40"
                    >
                        <ChevronLeft/>
                    </button>

                    {/* card(s) */}
                    <div className="flex-1 flex justify-center gap-4">
                        {visible.map(p => (
                            <PlantCard
                                key={p.id}
                                plant={p}
                                showDead
                                hideDelete
                                hideWater
                            />
                        ))}
                    </div>

                    {/* → button */}
                    <button
                        onClick={() => setPage(p => Math.min(p + 1, maxPage))}
                        disabled={page === maxPage}
                        className="btn btn-sm btn-ghost disabled:opacity-40"
                    >
                        <ChevronRight/>
                    </button>
                </div>
            </div>
        </div>
    );
};

export default PlantCarousel;
