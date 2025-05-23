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
import { useDisplayTemperature } from "../../hooks";
import {
    formatDateTimeForUserTZ,
    GreenhouseSensorDataAtom,
    JwtAtom, LoadingSpinner,
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

type Point = { time: string; display: string; value: number; };
type TabKey = "temperature" | "humidity" | "airPressure" | "airQuality";

export function getCssVar(name: string): string {
    return getComputedStyle(document.documentElement).getPropertyValue(name).trim();
}

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
    const { convert, unit} = useDisplayTemperature();

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

    useEffect(() => {
        rangeFromRef.current = rangeFrom;
        rangeToRef.current = rangeTo;
    }, [rangeFrom, rangeTo]);

    // Tab Helper
    const pretty: Record<TabKey, string> = {
        temperature: "Temperature",
        humidity: "Humidity",
        airPressure: "Air Pressure",
        airQuality: "Air Quality",
    };

    /* const unit: Record<TabKey, string> = {
        temperature: "°C",
        humidity: "%",
        airPressure: "hPa",
        airQuality: "ppm",
    }; */


    //  Chart Helpers
    function appendPointToChart(
        chartRef: React.RefObject<ChartJS | null>,
        point: { time: string; value: number },
        maxPoints = 500
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

        setGreenhouseData(prev =>
            prev.map(d =>
                d.deviceId === selectedDeviceId
                    ? { ...d, sensorHistoryRecords: [...(d.sensorHistoryRecords || []), ...logs] }
                    : d
            )
        );

        /* Push to charts (only if chart exists yet) */
        logs.forEach(r => {
            const iso = r.time instanceof Date ? r.time.toISOString() : String(r.time);
            if (!iso) return;
            appendPointToChart(chartRefs.temperature, { time: iso, value: Number(r.temperature) });
            appendPointToChart(chartRefs.humidity, { time: iso, value: Number(r.humidity) });
            appendPointToChart(chartRefs.airPressure, { time: iso, value: Number(r.airPressure) });
            appendPointToChart(chartRefs.airQuality, { time: iso, value: Number(r.airQuality) });
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
            [deviceId]: { ...latestLog, deviceId },
        }));

        // Only update chart if logs are in date range
        const from = new Date(rangeFromRef.current);
        const to = new Date(rangeToRef.current);
        to.setHours(23, 59, 59, 999);
        const inRange = logs.filter(l => {
            const t = new Date(l.time!);
            return t >= from && t <= to;
        });
        const existing = new Set(
            greenhouseData.flatMap(d => d.sensorHistoryRecords?.map(r => new Date(r.time!).getTime()) || [])
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
        userDeviceClient
            .getAllUserDevices(jwt)
            .then(list => {
                const devices = Array.isArray(list) ? list : [];
                setDevices(devices);
                if (!selectedDeviceId && devices.length) setSelectedDeviceId(devices[0].deviceId!);
            })
            .catch(() => toast.error("Failed to load devices"))
            .finally(() => setLoadingDevices(false));
    }, [jwt]);

    // load history data
    useEffect(() => {
        if (!jwt || !selectedDeviceId) return;
        setLoadingData(true);
        const debounced = setTimeout(() => {
            const start = new Date(rangeFrom + "T00:00:00");
            const end = new Date(rangeTo + "T23:59:59.999");
            greenhouseDeviceClient
                .getAllSensorHistoryByDeviceAndTimePeriodIdDto(selectedDeviceId, start, end, jwt)
                .then(setGreenhouseData)
                .catch(() => toast.error("Failed to load sensor data"))
                .finally(() => setLoadingData(false));
        }, 300);
        return () => clearTimeout(debounced);
    }, [jwt, selectedDeviceId, rangeFrom, rangeTo]);

    // load recent data
    useEffect(() => {
        if (!jwt) return;
        greenhouseDeviceClient
            .getRecentSensorDataForAllUserDevice(jwt)
            .then(res => {
                const recs = res?.sensorHistoryWithDeviceRecords ?? [];
                const snapshot = Object.fromEntries(recs.map(r => [r.deviceId, r]));
                setLatestSensorData(snapshot);
            })
            .catch(() => toast.error("Failed to load recent sensor data"));
    }, [jwt]);

    function downSampleRecords<T>(arr: T[], maxPoints = 500): T[] {
        if (arr.length <= maxPoints) return arr;
        const step = Math.floor(arr.length / maxPoints);
        return arr.filter((_, idx) => idx % step === 0);
    }

    //  Build series
    const series = useMemo(() => {
        const recs = greenhouseData.find(d => d.deviceId === selectedDeviceId)?.sensorHistoryRecords || [];
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

        const build = (key: keyof SensorHistoryDto): Point[] =>
            sampled.map(r => {
                const raw = Number(r[key]) || 0;
                const value = key === "temperature" ? convert(raw) ?? 0 : raw;
                return {
                    time: r.time instanceof Date ? r.time.toISOString() : String(r.time),
                    display: format(new Date(r.time!), "d MMM"),
                    value,
                };
            });

        return {
            temperature: build("temperature"),
            humidity: build("humidity"),
            airPressure: build("airPressure"),
            airQuality: build("airQuality"),
        };
    }, [greenhouseData, selectedDeviceId, rangeFrom, rangeTo]);

    const unitMap: Record<TabKey, string> = {
        temperature: unit,
        humidity: "%",
        airPressure: "hPa",
        airQuality: "ppm",
    };

    // Re-Usable Chart
    const ChartCard: React.FC<{ tabKey: TabKey; data: Point[]; label: string; chartRef: React.RefObject<ChartJS | null>; }> = ({ tabKey, data, label, chartRef }) => {
        const colorPrimary = getCssVar("--color-primary");

        return data.length ? (
            <div className="bg-[var(--color-surface)] rounded-2xl overflow-hidden mb-6 px-4 pt-4">
                <h3 className="text-lg font-semibold mb-3 text-fluid">{label}</h3>
                <div className="w-full h-[clamp(8rem,20vw,40rem)]">
                    <Line
                        ref={chartRef as any}
                        data={{
                            labels: data.map(p => p.time),
                            datasets: [
                                {
                                    data: data.map(p => p.value),
                                    tension: 0.3,
                                    borderWidth: 2,
                                    pointRadius: 0,
                                    borderColor: colorPrimary,
                                },
                            ],
                        }}
                        options={{
                            responsive: true,
                            maintainAspectRatio: false,
                            animation: false,
                            elements: { point: { radius: 0 } },
                            devicePixelRatio: 1.5,
                            plugins: {
                                legend: { display: false },
                                tooltip: {
                                    callbacks: {
                                        title: items => {
                                            if (!items.length) return "";
                                            const i = items[0].dataIndex as number;
                                            const point = data[i];
                                            return point ? format(new Date(point.time), "PPP HH:mm:ss") : "";
                                        },
                                    },
                                },
                            },
                            scales: {
                                x: {
                                    grid: { display: false },
                                    ticks: {
                                        color: colorPrimary,
                                        callback: (val: any) => data[val]?.display ?? "",
                                        maxTicksLimit: 12,
                                        autoSkip: true,
                                    },
                                },
                                y: {
                                    grid: { display: false },
                                    title: {
                                        display: true,
                                        text: `${label} (${unitMap[tabKey]})`,
                                        padding: 4,
                                        color: colorPrimary,
                                    },
                                    ticks: {
                                        color: colorPrimary,
                                        callback: (v: any) => `${v} ${unitMap[tabKey]}`,
                                    },
                                },
                            },
                        }}
                    />
                </div>
            </div>
        ) : (
            <div className="text-[--color-primary] mb-6">No {label} data</div>
        );
    };


    const fields: (keyof SensorHistoryWithDeviceDto)[] = ["temperature", "humidity", "airPressure", "airQuality"];
    const noData = !series.temperature.length && !series.humidity.length && !series.airPressure.length && !series.airQuality.length;

    return (
        <div className="min-h-[calc(100vh-64px)] flex flex-col font-display">
            {/* Header */}
            <TitleTimeHeader title="History"/>

            {/* Filters & controls */}
            <div className="flex flex-wrap items-start gap-fluid lg:items-center lg:justify-between p-fluid w-full">
                {/* Current status */}
                <div className="bg-[var(--color-surface)] shadow rounded-2xl p-fluid w-full sm:flex-1 flex flex-col justify-between">
                    {loadingDevices || loadingData ? <LoadingSpinner /> : (() => {
                        const latest = latestSensorData[selectedDeviceId!];
                        if (!latest || fields.some(f => latest[f] == null || latest[f] === 0)) return <p
                            className="text-center text-[--color-primary]">No data available</p>;
                        return (
                            <>
                                <div className="flex justify-between mb-2">
                                    <h2 className="font-bold text-fluid-header">Current Status</h2>
                                    <span className="text-fluid text-muted-foreground text-[--color-primary]">
                                    Last updated:&nbsp;{formatDateTimeForUserTZ(latest.time)}
                                    </span>
                                </div>
                                <div className="grid grid-cols-2 sm:grid-cols-4 gap-fluid text-fluid">
                                    {fields.map(f => {
                                        const key = f as TabKey;
                                        let value = (latest as any)[f];

                                        if (key === "temperature" && typeof value === "number") {
                                            value = convert(value);
                                        }

                                        return (
                                            <div key={f} className="flex">
                                                <span className="capitalize font-medium">{f}:</span>
                                                <span className="ml-1">
                                                    {typeof value === "number" ? value.toFixed(2) : "–"}{unitMap[key]}
                                                </span>
                                            </div>
                                        );
                                    })}
                                </div>
                            </>
                        );
                    })()}
                </div>
            </div>

            {/* Charts */}
            <main className="flex-1 overflow-y-auto p-fluid">
                <div className="bg-[var(--color-surface)] shadow rounded-2xl">
                    <div className="px-4 pt-4 flex flex-wrap items-center gap-4 sm:gap-6">

                        {/* Device selector */}
                        <div className="flex items-center gap-2">
                            <label className="font-medium text-fluid">Device:</label>
                            <select
                                className="select bg-[var(--color-surface)] text-fluid"
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
                            <label className="font-medium text-fluid">From:</label>
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
                                className="input input-xs bg-[var(--color-surface)] ml-1 text-fluid"
                            />

                            <label className="font-medium text-fluid">To:</label>
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
                                className="input input-xs bg-[var(--color-surface)] ml-1 text-fluid"
                            />
                        </div>
                    </div>

                    {/* tab bar */}
                    <div className="tabs tabs-bordered rounded-t-2xl">
                        {(Object.keys(pretty) as TabKey[]).map(key => (
                            <a
                                key={key}
                                className={`tab flex-1 tab-bordered${tab === key ? " tab-active" : ""} text-fluid`}
                                onClick={() => setTab(key)}
                            >
                                {pretty[key]}
                            </a>
                        ))}
                    </div>
                    <hr className="border-primary"/>

                    {/* active chart */}
                    <div className="p-fluid">
                        {loadingData ? <LoadingSpinner /> : noData ? (
                            <div className="p-8 text-center text-[--color-primary]">
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