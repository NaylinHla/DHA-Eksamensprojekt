import React, {useState} from "react";
import useAlertsRest, {Alert} from "../../hooks/useAlertsRest.tsx";
import {TitleTimeHeader} from "../../components";

const getYear = (dateString: string) => new Date(dateString).getFullYear();

const AlertOverview = () => {
    const [selectedYear, setSelectedYear] = useState<number | null>(null);
    const {alerts, loading} = useAlertsRest();

    const allYears = [...new Set(alerts.map(a => getYear(a.alertTime)))].sort((a, b) => b - a);

    const filteredAlerts = selectedYear
        ? alerts.filter((a) => getYear(a.alertTime) === selectedYear)
        : alerts;

    return (
        <div className="flex flex-col min-h-screen bg-[--color-background] text-[--color-primary] font-display">
            <TitleTimeHeader title="Alerts Overview"/>
            <div className="flex flex-grow overflow-hidden">
                {/* Sidebar Year Filter */}
                <aside
                    className="bg-base-100 w-32 flex flex-col items-center py-6 px-2 space-y-2 text-sm text-gray-500">
                    {selectedYear && (
                        <button
                            onClick={() => setSelectedYear(null)}
                            className="w-full text-center py-2 px-3 rounded-md text-gray-500 hover:bg-red-100 transition font-semibold"
                            title="Clear year filter"
                        >
                            Clear
                        </button>
                    )}

                    {allYears.map((year) => (
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
                </aside>

                {/* Alerts Feed */}
                <main className="flex-1 overflow-y-auto p-6 space-y-6">
                    {loading ? (
                        <div className="flex justify-center items-center mt-20 text-gray-500">
                            <svg className="animate-spin h-6 w-6 mr-3 text-gray-500" viewBox="0 0 24 24">
                                <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor"
                                        strokeWidth="4" fill="none"/>
                                <path className="opacity-75" fill="currentColor"
                                      d="M4 12a8 8 0 018-8v4a4 4 0 00-4 4H4z"/>
                            </svg>
                            Loading alerts...
                        </div>
                    ) : filteredAlerts.length === 0 ? (
                        <div className="text-gray-400 text-center mt-12">No alerts found.</div>
                    ) : (
                        filteredAlerts.map((alert: Alert, index: number) => (
                            <div key={index}>
                                <div className="text-xs text-gray-500 mb-1">
                                    {new Date(alert.alertTime).toLocaleDateString()}
                                </div>
                                <div className="bg-white text-black p-4 rounded-xl shadow-md whitespace-pre-line">
                                    {alert.alertDesc}
                                </div>
                            </div>
                        ))
                    )}
                </main>
            </div>
        </div>
    );
};

export default AlertOverview;
