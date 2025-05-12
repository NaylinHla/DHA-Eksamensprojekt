import React, { useEffect, useMemo, useRef, useState } from "react";
import { format } from 'date-fns';
import {
    CategoryScale,
    Chart as ChartJS,
    Filler,
    Legend as ChartLegend,
    LinearScale,
    LineElement,
    PointElement,
    Tooltip as ChartTooltip,
} from "chart.js";
import { Line } from "react-chartjs-2";
import toast from "react-hot-toast";
import { useAtom } from "jotai";
import {
    formatDateTimeForUserTZ,
    GetRecentSensorDataForAllUserDeviceDto,
    GreenhouseSensorDataAtom,
    JwtAtom,
    SelectedDeviceIdAtom,
    SensorHistoryDto,
    SensorHistoryWithDeviceDto,
    StringConstants,
    TitleTimeHeader,
    UserDevice,
    useThrottle,
    useTopicManager,
    useWebSocketMessage,
} from "../import";
import { greenhouseDeviceClient, userDeviceClient } from "../../apiControllerClients.ts";
import { cssVar } from "../../components/utils/Theme/theme.ts";

ChartJS.register(LineElement, PointElement, LinearScale, CategoryScale, ChartTooltip, ChartLegend, Filler);

type Point = { time: string; value: number; [key: string]: number | string };
type TabKey = "temperature" | "humidity" | "airPressure" | "airQuality";

