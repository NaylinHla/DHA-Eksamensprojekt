import toast from "react-hot-toast";
import {useEffect, useState} from "react";
import {useAtom} from "jotai";
import {AdminChangesPreferencesDto, JwtAtom, SensorHistoryWithDeviceDto} from "../../atoms";
import {greenhouseDeviceClient} from "../../apiControllerClients";

export default function MyDevicePage() {
    const [jwt] = useAtom(JwtAtom);
    const [deviceData, setDeviceData] = useState<SensorHistoryWithDeviceDto[]>([]);
    const [preferences, setPreferences] = useState<Record<string, AdminChangesPreferencesDto>>({});

    useEffect(() => {
        if (!jwt) return;

        greenhouseDeviceClient.getRecentSensorDataForAllUserDevice(jwt)
            .then((resp) => {
                const sensorData = resp.sensorHistoryWithDeviceRecords || [];
                setDeviceData(sensorData);

                // Initialize preferences for each device
                const initialPreferences: Record<string, AdminChangesPreferencesDto> = {};
                sensorData.forEach(device => {
                    if (device.deviceId) {
                        initialPreferences[device.deviceId] = {
                            interval: "Minute",
                            unit: "Celcius",
                            deviceId: device.deviceId
                        };
                    }
                });
                setPreferences(initialPreferences);
            })
            .catch(e => toast.error("Failed to fetch device data"));
    }, [jwt]);

    if (!jwt || jwt.length < 1) {
        return <div className="flex flex-col items-center justify-center h-screen">Please sign in to continue</div>;
    }

    return (
        <div className="flex flex-row flex-wrap items-start justify-around p-4 gap-6">
            {deviceData.map((device) => {
                if (!device.deviceId) return null;

                const currentPref = preferences[device.deviceId];

                return (
                    <div key={device.deviceId} className="flex flex-col p-4 border rounded shadow-md w-80">
                        <p className="font-bold">{device.deviceName ?? device.deviceId}</p>
                        {device.deviceDesc && <p className="text-sm italic text-gray-600">{device.deviceDesc}</p>}
                        {device.deviceCreateDateTime && (
                            <p className="text-xs text-gray-500">
                                Created: {new Date(device.deviceCreateDateTime).toLocaleDateString()}
                            </p>
                        )}
                        <img className="w-32 mb-2" src="https://joy-it.net/files/files/Produkte/SBC-NodeMCU-ESP32/SBC-NodeMCU-ESP32-01.png" alt="Device" />
                        <p>🌡 Temp: {device.temperature}°C</p>
                        <p>💧 Humidity: {device.humidity}%</p>
                        <p>🌬 Air Pressure: {device.airPressure} hPa</p>
                        <p>🧪 Air Quality: {device.airQuality}</p>
                        <p>📅 Time: {new Date(device.time ?? "").toLocaleString()}</p>

                        <label className="input mt-2">
                            <b>Interval:</b>
                            <input
                                value={currentPref?.interval ?? ""}
                                onChange={(e) => setPreferences(prev => ({
                                    ...prev,
                                    [device.deviceId!]: {
                                        ...prev[device.deviceId!],
                                        interval: e.target.value
                                    }
                                }))}
                                placeholder="Interval"
                                type="text"
                                className="grow"
                            />
                        </label>

                        <label className="input mt-1">
                            <b>Unit:</b>
                            <input
                                value={currentPref?.unit ?? ""}
                                onChange={(e) => setPreferences(prev => ({
                                    ...prev,
                                    [device.deviceId!]: {
                                        ...prev[device.deviceId!],
                                        unit: e.target.value
                                    }
                                }))}
                                placeholder="Unit"
                                type="text"
                                className="grow"
                            />
                        </label>

                        <button className="btn mt-2" onClick={() => {
                            greenhouseDeviceClient.adminChangesPreferences(currentPref, jwt)
                                .then(() => toast.success("Preferences sent to device"))
                                .catch(e => toast.error("Error: " + e.message));
                        }}>
                            Change preferences
                        </button>
                    </div>
                );
            })}
        </div>
    );
}
