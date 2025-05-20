import React, {useEffect, useMemo, useState} from "react";
import toast from "react-hot-toast";
import {useAtom} from "jotai";
import {Pencil, Plus} from "lucide-react";
import {AdminChangesPreferencesDto, JwtAtom, SensorHistoryWithDeviceDto, UserDevice} from "../../atoms";
import {greenhouseDeviceClient, userDeviceClient} from "../../apiControllerClients";
import {
    ConfirmModal,
    formatDateTimeForUserTZ,
    IntervalSelector,
    SearchBar,
    TitleTimeHeader,
    UserDeviceModal
} from "../import";

// Spinner shown during loading
const Spinner = (
    <div className="flex justify-center items-center h-32">
        <svg className="animate-spin h-8 w-8 mr-3 text-gray-500" viewBox="0 0 24 24">
            <circle cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" fill="none"
                    className="opacity-25"/>
            <path fill="currentColor" d="M4 12a8 8 0 018-8v4a4 4 0 00-4 4H4z" className="opacity-75"/>
        </svg>
        <span className="text-gray-500">Loading…</span>
    </div>
);

// Helper function to format device values (temperature, humidity, etc.)
const formatDeviceValue = (
    value: number | null | undefined,
    unit: string,
    threshold: number = 0,
    defaultValue: string = "N/A"
): string => {
    if (value != null && value > threshold) {
        return `${value.toFixed(2)} ${unit}`;
    }
    return defaultValue;
};

interface LocalPref {
    deviceId: string;
    /** raw seconds */
    interval: number;
}

