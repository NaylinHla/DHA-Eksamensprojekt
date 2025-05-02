import {useEffect, useMemo, useRef, useState} from "react";
import {CartesianGrid, Legend, Line, LineChart, ResponsiveContainer, Tooltip, XAxis, YAxis} from "recharts";
import toast from "react-hot-toast";
import {useAtom} from "jotai";
import {
    AdminHasDeletedData,
    formatDateTimeForUserTZ,
    GreenhouseSensorDataAtom,
    JwtAtom,
    SelectedDeviceIdAtom,
    SensorHistoryDto,
    StringConstants,
    UserDevice,
    useTopicManager,
    useWebSocketMessage,
} from "../import";
import {greenhouseDeviceClient} from "../../apiControllerClients.ts";

export default function DeviceHistory() {
    const [greenhouseSensorDataAtom, setGreenhouseSensorDataAtom] = useAtom(GreenhouseSensorDataAtom);
    const [jwt] = useAtom(JwtAtom);
    const [devices, setDevices] = useState<UserDevice[]>([]);
    const [selectedDeviceId, setSelectedDeviceId] = useAtom(SelectedDeviceIdAtom);
    const [loadingDevices, setLoadingDevices] = useState(true);
    const [loadingData, setLoadingData] = useState(false);
    const prevId = useRef<string | null>(null);
    const {subscribe, unsubscribe} = useTopicManager();

    const Spinner = () => (
        <div className="flex justify-center items-center h-32">
            <svg className="animate-spin h-8 w-8 mr-3 text-gray-500" viewBox="0 0 24 24">
                <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"
                        fill="none"/>
                <path className="opacity-75" fill="currentColor"
                      d="M4 12a8 8 0 018-8v4a4 4 0 00-4 4H4z"/>
            </svg>
            <span className="text-gray-500">Loading…</span>
        </div>
    );

    // WebSocket subscriptions
    useEffect(() => {
        if (!selectedDeviceId) return;
        const topic = `GreenhouseSensorData/${selectedDeviceId}`;
        // Unsubscribe from the previous device if it's different from the new one
        if (prevId.current && prevId.current !== selectedDeviceId) {
            unsubscribe(`GreenhouseSensorData/${prevId.current}`).then();
        }
        subscribe(topic).then();
        prevId.current = selectedDeviceId;

        return () => {
            unsubscribe(topic).then();
        };
    }, [selectedDeviceId, subscribe, unsubscribe]);


    // Fetch devices
    useEffect(() => {
        if (!jwt) return;
        setLoadingDevices(true);
        greenhouseDeviceClient.getAllUserDevices(jwt).then((res: any) => {
            const list = res.allUserDevice || [];
            setDevices(list);
            if (!selectedDeviceId && list.length) {
                setSelectedDeviceId(list[0].deviceId!); // If no device is selected, pick the first one
                setLoadingData(true);
            }
        })
            .catch(() => toast.error("Failed to load devices"))
            .finally(() => setLoadingDevices(false));
    }, [jwt, selectedDeviceId, setSelectedDeviceId]);


    // Fetch data for selected device once the device is selected
    useEffect(() => {
        if (!jwt || !selectedDeviceId) return;
        setLoadingData(true);
        greenhouseDeviceClient.getSensorDataByDeviceId(selectedDeviceId, jwt)
            .then(response => setGreenhouseSensorDataAtom(response))
            .catch(() => toast.error("Failed to load sensor data"))
            .finally(() => setLoadingData(false));
    }, [jwt, selectedDeviceId, setGreenhouseSensorDataAtom]);

    // Live updates
    useWebSocketMessage(StringConstants.ServerBroadcastsLiveDataToDashboard, (dto: any) => {
        const newLogs: SensorHistoryDto[] = dto.logs?.[0]?.sensorHistoryRecords || [];
        if (!newLogs.length) return;
        const unique = newLogs.filter(log => {
            const t = log.time ? new Date(log.time).getTime() : NaN;
            return !isNaN(t) && !greenhouseSensorDataAtom.some(dev =>
                dev.sensorHistoryRecords?.some(old => new Date(old.time!).getTime() === t)
            );
        });
        if (!unique.length) return;
        setGreenhouseSensorDataAtom(prev =>
            prev.map(dev =>
                dev.deviceId === selectedDeviceId
                    ? {...dev, sensorHistoryRecords: [...(dev.sensorHistoryRecords || []), ...unique]}
                    : dev
            )
        );
    });

    // Deleted data broadcast
    useWebSocketMessage(StringConstants.AdminHasDeletedData, (_: AdminHasDeletedData) => {
        toast("Someone has deleted everything.");
        setGreenhouseSensorDataAtom([]);
    });

    // Prepare chart data
    const chartDataByKey = useMemo(() => {
        const deviceData = greenhouseSensorDataAtom.find(r => r.deviceId === selectedDeviceId);
        const recs = deviceData?.sensorHistoryRecords || [];
        const format = (key: keyof SensorHistoryDto) =>
            recs.map(e => ({time: formatDateTimeForUserTZ(e.time), value: e[key] ?? NaN}));
        return {
            temperature: format("temperature"),
            humidity: format("humidity"),
            airPressure: format("airPressure"),
            airQuality: format("airQuality"),
        };
    }, [greenhouseSensorDataAtom, selectedDeviceId]);

    const renderChart = (data: any[], label: string) => (
        <div className="mb-10 px-2">
            <h2 className="text-xl font-semibold mb-2">{label}</h2>
            <ResponsiveContainer width="100%" height={400}>
                <LineChart data={data}>
                    <CartesianGrid strokeDasharray="3 3"/>
                    <XAxis dataKey="time"/>
                    <YAxis/>
                    <Tooltip/>
                    <Legend/>
                    <Line
                        type="monotone"
                        dataKey="value"
                        name={label}
                        stroke="var(--color-primary)"
                        dot={false}
                        isAnimationActive={false}
                        animationDuration={500}
                    />
                </LineChart>
            </ResponsiveContainer>
        </div>
    );

    return (
        <div>
            {/* Top Bar + Status Box */}
            <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4 p-4">
                <h1 className="text-2xl font-bold">Overview:</h1>

                {/* Current Status Box */}
                <div className="border rounded p-4 shadow bg-[boxcolor] min-w-[260px] w-full sm:w-auto relative">
                    <div className={loadingDevices || loadingData ? "invisible" : ""}>
                        {(() => {
                            const deviceData = greenhouseSensorDataAtom.find(r => r.deviceId === selectedDeviceId);
                            const latest = deviceData?.sensorHistoryRecords
                                ?.sort((a, b) =>
                                    new Date(b.time ?? 0).getTime() - new Date(a.time ?? 0).getTime()
                                )[0];

                            const formatNumber = (value: number | null | undefined) =>
                                value != null ? value.toFixed(2) : "N/A";

                            if (!latest) {
                                return (
                                    <>
                                        <h2 className="font-bold mb-3">Current Status</h2>
                                        <p className="text-sm">No data available</p>
                                    </>
                                );
                            }

                            return (
                                <>
                                    <div className="flex items-center justify-between mb-3">
                                        <h2 className="font-bold">Current Status</h2>
                                        <span className="text-xs text-gray-500">
                                          {formatDateTimeForUserTZ(latest.time)}
                                        </span>
                                    </div>
                                    <div className="text-sm space-y-2">
                                        <div className="grid grid-cols-2 gap-x-4">
                                            <div className="flex justify-between">
                                                <span className="font-medium w-24">Temperature:</span>
                                                <span>{formatNumber(latest.temperature)} °C</span>
                                            </div>
                                            <div className="flex justify-between">
                                                <span className="font-medium w-24">Humidity:</span>
                                                <span>{formatNumber(latest.humidity)} %</span>
                                            </div>
                                            <div className="flex justify-between">
                                                <span className="font-medium w-24">Air Pressure:</span>
                                                <span>{formatNumber(latest.airPressure)} hPa</span>
                                            </div>
                                            <div className="flex justify-between">
                                                <span className="font-medium w-24">Air Quality:</span>
                                                <span>{formatNumber(latest.airQuality)} ppm</span>
                                            </div>
                                        </div>
                                    </div>
                                </>
                            );
                        })()}
                    </div>

                    {/* Spinner overlay */}
                    {(loadingDevices || loadingData) && (
                        <div className="absolute inset-0 flex justify-center items-center bg-white bg-opacity-80 rounded">
                            <div className="flex flex-col items-center">
                                <svg className="animate-spin h-6 w-6 text-gray-500 mb-2" viewBox="0 0 24 24">
                                    <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" fill="none" />
                                    <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8v4a4 4 0 00-4 4H4z" />
                                </svg>
                                <span className="text-sm text-gray-500">Loading...</span>
                            </div>
                        </div>
                    )}
                </div>

                {/* Device selector */}
                <div className="flex flex-col sm:flex-row sm:items-center gap-2">
                    <label className="font-medium">Select Device:</label>
                    <select
                        className="border rounded p-2"
                        value={selectedDeviceId||""}
                        onChange={e => { setSelectedDeviceId(e.target.value); setLoadingData(true); }}
                        disabled={loadingDevices}
                    >
                        {devices.length === 0 ? (
                            <option>No device found</option>
                        ) : (
                            devices.map(d => (
                                <option key={d.deviceId} value={d.deviceId}>
                                    {d.deviceName}
                                </option>
                            ))
                        )}
                    </select>
                </div>

            </div>

            {/* Chart loader or charts underneath top bar */}
            {loadingData
                ? <Spinner/>
                : (
                    <>
                        {renderChart(chartDataByKey.temperature, "Temperature")}
                        {renderChart(chartDataByKey.humidity, "Humidity")}
                        {renderChart(chartDataByKey.airPressure, "Air Pressure")}
                        {renderChart(chartDataByKey.airQuality, "Air Quality")}
                    </>
                )
            }

        </div>
    );
}
