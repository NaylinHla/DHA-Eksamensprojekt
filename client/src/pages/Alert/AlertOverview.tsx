import React, {useState} from "react";
import useAlertsRest, {Alert} from "../../hooks/useAlertsRest.tsx";
import {formatDateTimeForUserTZ, LoadingSpinner, TitleTimeHeader, useConvertTemperatureInSentence} from "../import";
import {MyAlertConditionRoute} from "../../routeConstants";
import {useNavigate} from "react-router-dom";

const getYear = (dateString: string) => new Date(dateString).getFullYear();

const AlertOverview = () => {
    const [selectedYear, setSelectedYear] = useState<number | null>(null);
    const {alerts, loading} = useAlertsRest();
    const navigate = useNavigate();
    const allYears = [...new Set(alerts.map(a => getYear(a.alertTime)))].sort((a, b) => b - a);
    const { convertTemperatureInSentence } = useConvertTemperatureInSentence();

    const filteredAlerts = selectedYear
        ? alerts.filter((a) => getYear(a.alertTime) === selectedYear)
        : alerts;

    // Handler for alert click (check if it's related to a plant or a device)
    const handleAlertClick = (alert: Alert) => {
        console.log('Alert clicked:', alert);
        if (alert.alertPlantId) {
            navigate(MyAlertConditionRoute);
        } else if (alert.alertUserDeviceId) {
            console.log('Navigating with:', { autoSelectDevice: true, deviceId: alert.alertUserDeviceId });
            navigate(MyAlertConditionRoute, {
                state: { autoSelectDevice: true},
                replace: false,
            });
        } else {
            console.log(`Alert ID: ${alert.alertId}`);
        }
    };



    return (
        <div className="flex flex-col min-h-screen bg-[--color-background] text-[--color-primary] font-display">
            <TitleTimeHeader title="Alerts Overview"/>
            <div className="flex flex-grow overflow-hidden">
                {/* Conditional aside */}
                {!loading && filteredAlerts.length > 0 && (
                    <aside
                        className="w-[clamp(8rem,15vw,12rem)] flex flex-col items-center p-fluid space-y-2 text-fluid">
                        {selectedYear && (
                            <button
                                onClick={() => setSelectedYear(null)}
                                className="w-full text-center py-2 px-3 rounded-md text-[--color-primary] hover:text-white hover:bg-neutral transition font-semibold"
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
                )}

                {/* Main content */}
                <main className="flex-1 overflow-y-auto p-fluid">
                    {loading || filteredAlerts.length === 0 ? (
                        <div className="flex items-center justify-center mt-5">
                            {loading ? (
                                <LoadingSpinner />
                            ) : (
                                <div className="text-gray-400 text-fluid">No alerts found.</div>
                            )}
                        </div>
                    ) : (
                        <div className="space-y-6">
                            {filteredAlerts.map((alert: Alert, index: number) => (
                                <div key={index}>
                                    <div className="text-fluid text-[--color-primary] mb-[clamp(0.25rem,0.5vw,0.5rem)]">
                                        {formatDateTimeForUserTZ(alert.alertTime)}
                                    </div>
                                    <div
                                        className="bg-[var(--color-surface)] p-fluid rounded-xl shadow-md whitespace-pre-line cursor-pointer text-fluid"
                                        onClick={() => handleAlertClick(alert)}
                                    >
                                        {convertTemperatureInSentence(alert.alertDesc)}
                                    </div>
                                </div>
                            ))}
                        </div>
                    )}
                </main>
            </div>
        </div>
    );
};

export default AlertOverview;