export default function MyDevicePage() {
    const [jwt] = useAtom(JwtAtom);
    const [deviceData, setDeviceData] = useState<SensorHistoryWithDeviceDto[]>([]);
    const [preferences, setPreferences] = useState<Record<string, LocalPref>>({});
    const [descExpanded, setDescExpanded] = useState<Record<string, boolean>>({});
    const [searchTerm, setSearchTerm] = useState("");
    const [isModalOpen, setIsModalOpen] = useState(false); // State to control the modal
    const [deviceToRemove, setDeviceToRemove] = useState<UserDevice | null>(null);
    const [editModalOpen, setEditModalOpen] = useState(false);
    const [selectedDevice, setSelectedDevice] = useState<{ deviceId: string } | null>(null);
    const [loading, setLoading] = useState(false);

    useEffect(() => {
        if (!jwt) return;

        let showSpinnerTimeout: NodeJS.Timeout | null = null;

        // Start timer to show spinner after 200ms
        showSpinnerTimeout = setTimeout(() => {
            setLoading(true);
        }, 200);

        greenhouseDeviceClient
            .getRecentSensorDataForAllUserDevice(jwt)
            .then((resp) => {
                const sensorData = resp?.sensorHistoryWithDeviceRecords || [];
                setDeviceData(sensorData);

                const initial: Record<string, LocalPref> = {};
                sensorData.forEach((d) => {
                    const secs = Number(d.deviceWaitTime) || 0;
                    if (!d.deviceId) return;
                    initial[d.deviceId] = {deviceId: d.deviceId, interval: secs};
                });
                setPreferences(initial);
            })
            .catch(() => toast.error("Failed to fetch devices"))
            .finally(() => {
                // Cancel spinner timer if request finished before 200ms
                if (showSpinnerTimeout) {
                    clearTimeout(showSpinnerTimeout);
                    showSpinnerTimeout = null;
                }
                setLoading(false);
            });
        return () => {
            if (showSpinnerTimeout) {
                clearTimeout(showSpinnerTimeout);
            }
        };
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
            const id = deviceToRemove.deviceId;

            userDeviceClient
                .deleteUserDevice(id, jwt)
                .then(() => {
                    toast.success(`Device "${deviceName}" removed successfully!`);
                    setDeviceData((prev) => prev.filter((d) => d.deviceId !== id));
                    setPreferences((prev) => {
                        const newPref = {...prev};
                        if (id) delete newPref[id];
                        return newPref;
                    });
                })
                .catch((e) => {
                    toast.error(`Failed to remove device: ${e.message}`);
                })
                .finally(() => {
                    setIsModalOpen(false);
                    setDeviceToRemove(null);
                });
        }
    };


    const cancelRemoveDevice = () => {
        setIsModalOpen(false);
    };

    return (
        <div
            className="min-h-[calc(100vh-64px)] flex flex-col bg-[--color-background] text-[--color-primary] font-display">
            {/* Header */}
            <TitleTimeHeader title="My Device"/>

            <main className="flex-1 overflow-y-auto px-6 py-4">
                <div className="flex flex-col p-6">
                    {/* Header row */}
                    <div className="flex flex-wrap justify-between items-center gap-4 mb-6">
                        <SearchBar searchTerm={searchTerm} onSearch={setSearchTerm}/>
                    </div>

                    {loading ? (
                        <div className="my-10">{Spinner}</div>
                    ) : (
                        <div
                            className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 xl:grid-cols-4 2xl:grid-cols-5 gap-fluid">
                            {filtered.map((device) => {
                                const id = device.deviceId!;
                                const pref = preferences[id]!;
                                const expanded = descExpanded[id] || false;

                                return (
                                    <div key={id}
                                         className="relative flex flex-col justify-between rounded-xl bg-[var(--color-surface)] shadow-md p-4 w-full h-auto transition-shadow cursor-pointer hover:shadow-lg">

                                        <div className="absolute top-2 right-2 flex items-center gap-2">
                                            <button
                                                onClick={(e) => {
                                                    e.stopPropagation();
                                                    setSelectedDevice({deviceId: id});
                                                    setEditModalOpen(true);
                                                }}
                                                className="text-gray-400 hover:text-blue-500 transition"
                                                title="Edit device"
                                            >
                                                <Pencil size={18}/>
                                            </button>

                                            {/* ✕ Remove button */}
                                            <button
                                                onClick={(e) => {
                                                    e.stopPropagation();
                                                    handleRemoveDevice(device);
                                                }}
                                                className="text-gray-400 hover:text-red-500 transition text-lg leading-none"
                                                title="Remove device"
                                            >
                                                ✕
                                            </button>
                                        </div>

                                        <p className="font-semibold text-lg truncate mr-7 mb-1">
                                            {device.deviceName ?? id}
                                        </p>

                                        {/* Description Section */}
                                        <div className="flex flex-col flex-grow">
                                            {device.deviceDesc ? (
                                                <div
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
                                                </div>
                                            ) : (
                                                <div className="flex flex-col justify-between h-full">
                                                    <p className="text-sm text-muted-foreground mb-1">No description</p>
                                                </div>
                                            )}

                                            {/* Show more button */}
                                            {device.deviceDesc && device.deviceDesc.length > 100 && (
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
                                        </div>


                                        {/* For devices without a "Show more" button, ensure their height matches. */}
                                        {!device.deviceDesc || device.deviceDesc.length <= 100 ? (
                                            <div style={{height: "4.5em"}}></div> // Fixed height for non-expanded descriptions
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
                                            <p><strong>Temp:</strong> {formatDeviceValue(device.temperature, "°C")}</p>
                                            <p><strong>Humidity:</strong> {formatDeviceValue(device.humidity, "%")}</p>
                                            <p><strong>Air
                                                Pressure:</strong> {formatDeviceValue(device.airPressure, "hPa")}
                                            </p>
                                            <p><strong>Air
                                                Quality:</strong> {formatDeviceValue(device.airQuality, "ppm")}
                                            </p>
                                            <p><strong>When:</strong>{" "}
                                                {device.time && (device.time as unknown as string) !== "0001-01-01T00:00:00Z"
                                                    ? new Date(device.time).toLocaleString()
                                                    : "N/A"}
                                            </p>
                                        </div>

                                        {/* Interval Selector */}
                                        <div onClick={(e) => e.stopPropagation()}>
                                            <IntervalSelector
                                                totalSeconds={pref.interval}
                                                onChange={(newSecs) =>
                                                    setPreferences((p) => ({
                                                        ...p,
                                                        [id]: {deviceId: id, interval: newSecs},
                                                    }))
                                                }
                                            />
                                        </div>

                                        {/* Change Preferences Button */}
                                        <button
                                            className="px-8 py-3 text-sm sm:text-base font-semibold btn btn-neutral bg-transparent btn-sm mt-auto"
                                            onClick={() => {
                                                const dto: AdminChangesPreferencesDto = {
                                                    deviceId: id,
                                                    interval: String(pref.interval),
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
                                    setSelectedDevice(null); // null = create new
                                    setEditModalOpen(true);
                                }}
                                className="flex items-center justify-center border-2 border-dashed border-muted rounded-xl h-[500px] bg-[var(--color-surface)] hover:border-muted/60 transition"
                            >
                                <Plus className="w-10 h-10 text-muted-foreground"/>
                            </button>
                        </div>
                    )}
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

            <UserDeviceModal
                open={editModalOpen}
                device={selectedDevice}
                onClose={() => setEditModalOpen(false)}
                onSaved={() => {
                    setLoading(true);
                    greenhouseDeviceClient
                        .getRecentSensorDataForAllUserDevice(jwt)
                        .then((resp) => {
                            const records = resp.sensorHistoryWithDeviceRecords || [];
                            setDeviceData(records);

                            const initialPref: Record<string, LocalPref> = {};
                            records.forEach((d) => {
                                const secs = Number(d.deviceWaitTime) || 0;
                                if (!d.deviceId) return;
                                initialPref[d.deviceId] = {deviceId: d.deviceId, interval: secs};
                            });
                            setPreferences(initialPref);

                        })
                        .catch(() => toast.error("Failed to refresh device list"))
                        .finally(() => setLoading(false));
                    setEditModalOpen(false);
                }}
            />
        </div>
    );
}