export default function HistoryPage() {
    // dates
    const today = new Date();
    const monthAgo = new Date(today);
    monthAgo.setMonth(today.getMonth() - 1);
    const isoToday = today.toISOString().slice(0, 10);
    const isoMonthAgo = monthAgo.toISOString().slice(0, 10);

    // atoms & state
    const [greenhouseData, setGreenhouseData] = useAtom(GreenhouseSensorDataAtom);
    const [latestSensorData, setLatestSensorData] = useState<Record<string, SensorHistoryWithDeviceDto>>({});
    const [jwt] = useAtom(JwtAtom);
    const [devices, setDevices] = useState<UserDevice[]>([]);
    const [selectedDeviceId, setSelectedDeviceId] = useAtom(SelectedDeviceIdAtom);
    const [loadingDevices, setLoadingDevices] = useState(true);
    const [loadingData, setLoadingData] = useState(false);
    const [rangeFrom, setRangeFrom] = useState(isoMonthAgo);
    const [rangeTo, setRangeTo] = useState(isoToday);
    const [tab, setTab] = useState<TabKey>("temperature");

    const chartRefs = {
        temperature: useRef<ChartJS | null>(null),
        humidity: useRef<ChartJS | null>(null),
        airPressure: useRef<ChartJS | null>(null),
        airQuality: useRef<ChartJS | null>(null),
    };

    // WebSocket bindings
    const { subscribe, unsubscribe } = useTopicManager();
    const prevTopic = useRef<string | null>(null);

    const rangeFromRef = useRef(rangeFrom);
    const rangeToRef = useRef(rangeTo);
    useEffect(() => { rangeFromRef.current = rangeFrom; rangeToRef.current = rangeTo; }, [rangeFrom, rangeTo]);

    
    // Tab Helper
    const pretty: Record<TabKey, string> = {
        temperature: "Temperature",
        humidity: "Humidity",
        airPressure: "Air Pressure",
        airQuality: "Air Quality",
    };

    const unit: Record<TabKey, string> = {
        temperature: "°C",
        humidity: "%",
        airPressure: "hPa",
        airQuality: "ppm",
    };


    //  Chart Helpers
    function appendPointToChart(
        chartRef: React.RefObject<ChartJS | null>,
        point: { time: string; value: number },
        maxPoints = 500,
    ) {
        const chart = chartRef.current;
        if (!chart) return;

        // Ensure arrays exist
        if (!Array.isArray(chart.data.labels)) chart.data.labels = [];
        if (!Array.isArray(chart.data.datasets[0].data)) chart.data.datasets[0].data = [];

        // 1) Append new
        chart.data.labels.push(point.time);
        (chart.data.datasets[0].data as number[]).push(point.value);

        // 2) Pop oldest if over limit
        if (chart.data.labels.length > maxPoints) {
            chart.data.labels.shift();
            (chart.data.datasets[0].data as number[]).shift();
        }

        chart.update("none");
    }

    const throttledAppend = useThrottle((logs: SensorHistoryDto[]) => {
        if (!logs.length) return;

        // update jotai store
        setGreenhouseData(prev => prev.map(d =>
            d.deviceId === selectedDeviceId
                ? { ...d, sensorHistoryRecords: [...(d.sensorHistoryRecords || []), ...logs] }
                : d,
        ));

        /* Push to charts (only if chart exists yet) */
        logs.forEach(r => {
            const t = formatDateTimeForUserTZ(r.time);
            if (!t) return;
            appendPointToChart(chartRefs.temperature, { time: t, value: Number(r.temperature) });
            appendPointToChart(chartRefs.humidity, { time: t, value: Number(r.humidity) });
            appendPointToChart(chartRefs.airPressure, { time: t, value: Number(r.airPressure) });
            appendPointToChart(chartRefs.airQuality, { time: t, value: Number(r.airQuality) });
        });
    }, 1000);

    /* Websocket Data */
    useWebSocketMessage(StringConstants.ServerBroadcastsLiveDataToDashboard, (dto: any) => {
        const logs: SensorHistoryDto[] = dto.logs?.[0]?.sensorHistoryRecords || [];
        const deviceId = dto.logs?.[0]?.deviceId;
        if (!logs.length || !deviceId) return;

        // Update the latestSensorData for this device
        const latestLog = logs[logs.length - 1];
        setLatestSensorData(prev => ({
            ...prev,
            [deviceId]: {
                deviceId,
                temperature: latestLog.temperature,
                humidity: latestLog.humidity,
                airPressure: latestLog.airPressure,
                airQuality: latestLog.airQuality,
                time: latestLog.time,
            },
        }));

        // Only update chart if logs are in date range
        const from = new Date(rangeFromRef.current);
        const to = new Date(rangeToRef.current);
        to.setHours(23, 59, 59, 999);
        const inRange = logs.filter(l => {
            const t = new Date(l.time!);
            return t >= from && t <= to;
        });
        if (!inRange.length) return;

        const existing = new Set(
            greenhouseData.flatMap(d => d.sensorHistoryRecords?.map(r => new Date(r.time!).getTime()) || []),
        );
        const unique = inRange.filter(l => !existing.has(new Date(l.time!).getTime()));
        if (unique.length) throttledAppend(unique);
    });

    // subscribe to topic changes
    useEffect(() => {
        if (!selectedDeviceId) return;
        const newTopic = `GreenhouseSensorData/${selectedDeviceId}`;
        if (prevTopic.current && prevTopic.current !== selectedDeviceId) {
            unsubscribe(`GreenhouseSensorData/${prevTopic.current}`).catch(() => {});
        }
        subscribe(newTopic).catch(() => {});
        prevTopic.current = selectedDeviceId;
        return () => void unsubscribe(newTopic).catch(() => {});
    }, [selectedDeviceId]);

    // Fetch devices
    useEffect(() => {
        if (!jwt) return;
        setLoadingDevices(true);
        userDeviceClient.getAllUserDevices(jwt)
            .then((list: any) => {
                const devices = Array.isArray(list) ? list : [];
                setDevices(devices);
                if (!selectedDeviceId && devices.length) setSelectedDeviceId(devices[0].deviceId!);
            })
            .catch(() => toast.error("Failed to load devices", { id: "load-devices-error" }))
            .finally(() => setLoadingDevices(false));
    }, [jwt]);

    // load history data
    useEffect(() => {
        if (!jwt || !selectedDeviceId) return;
        setLoadingData(true);
        const debounced = setTimeout(() => {
            const [fy, fm, fd] = rangeFrom.split("-").map(Number);
            const [ty, tm, td] = rangeTo.split("-").map(Number);
            const localStart = new Date(fy, fm - 1, fd, 0, 0, 0, 0);
            const localEnd = new Date(ty, tm - 1, td, 23, 59, 59, 999);
            const utcStart = new Date(localStart.toISOString());
            const utcEnd = new Date(localEnd.toISOString());
            greenhouseDeviceClient.getAllSensorHistoryByDeviceAndTimePeriodIdDto(selectedDeviceId, utcStart, utcEnd, jwt)
                .then(setGreenhouseData)
                .catch(() => toast.error("Failed to load sensor data", { id: "load-sensor-error" }))
                .finally(() => setLoadingData(false));
        }, 300);
        return () => clearTimeout(debounced);
    }, [jwt, selectedDeviceId, rangeFrom, rangeTo]);

    // load recent data
    useEffect(() => {
        if (!jwt) return;
        greenhouseDeviceClient.getRecentSensorDataForAllUserDevice(jwt)
            .then((res: GetRecentSensorDataForAllUserDeviceDto | null) => {
                if (!res?.sensorHistoryWithDeviceRecords?.length) { setLatestSensorData({}); return; }
                const snapshot = res.sensorHistoryWithDeviceRecords.reduce((acc, curr) => {
                    if (curr.deviceId) acc[curr.deviceId] = curr;
                    return acc;
                }, {} as Record<string, SensorHistoryWithDeviceDto>);
                setLatestSensorData(snapshot);
            })
            .catch(() => toast.error("Failed to load recent sensor data", { id: "load-sensor-error" }));
    }, [jwt]);

    function downSampleRecords<T>(arr: T[], maxPoints = 500): T[] {
        if (arr.length <= maxPoints) return arr;
        const step = Math.floor(arr.length / maxPoints);
        return arr.filter((_, idx) => idx % step === 0);
    }

    //  Build series
    const series = useMemo(() => {
        const recs = greenhouseData.find(d => d.deviceId === selectedDeviceId)?.sensorHistoryRecords || [];
        if (!recs.length) return { temperature: [], humidity: [], airPressure: [], airQuality: [] };

        // 1) Deduplicate consecutive identical records
        const unique: SensorHistoryDto[] = [];
        let prev: Partial<SensorHistoryDto> = {};
        for (const r of recs) {
            if (
                r.time === prev.time &&
                r.temperature === prev.temperature &&
                r.humidity === prev.humidity &&
                r.airPressure === prev.airPressure &&
                r.airQuality === prev.airQuality
            ) continue;
            unique.push(r);
            prev = r;
        }

        // 2) Down-sample
        const sampled = downSampleRecords(unique, 500);

        // 3) Build point arrays
        const build = (key: keyof SensorHistoryDto): Point[] => sampled.map(r => ({
            time: format(new Date(r.time!), 'd MMM'),
            value: Number(r[key]) || 0,
        }));

        return {
            temperature: build("temperature"),
            humidity: build("humidity"),
            airPressure: build("airPressure"),
            airQuality: build("airQuality"),
        };
    }, [greenhouseData, selectedDeviceId, rangeFrom, rangeTo]);

    // UI Elements
    const Spinner = (
        <div className="flex justify-center items-center h-32">
            <svg className="animate-spin h-8 w-8 mr-3 text-gray-500" viewBox="0 0 24 24">
                <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" fill="none" />
                <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8v4a4 4 0 00-4 4H4z" />
            </svg>
            <span className="text-gray-500">Loading…</span>
        </div>
    );

    const unitMap: Record<string, string> = {
        temperature: "°C",
        humidity: "%",
        airPressure: "hPa",
        airQuality: "ppm",
    };

    // Re-Usable Chart
    const ChartCard: React.FC<{ tabKey: TabKey; data: Point[]; label: string; chartRef: React.RefObject<ChartJS | null>; }> = ({ tabKey, data, label, chartRef }) => (
        data.length ? (
            <div className="bg-[var(--color-surface)] rounded-2xl overflow-hidden mb-6 px-4 pt-4">
                <h3 className="text-lg font-semibold mb-3">{label}</h3>
                <div className="h-50 w-full">
                    <Line
                        ref={chartRef as any}
                        data={{
                            labels: data.map(p => p.time),
                            datasets: [{
                                data: data.map(p => p.value),
                                tension: 0.3,
                                borderWidth: 2,
                                pointRadius: 0,
                                borderColor: cssVar("--color-primary"),
                            }],
                        }}
                        options={{
                            responsive: true,
                            maintainAspectRatio: false,
                            animation: false,
                            elements: { point: { radius: 0 } },
                            devicePixelRatio: 1.5,
                            plugins: {
                                legend: { display: false },
                                tooltip: { enabled: true, intersect: false, mode: "index" },
                            },
                            scales: {
                                x: {
                                    grid: { display: false },
                                    ticks: { maxTicksLimit: 12, autoSkip: true },
                                },
                                y: {
                                    grid: { display: false },
                                    title: {
                                        display: true,
                                        text: `${label} (${unit[tabKey]})`,
                                        padding: 4,
                                    },
                                    ticks: {
                                        callback: (v: string | number) => `${v} ${unit[tabKey]}`,
                                        autoSkip: true,
                                    },
                                },
                            },
                        }}
                    />
                </div>
            </div>
        ) : (
            <div className="text-gray-500 mb-6">No {label} data</div>
        )
    );

    const noData = !series.temperature.length && !series.humidity.length && !series.airPressure.length && !series.airQuality.length;
    const fields: (keyof SensorHistoryWithDeviceDto)[] = ["temperature", "humidity", "airPressure", "airQuality"];

    return (
        <div className="min-h-[calc(100vh-64px)] flex flex-col font-display">
            {/* Header */}
            <TitleTimeHeader title="History"/>

            {/* Filters & controls */}
            <div className="flex flex-wrap items-start gap-4 lg:items-center lg:justify-between p-4 w-full">
                {/* Current status */}
                <div
                    className="bg-[var(--color-surface)] shadow rounded-2xl p-4 w-full sm:flex-1 flex flex-col justify-between">
                    {loadingDevices || loadingData ? Spinner : (() => {
                        const latest = latestSensorData[selectedDeviceId!];
                        if (!latest || fields.some(f => latest[f] == null || latest[f] === 0)) return <p
                            className="text-center text-gray-500">No data available</p>;
                        return (
                            <>
                                <div className="flex justify-between mb-2">
                                    <h2 className="font-bold">Current Status</h2>
                                </div>
                                <div className="grid grid-cols-2 sm:grid-cols-4 gap-x-8 gap-y-1 text-sm">
                                    {fields.map(f => (
                                        <div key={f} className="flex">
                                            <span className="capitalize font-medium">{f}:</span>
                                            <span className="ml-1">
                                                {(latest as any)[f]?.toFixed(2)}{unitMap[f]}
                                            </span>
                                        </div>
                                    ))}
                                </div>
                            </>
                        );
                    })()}
                </div>
                
            </div>

            {/* Charts */}
            <main className="flex-1 overflow-y-auto px-6 pb-6">
                <div className="bg-[var(--color-surface)] shadow rounded-2xl">
                    <div className="px-4 pt-4 flex flex-wrap items-center gap-4 sm:gap-6">
                        
                        {/* Device selector */}
                        <div className="flex items-center gap-2">
                            <label className="font-medium text-sm">Device:</label>
                            <select
                                className="select select-xs bg-[var(--color-surface)]"
                                value={selectedDeviceId || ""}
                                onChange={e => setSelectedDeviceId(e.target.value)}
                                disabled={loadingDevices}
                            >
                                {devices.length === 0
                                    ? <option>No devices</option>
                                    : devices.map(d => (
                                        <option key={d.deviceId} value={d.deviceId}>
                                            {d.deviceName?.length! > 25
                                                ? d.deviceName!.slice(0, 25) + "…"
                                                : d.deviceName || "Unnamed"}
                                        </option>
                                    ))}
                            </select>
                        </div>
                        
                        {/* Date pickers */}
                        <div className="flex items-center gap-2 text-sm ml-auto">
                            <label className="font-medium text-sm">From:</label>
                            <input
                                type="date"
                                value={rangeFrom}
                                onChange={e => {
                                    const v = e.target.value;
                                    if (new Date(v) > new Date(rangeTo)) {
                                        toast.error("From date cannot be after To date");
                                        return;
                                    }
                                    setRangeFrom(v);
                                }}
                                className="input input-xs bg-[var(--color-surface)] ml-1"
                            />
                            <label className="font-medium text-sm">To:</label>
                            
                            <input
                                type="date"
                                value={rangeTo}
                                onChange={e => {
                                    const v = e.target.value;
                                    if (new Date(v) < new Date(rangeFrom)) {
                                        toast.error("To date cannot be before From date");
                                        return;
                                    }
                                    setRangeTo(v);
                                }}
                                className="input input-xs bg-[var(--color-surface)] ml-1"
                            />
                        </div>
                    </div>
                    
                    {/* tab bar */}
                    <div className="tabs tabs-bordered rounded-t-2xl">
                        {(Object.keys(pretty) as TabKey[]).map(key => (
                            <a
                                key={key}
                                className={`tab flex-1 tab-bordered${tab === key ? " tab-active" : ""}`}
                                onClick={() => setTab(key)}
                            >
                                {pretty[key]}
                            </a>
                        ))}
                    </div>
                    <hr className="border-primary"/>
                    
                    {/* active chart */}
                    <div className="p-4">
                        {loadingData ? Spinner : noData ? (
                            <div className="p-8 text-center text-gray-500">
                                No data available for the selected date range.
                            </div>
                        ) : (
                            <ChartCard
                                tabKey={tab}
                                data={series[tab]}
                                label={pretty[tab]}
                                chartRef={chartRefs[tab]}
                            /> 
                            )}
                        </div>
                    </div>
            </main>
        </div>
    );
}