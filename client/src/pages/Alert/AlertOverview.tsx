import React, { useState } from "react";
import useAlertsRest, { Alert } from "../hooks/useAlertsRest.tsx";

const getYear = (dateString: string) => new Date(dateString).getFullYear();

const AlertOverview = () => {
    const [selectedYear, setSelectedYear] = useState<number | null>(null);
    const { alerts } = useAlertsRest(selectedYear);

    const years = [...new Set(alerts.map(a => new Date(a.alertTime).getFullYear()))].sort((a, b) => b - a);

    const filteredAlerts = selectedYear
        ? alerts.filter((a: { alertTime: string; }) => getYear(a.alertTime) === selectedYear)
        : alerts;

    return (
        <div className="flex flex-col min-h-screen bg-[--color-background] text-[--color-primary] font-display">
            <div className="flex flex-grow overflow-hidden">
                {/* Sidebar Year Filter */}
                <aside
                    className="bg-base-100 w-32 flex flex-col items-center py-6 px-2 space-y-2 text-sm text-gray-500"
                >
                    {years.map((year) => (
                        <button
                            key={year}
                            onClick={() => setSelectedYear(year)}
                            className={`w-full text-center py-2 px-3 rounded-md hover:bg-gray-100 transition ${
                                selectedYear === year ? "text-[--color-primary] font-semibold bg-gray-200" : ""
                            }`}
                        >
                            {year}
                        </button>
                    ))}
                    {selectedYear && (
                        <button
                            onClick={() => setSelectedYear(null)}
                            className="w-full text-center py-2 px-3 rounded-md text-gray-500 lg:text-3xl hover:bg-red-100 mt-4 transition"
                            title="Show all years"
                        >
                            Year
                        </button>
                    )}
                </aside>


                {/* Alerts Feed */}
                <main className="flex-1 overflow-y-auto p-6 space-y-6">
                    {filteredAlerts.map((alert: Alert, index: number) => (
                        <div key={index}>
                            <div className="text-xs text-gray-500 mb-1">
                                {new Date(alert.alertTime).toLocaleDateString()}
                            </div>
                            <div className="bg-white text-black p-4 rounded-xl shadow-md whitespace-pre-line">
                                {alert.alertDesc}
                            </div>
                        </div>
                    ))}
                </main>
            </div>
        </div>
    );
};

export default AlertOverview;
