import React, {useEffect, useMemo, useState} from "react";
import toast from "react-hot-toast";
import {useAtom} from "jotai";
import {Plus} from "lucide-react";
import {AdminChangesPreferencesDto, JwtAtom, SensorHistoryWithDeviceDto} from "../../atoms";
import {greenhouseDeviceClient, userDeviceClient} from "../../apiControllerClients";
import {ConfirmModal, formatDateTimeForUserTZ, SearchBar, TitleTimeHeader} from "../import"; // Import the ConfirmModal

const intervalMultipliers = {
    Second: 1,
    Minute: 60,
    Hour: 3600,
    Days: 86400,
    Week: 604800,
    Month: 2592000
} as const;

interface LocalPref extends AdminChangesPreferencesDto {
    intervalValue: number;
    intervalUnit: keyof typeof intervalMultipliers;
}

export default function MyDevicePage() {
    const [jwt] = useAtom(JwtAtom);
    const [deviceData, setDeviceData] = useState<SensorHistoryWithDeviceDto[]>([]);
    const [preferences, setPreferences] = useState<Record<string, LocalPref>>({});
    const [descExpanded, setDescExpanded] = useState<Record<string, boolean>>({});
    const [searchTerm, setSearchTerm] = useState("");
    const [isModalOpen, setIsModalOpen] = useState(false); // State to control the modal
    const [deviceToRemove, setDeviceToRemove] = useState<SensorHistoryWithDeviceDto | null>(null); // Store full device object

    useEffect(() => {
        if (!jwt) return;
        greenhouseDeviceClient
            .getRecentSensorDataForAllUserDevice(jwt)
            .then((resp) => {
                const sensorData = resp.sensorHistoryWithDeviceRecords || [];
                setDeviceData(sensorData);

                const initial: Record<string, LocalPref> = {};
                sensorData.forEach((d) => {
                    if (d.deviceId) {
                        initial[d.deviceId] = {
                            deviceId: d.deviceId,
                            intervalValue: 1,
                            intervalUnit: "Minute"
                        };
                    }
                });
                setPreferences(initial);
            })
            .catch(() => toast.error("Failed to fetch device data"));
    }, [jwt]);

    const filtered = useMemo(() => {
        const t = searchTerm.trim().toLowerCase();
        if (!t) return deviceData;
        return deviceData.filter((d) =>
            ((d.deviceName ?? d.deviceId) || "").toLowerCase().includes(t)
        );
    }, [deviceData, searchTerm]);

    const handleRemoveDevice = (device: SensorHistoryWithDeviceDto) => {
        setDeviceToRemove(device);
        setIsModalOpen(true);
    };

    const confirmRemoveDevice = () => {
        if (deviceToRemove) {
            const deviceName = deviceToRemove.deviceName ?? "Unknown Device";
            console.log(`Device ${deviceName} removed!`);
            toast.success(`Device "${deviceName}" removed successfully!`);
            setIsModalOpen(false);
            //TODO Add your remove logic here (e.g., API call)
        }
    };

    const cancelRemoveDevice = () => {
        setIsModalOpen(false);
    };

    return (
        <div
            className="min-h-[calc(100vh-64px)] flex flex-col bg-[--color-background] text-[--color-primary] font-display">
            {/* Header */}
            <TitleTimeHeader/>

            <main className="flex-1 overflow-y-auto px-6 py-4">
                <div className="flex flex-col p-6">
                    {/* Header row */}
                    <div className="flex flex-wrap justify-between items-center gap-4 mb-6">
                        <SearchBar searchTerm={searchTerm} onSearch={setSearchTerm}/>
                    </div>

                    {/* Grid of device cards */}
                    <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 xl:grid-cols-4 gap-6">
                        {filtered.map((device) => {
                            const id = device.deviceId!;
                            const pref = preferences[id]!;
                            const expanded = descExpanded[id] || false;

                            return (
                                <div
                                    key={id}
                                    className="relative flex flex-col justify-between rounded-xl bg-[var(--color-surface)] shadow-md p-4 w-full h-auto transition-shadow"
                                >
                                    {/* ✕ Remove button */}
                                    <button
                                        onClick={() => handleRemoveDevice(device)}  // Pass full device object
                                        className="absolute top-2 right-2 text-gray-400 hover:text-red-500 transition"
                                    >
                                        ✕
                                    </button>

                                    <p className="font-semibold text-lg truncate mr-4 mb-1">
                                        {device.deviceName ?? id}
                                    </p>

                                    {device.deviceDesc && (
                                        <>
                                            <p
                                                style={{
                                                    display: "-webkit-box",
                                                    WebkitBoxOrient: "vertical",
                                                    WebkitLineClamp: expanded ? undefined : 3,
                                                    overflow: "hidden",
                                                    transition: "max-height 0.3s ease",
                                                    maxHeight: expanded ? "none" : "4.5em",
                                                }}
                                                className="text-sm text-muted-foreground mb-1"
                                            >
                                                {device.deviceDesc}
                                            </p>
                                            {device.deviceDesc.length > 100 && (
                                                <button
                                                    className="text-xs text-primary mb-2 self-start"
                                                    onClick={() =>
                                                        setDescExpanded((prev) => ({
                                                            ...prev,
                                                            [id]: !prev[id]
                                                        }))
                                                    }
                                                    style={{
                                                        marginTop: "0.5rem", // Ensure there's space between the description and the button
                                                    }}
                                                >
                                                    {expanded ? "Show less" : "Show more"}
                                                </button>
                                            )}
                                        </>
                                    )}

                                    {/* For devices without a "Show more" button, ensure their height matches. */}
                                    {!device.deviceDesc || device.deviceDesc.length <= 100 ? (
                                        <div style={{ height: "4.5em" }}></div> // Fixed height for non-expanded descriptions
                                    ) : null}


                                    {device.deviceCreateDateTime && (
                                        <p className="text-xs text-muted-foreground mb-2">
                                            Created: {formatDateTimeForUserTZ(device.deviceCreateDateTime)}
                                        </p>
                                    )}

                                    <div className="flex justify-center mb-2">
                                        <img
                                            className="w-24"
                                            src="https://joy-it.net/files/files/Produkte/SBC-NodeMCU-ESP32/SBC-NodeMCU-ESP32-01.png"
                                            alt="Device"
                                        />
                                    </div>

                                    <div className="text-sm space-y-1 mb-3">
                                        <p>🌡 Temp: {device.temperature}°C</p>
                                        <p>💧 Humidity: {device.humidity}%</p>
                                        <p>🌬 Air Pressure: {device.airPressure} hPa</p>
                                        <p>🧪 Air Quality: {device.airQuality} ppm</p>
                                        <p>📅 {new Date(device.time ?? "").toLocaleString()}</p>
                                    </div>

                                    <label className="text-sm mb-1 font-medium">Update interval:</label>
                                    <div className="flex gap-2 mb-3">
                                        <input
                                            type="number"
                                            min={1}
                                            value={pref.intervalValue}
                                            onChange={(e) =>
                                                setPreferences((prev) => ({
                                                    ...prev,
                                                    [id]: {
                                                        ...prev[id],
                                                        intervalValue: Math.max(1, parseInt(e.target.value, 10) || 1)
                                                    }
                                                }))
                                            }
                                            className="border rounded px-2 w-16 text-sm"
                                        />
                                        <select
                                            value={pref.intervalUnit}
                                            onChange={(e) =>
                                                setPreferences((prev) => ({
                                                    ...prev,
                                                    [id]: {
                                                        ...prev[id],
                                                        intervalUnit: e.target.value as LocalPref["intervalUnit"]
                                                    }
                                                }))
                                            }
                                            className="border rounded px-2 text-sm"
                                        >
                                            {Object.keys(intervalMultipliers).map((opt) => (
                                                <option key={opt} value={opt}>
                                                    {opt}
                                                </option>
                                            ))}
                                        </select>
                                    </div>

                                    <button
                                        className="px-8 py-3 text-sm sm:text-base font-semibold btn btn-neutral bg-transparent btn-sm mt-auto"
                                        onClick={() => {
                                            const seconds = pref.intervalValue * intervalMultipliers[pref.intervalUnit];
                                            const dto: AdminChangesPreferencesDto = {
                                                deviceId: id,
                                                interval: seconds.toString()
                                            };
                                            userDeviceClient
                                                .adminChangesPreferences(dto, jwt)
                                                .then(() => toast.success("Preferences sent"))
                                                .catch((e) => toast.error("Error: " + e.message));
                                        }}
                                    >
                                        Change preferences
                                    </button>
                                </div>
                            );
                        })}

                        {/* Add device card */}
                        <button
                            onClick={() => {
                                /* TODO: Add device logic */
                            }}
                            className="flex items-center justify-center border-2 border-dashed border-muted rounded-xl h-[500px] bg-[var(--color-surface)] hover:border-muted/60 transition"
                        >
                            <Plus className="w-10 h-10 text-muted-foreground"/>
                        </button>
                    </div>
                </div>
            </main>

            {/* Confirm Modal */}
            <ConfirmModal
                isOpen={isModalOpen}
                title="Confirm Removal"
                subtitle={`Are you sure you want to remove the device?`}
                onConfirm={confirmRemoveDevice}
                onCancel={cancelRemoveDevice}
            />
        </div>
    );
}
