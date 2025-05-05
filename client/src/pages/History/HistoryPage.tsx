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

type Point = { time: string; value: number };

function downsampleAvg(points: Point[], maxTotalPoints: number): Point[] {
    const n = points.length;
    if (n <= maxTotalPoints) return points;
    const bucketSize = Math.ceil(n / maxTotalPoints);
    const down: Point[] = [];
    for (let i = 0; i < n; i += bucketSize) {
        const slice = points.slice(i, i + bucketSize);
        const avg = slice.reduce((sum, p) => sum + p.value, 0) / slice.length;
        down.push({ time: slice[0].time, value: avg });
    }
    return down;
}

export default function DeviceHistory() {
    const todayDate = new Date();
    const oneMonthAgoDate = new Date();
    oneMonthAgoDate.setMonth(todayDate.getMonth() - 1);
    const isoToday = todayDate.toISOString().slice(0, 10);
    const isoOneMonthAgo = oneMonthAgoDate.toISOString().slice(0, 10);

    const [greenhouseSensorDataAtom, setGreenhouseSensorDataAtom] = useAtom(GreenhouseSensorDataAtom);
    const [jwt] = useAtom(JwtAtom);
    const [devices, setDevices] = useState<UserDevice[]>([]);
    const [selectedDeviceId, setSelectedDeviceId] = useAtom(SelectedDeviceIdAtom);
    const [loadingDevices, setLoadingDevices] = useState(true);
    const [loadingData, setLoadingData] = useState(false);
    const [rangeFrom, setRangeFrom] = useState<string>(isoOneMonthAgo);
    const [rangeTo, setRangeTo] = useState<string>(isoToday);

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
            .catch(() => toast.error("Failed to load devices", { id: "load-devices-error" }))
            .finally(() => setLoadingDevices(false));
    }, [jwt, selectedDeviceId, setSelectedDeviceId]);

    // --- Debounce range updates ---
    useEffect(() => {
        if (!jwt || !selectedDeviceId) return;
        const timeout = setTimeout(() => {
            setLoadingData(true);
            greenhouseDeviceClient
                .getAllSensorHistoryByDeviceAndTimePeriodIdDto(
                    selectedDeviceId,
                    new Date(rangeFrom),
                    new Date(rangeTo),
                    jwt
                )
                .then(res => setGreenhouseSensorDataAtom(res))
                .catch(() => toast.error("Failed to load sensor data", { id: "load-sensor-error" }))
                .finally(() => setLoadingData(false));
        }, 300); // 300ms debounce
        return () => clearTimeout(timeout);
    }, [jwt, selectedDeviceId, rangeFrom, rangeTo]);

    // --- WebSocket live update ---
    useWebSocketMessage(StringConstants.ServerBroadcastsLiveDataToDashboard, (dto: any) => {
        if (rangeTo !== isoToday) return;
        const newLogs: SensorHistoryDto[] = dto.logs?.[0]?.sensorHistoryRecords || [];
        if (!newLogs.length) return;

        const existing = new Set(
            greenhouseSensorDataAtom.flatMap(d =>
                d.sensorHistoryRecords?.map(r => new Date(r.time!).getTime()) ?? []
            )
        );
        const unique = newLogs.filter(log =>
            !existing.has(new Date(log.time!).getTime())
        );
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
        toast("Someone has deleted everything.", { id: "admin-deleted-data" });
        setGreenhouseSensorDataAtom([]);
    });

    // Prepare chart data
    const chartDataByKey = useMemo(() => {
        const deviceData = greenhouseSensorDataAtom.find(r => r.deviceId === selectedDeviceId);
        const recs = deviceData?.sensorHistoryRecords || [];

        type NumericKey = Exclude<keyof SensorHistoryDto, "time">;
        const format = (key: NumericKey) =>
            recs.map(e => ({
                time: formatDateTimeForUserTZ(e.time),
                value: e[key] ?? NaN
            }));

        const from = new Date(rangeFrom);
        const to = new Date(rangeTo);
        const days = Math.max((to.getTime() - from.getTime()) / (1000 * 60 * 60 * 24), 1);
        const maxPoints = Math.min(Math.ceil(days * 50), 1000);

        return {
            temperature: downsampleAvg(format("temperature"), maxPoints),
            humidity: downsampleAvg(format("humidity"), maxPoints),
            airPressure: downsampleAvg(format("airPressure"), maxPoints),
            airQuality: downsampleAvg(format("airQuality"), maxPoints)
        };
    }, [greenhouseSensorDataAtom, selectedDeviceId, rangeFrom, rangeTo]);


    const graphReady = Object.values(chartDataByKey).some(series => series.length > 0);

    const renderChart = useMemo(() => (data: Point[], label: string) => (
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
    ), []);

    return (
        <div>
            {/* Top Bar + Status Box */}
            <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4 p-4">
                <h1 className="text-2xl font-bold">Overview:</h1>

                <div className="flex gap-2">
                    <label>From:
                        <input type="date" value={rangeFrom} onChange={e => setRangeFrom(e.target.value)}
                               className="border ml-1 p-1 rounded"/>
                    </label>
                    <label>To:
                        <input type="date" value={rangeTo} onChange={e => setRangeTo(e.target.value)}
                               className="border ml-1 p-1 rounded"/>
                    </label>
                </div>

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
                {(!graphReady || loadingData)
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
