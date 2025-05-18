import React, {useState} from "react";
import useAlertsRest, {Alert} from "../../hooks/useAlertsRest.tsx";
import {formatDateTimeForUserTZ, TitleTimeHeader} from "../import";

const getYear = (dateString: string) => new Date(dateString).getFullYear();

const AlertOverview = () => {
    const [selectedYear, setSelectedYear] = useState<number | null>(null);
    const {alerts, loading} = useAlertsRest();

    const allYears = [...new Set(alerts.map(a => getYear(a.alertTime)))].sort((a, b) => b - a);

    const filteredAlerts = selectedYear
        ? alerts.filter((a) => getYear(a.alertTime) === selectedYear)
        : alerts;

    // Handler for alert click (check if it's related to a plant or a device)
    const handleAlertClick = (alert: Alert) => {
        if (alert.alertPlantId) {
            console.log(`Alert related to Plant with ID: ${alert.alertPlantId}`);
        } else if (alert.alertUserDeviceId) {
            console.log(`Alert related to Device with ID: ${alert.alertUserDeviceId}`);
        } else {
            console.log(`Alert ID: ${alert.alertId}`);
        }
    };

    return (
        <div className="flex flex-col min-h-screen bg-[--color-background] text-[--color-primary] font-display">
            <TitleTimeHeader title="Alerts Overview"/>
            <div className="flex flex-grow overflow-hidden">
                {/* Sidebar Year Filter */}
                <aside
                    className="w-[clamp(8rem,15vw,12rem)] flex flex-col items-center p-fluid space-y-2 text-fluid">
                    {selectedYear && (
                        <button
                            onClick={() => setSelectedYear(null)}
                            className="w-full text-center py-2 px-3 rounded-md text-gray-500 hover:text-white hover:bg-neutral transition font-semibold"
                            title="Clear year filter"
                        >
                            Clear
                        </button>
                    )}

                    {allYears.map((year) => (
                        <button
                            key={year}
                            onClick={() => setSelectedYear(year)}
                            className={`w-full text-center p-fluid rounded-md hover:bg-neutral hover:text-white transition ${
                                selectedYear === year ? "text-white font-semibold bg-primary" : ""
                            }`}
                        >
                            {year}
                        </button>
                    ))}
                </aside>

                {/* Alerts Feed */}
                <main className="flex-1 overflow-y-auto p-fluid space-y-6">
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
                        <div className="text-gray-400 text-center mt-[clamp(1rem,2vw,2rem)] text-fluid">No alerts found.</div>
                    ) : (
                        filteredAlerts.map((alert: Alert, index: number) => (
                            <div key={index}>
                                <div className="text-fluid text-gray-500 mb-[clamp(0.25rem,0.5vw,0.5rem)]">
                                    {formatDateTimeForUserTZ(alert.alertTime)}
                                </div>
                                <div className="bg-[var(--color-surface)] p-fluid rounded-xl shadow-md whitespace-pre-line cursor-pointer text-fluid"
                                    onClick={() => handleAlertClick(alert)}
                                >
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
