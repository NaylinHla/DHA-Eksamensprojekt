import React, {useEffect, useState} from "react";

// Your multipliers map
const intervalMultipliers = {
    Second: 1,
    Minute: 60,
    Hour: 3600,
    Days: 86400,
    Week: 604800,
    Month: 2592000,
} as const;
type Unit = keyof typeof intervalMultipliers;

interface IntervalSelectorProps {
    /** Pass in the raw seconds; the component will pick the best unit/value */
    totalSeconds: number;
    /** Called any time user edits */
    onChange: (newTotalSeconds: number) => void;
}

const IntervalSelector: React.FC<IntervalSelectorProps> = ({
                                                               totalSeconds,
                                                               onChange,
                                                           }) => {
    const [value, setValue] = useState(1);
    const [unit, setUnit] = useState<Unit>("Second");

    // Whenever totalSeconds changes from above, recalculate unit/value
    useEffect(() => {
        let secs = totalSeconds;
        let chosenUnit: Unit = "Second";
        let chosenValue = secs;

        // Try largest multipliers first
        (Object.entries(intervalMultipliers) as [Unit, number][])
            .sort((a, b) => b[1] - a[1])
            .some(([u, mult]) => {
                if (secs >= mult && secs % mult === 0) {
                    chosenUnit = u;
                    chosenValue = secs / mult;
                    return true;
                }
                return false;
            });

        setUnit(chosenUnit);
        setValue(chosenValue);
    }, [totalSeconds]);

    const handleValue = (e: React.ChangeEvent<HTMLInputElement>) => {
        const v = Math.max(1, parseInt(e.target.value, 10) || 1);
        setValue(v);
        onChange(v * intervalMultipliers[unit]);
    };

    const handleUnit = (e: React.ChangeEvent<HTMLSelectElement>) => {
        const u = e.target.value as Unit;
        setUnit(u);
        onChange(value * intervalMultipliers[u]);
    };

    return (
        <div className="flex gap-2 mb-3">
            <input
                type="number"
                min={1}
                value={value}
                onChange={handleValue}
                className="border rounded px-2 w-16 text-sm"
            />
            <select
                value={unit}
                onChange={handleUnit}
                className="border rounded px-2 text-sm"
            >
                {(Object.keys(intervalMultipliers) as Unit[]).map((u) => (
                    <option key={u} value={u}>
                        {u}
                    </option>
                ))}
            </select>
        </div>
    );
};

export default IntervalSelector;
