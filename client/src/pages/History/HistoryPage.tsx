import React, {useEffect, useMemo, useRef, useState} from "react";
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
import {Line} from "react-chartjs-2";
import toast from "react-hot-toast";
import {useAtom} from "jotai";
import {
    formatDateTimeForUserTZ, GetRecentSensorDataForAllUserDeviceDto,
    GreenhouseSensorDataAtom,
    JwtAtom,
    SelectedDeviceIdAtom,
    SensorHistoryDto, SensorHistoryWithDeviceDto,
    StringConstants, TitleTimeHeader,
    UserDevice,
    useThrottle,
    useTopicManager,
    useWebSocketMessage,
} from "../import";
import {greenhouseDeviceClient, userDeviceClient} from "../../apiControllerClients.ts";

ChartJS.register(LineElement, PointElement, LinearScale, CategoryScale, ChartTooltip, ChartLegend, Filler);

type Point = { time: string; value: number; [key: string]: number | string };


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

    const chartRefs = {
        temperature: useRef<any>(null),
        humidity: useRef<any>(null),
        airPressure: useRef<any>(null),
        airQuality: useRef<any>(null),
    };

    const {subscribe, unsubscribe} = useTopicManager();
    const prevTopic = useRef<string | null>(null);

    const rangeFromRef = useRef(rangeFrom);
    const rangeToRef = useRef(rangeTo);

    useEffect(() => {
        rangeFromRef.current = rangeFrom;
        rangeToRef.current = rangeTo;
    }, [rangeFrom, rangeTo]);

    function appendPointToChart(
        chartRef: React.RefObject<ChartJS>,
        point: { time: string; value: number },
        maxPoints = 500
    ) {
        const chart = chartRef.current;
        if (!chart) return;

        // Ensure arrays exist
        if (!Array.isArray(chart.data.labels)) {
            chart.data.labels = [];
        }
        if (!Array.isArray(chart.data.datasets[0].data)) {
            chart.data.datasets[0].data = [];
        }

        // 1) Append new
        chart.data.labels.push(point.time);
        chart.data.datasets[0].data.push(point.value);

        // 2) Pop oldest if over limit
        if (chart.data.labels.length > maxPoints) {
            chart.data.labels.shift();
            chart.data.datasets[0].data.shift();
        }

        // 3) Redraw instantly
        chart.update('none');
    }

    const throttledAppend = useThrottle((logs: SensorHistoryDto[]) => {
        if (logs.length === 0) return;

        setGreenhouseData((prev) => {
            return prev.map((d) =>
                d.deviceId === selectedDeviceId
                    ? {
                        ...d,
                        sensorHistoryRecords: [...(d.sensorHistoryRecords || []), ...logs]
                    }
                    : d
            );
        });

        logs.forEach(r => {
            const t = formatDateTimeForUserTZ(r.time);
            if (!t) return;

            appendPointToChart(chartRefs.temperature,  { time: t, value: Number(r.temperature) });
            appendPointToChart(chartRefs.humidity,     { time: t, value: Number(r.humidity)    });
            appendPointToChart(chartRefs.airPressure,  { time: t, value: Number(r.airPressure) });
            appendPointToChart(chartRefs.airQuality,   { time: t, value: Number(r.airQuality)  });
        });
    }, 1000);

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

        const inRange = logs.filter(log => {
            const time = new Date(log.time!);
            return time >= from && time <= to;
        });

        if (!inRange.length) return;

        const existing = new Set(
            greenhouseData.flatMap((d) =>
                d.sensorHistoryRecords?.map((r) => new Date(r.time!).getTime()) || []
            )
        );

        const unique = inRange.filter((l) => !existing.has(new Date(l.time!).getTime()));
        if (unique.length) throttledAppend(unique);
    });

    // subscribe to topic changes
    useEffect(() => {
        if (!selectedDeviceId) return;
        const topic = `GreenhouseSensorData/${selectedDeviceId}`;
        // Unsubscribe from the previous device if it's different from the new one
        if (prevTopic.current && prevTopic.current !== selectedDeviceId) {
            unsubscribe(`GreenhouseSensorData/${prevTopic.current}`).then();
        }
        subscribe(topic).then();
        prevTopic.current = selectedDeviceId;
        return () => void unsubscribe(topic);
    }, [selectedDeviceId]);

    // Fetch devices
    useEffect(() => {
        if (!jwt) return;
        setLoadingDevices(true);
        userDeviceClient
            .getAllUserDevices(jwt)
            .then((res: any) => {
                const list = Array.isArray(res) ? res : [];
                setDevices(list);
                if (!selectedDeviceId && list.length) {setSelectedDeviceId(list[0].deviceId!);}
            })
            .catch(() => toast.error("Failed to load devices", {id: "load-devices-error"}))
            .finally(() => setLoadingDevices(false));
    }, [jwt]);

    // load history data
    useEffect(() => {
        if (!jwt || !selectedDeviceId) return;
        setLoadingData(true);

        const timer = setTimeout(() => {
            const [fromYear, fromMonth, fromDay] = rangeFrom.split("-").map(Number);
            const [toYear, toMonth, toDay] = rangeTo.split("-").map(Number);

            // Local time at 00:00
            const localStart = new Date(fromYear, fromMonth - 1, fromDay, 0, 0, 0, 0);
            const localEnd = new Date(toYear, toMonth - 1, toDay, 23, 59, 59, 999);

            // Convert local time to UTC by using .toISOString() and building Date from that
            const utcStart = new Date(localStart.toISOString()); // This is already UTC
            const utcEnd = new Date(localEnd.toISOString());

            greenhouseDeviceClient
                .getAllSensorHistoryByDeviceAndTimePeriodIdDto(selectedDeviceId, utcStart, utcEnd, jwt)
                .then((data) => {
                    setGreenhouseData(data);
                })
                .catch(() => toast.error("Failed to load sensor data", {id: "load-sensor-error"}))
                .finally(() => setLoadingData(false));
        }, 300); // 300ms debounce
        return () => clearTimeout(timer);
    }, [jwt, selectedDeviceId, rangeFrom, rangeTo]);

    // load recent data
    useEffect(() => {
        if (!jwt) return;

        greenhouseDeviceClient
            .getRecentSensorDataForAllUserDevice(jwt)
            .then((res: GetRecentSensorDataForAllUserDeviceDto | null) => {
                if (!res || !res.sensorHistoryWithDeviceRecords || res.sensorHistoryWithDeviceRecords.length === 0) {
                    setLatestSensorData({});
                    return; //If server responded with 204 No Content, res might be null or empty object
                }
                const records = res.sensorHistoryWithDeviceRecords;
                const snapshot = records.reduce((acc, curr) => {
                    if (curr.deviceId) acc[curr.deviceId] = curr;
                    return acc;
                }, {} as Record<string, SensorHistoryWithDeviceDto>);

                setLatestSensorData(snapshot);
            })
            .catch(() => toast.error("Failed to load recent sensor data", { id: "load-sensor-error" }));
    }, [jwt, selectedDeviceId]);

    function downSampleRecords<T>(arr: T[], maxPoints = 500): T[] {
        const n = arr.length;
        if (n <= maxPoints) return arr;
        const step = Math.floor(n / maxPoints);
        const res: T[] = [];
        for (let i = 0; i < n; i += step) res.push(arr[i]);
        return res;
    }

    // sync, down sampled series with timing logs
    const series = useMemo(() => {
        const recs = greenhouseData.find(d => d.deviceId === selectedDeviceId)?.sensorHistoryRecords || [];
        if (!recs.length) return {temperature: [], humidity: [], airPressure: [], airQuality: []};

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

        // 2) Down sample those unique records
        const sampledRecs = downSampleRecords(unique, 500);

        // 3) Build each metric series
        const temperatureSeries: Point[] = [];
        const humiditySeries: Point[] = [];
        const airPressureSeries: Point[] = [];
        const airQualitySeries: Point[] = [];

        sampledRecs.forEach(r => {
            const t = formatDateTimeForUserTZ(r.time);
            if (!t) return;
            temperatureSeries.push({time: t, value: Number(r.temperature) || 0});
            humiditySeries.push({time: t, value: Number(r.humidity) || 0});
            airPressureSeries.push({time: t, value: Number(r.airPressure) || 0});
            airQualitySeries.push({time: t, value: Number(r.airQuality) || 0});
        });

        return {
            temperature: temperatureSeries,
            humidity: humiditySeries,
            airPressure: airPressureSeries,
            airQuality: airQualitySeries,
        };
    }, [greenhouseData, selectedDeviceId, rangeFrom, rangeTo]);

    const Spinner = (
        <div className="flex justify-center items-center h-32">
            <svg className="animate-spin h-8 w-8 mr-3 text-gray-500" viewBox="0 0 24 24">
                <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"
                        fill="none"/>
                <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8v4a4 4 0 00-4 4H4z"/>
            </svg>
            <span className="text-gray-500">Loading…</span>
        </div>
    );

    // Function to get the CSS variable value
    const getCSSVar = (name: string) => getComputedStyle(document.documentElement).getPropertyValue(name);
    const primaryColor = getCSSVar('--color-primary');
    const primaryHoverColor = getCSSVar('--color-primaryhover');

    const Chart = ({data, label, chartRef}: { data: Point[]; label: string; chartRef: any }) =>
        data.length ? (
            <div className="mb-10 px-2">
                <h2 className="text-xl font-semibold mb-2">{label}</h2>
                <Line
                    ref={chartRef}
                    data={{
                        labels: data.map((point) => point.time),
                        datasets: [
                            {
                                label,
                                data: data.map((point) => point.value),
                                fill: true,
                                backgroundColor: primaryColor,
                                borderColor: primaryHoverColor,
                                tension: 0.4,
                                pointRadius: 0,
                            },
                        ],
                    }}
                    options={{
                        responsive: true,
                        animation: false,
                        plugins: {
                            legend: {display: true},
                            tooltip: {mode: "index", intersect: false},
                        },
                        interaction: {
                            mode: "nearest" as const,
                            axis: "x" as const,
                            intersect: false,
                        },
                        scales: {
                            x: {
                                title: {display: true, text: "Time"},
                                ticks: {maxTicksLimit: 10, autoSkip: true},
                            },
                            y: {title: {display: true, text: label}},
                        },
                    }}
                />
            </div>
        ) : <div className="text-gray-500">No {label} data</div>;

    //Make sure the range is set logical
    const handleDateChange = (e: React.ChangeEvent<HTMLInputElement>, isFromDate: boolean) => {
        const newDate = e.target.value;

        if (isFromDate) {
            if (new Date(newDate) > new Date(rangeTo)) {
                toast.error("From date cannot be after To date", { id: "date-range-error" });
                return;
            }
            setRangeFrom(newDate);
        } else {
            if (new Date(newDate) < new Date(rangeFrom)) {
                toast.error("To date cannot be before From date", { id: "date-range-error" });
                return;
            }
            setRangeTo(newDate);
        }
    };

    //No data for selected date range
    const isEmpty =
        series.temperature.length === 0 &&
        series.humidity.length === 0 &&
        series.airPressure.length === 0 &&
        series.airQuality.length === 0;

    //Show unit in the current data box
    const unitMap: Record<string, string> = {
        temperature: "°C",
        humidity: "%",
        airPressure: "hPa",
        airQuality: "ppm",
    }

    const fields: (keyof SensorHistoryWithDeviceDto)[] = ["temperature", "humidity", "airPressure", "airQuality"];

    return (
        <div>
            {/* Filters */}
            <div>
                <TitleTimeHeader title="Overview" />
            </div>

            <div className="flex flex-wrap items-start gap-4 lg:items-center lg:justify-between p-4 w-full">
                <div className="flex gap-2">
                    <label>From:
                        <input
                            type="date"
                            value={rangeFrom}
                            onChange={(e) => handleDateChange(e, true)}
                            className="border ml-1 p-1 rounded"
                        />
                    </label>
                    <label>To:
                        <input
                            type="date"
                            value={rangeTo}
                            onChange={(e) => handleDateChange(e, false)}
                            className="border ml-1 p-1 rounded"
                        />
                    </label>
                </div>
                {/* Current Status Box */}
                <div
                    className="border rounded p-4 shadow min-w-[260px] flex flex-col justify-between flex-grow sm:max-w-[355px] h-[114px]">
                    {loadingDevices || loadingData ? (
                        Spinner
                    ) : (() => {
                        const latest = latestSensorData[selectedDeviceId!];

                        if (!latest || fields.some((field) => latest[field] === 0.00 || latest[field] == null)) {
                            return (
                                <div className="flex justify-center items-center h-full">
                                    <p className="text-center">No data available</p>
                                </div>
                            );
                        }

                        return (
                            <>
                                <div className="flex justify-between mb-2">
                                    <h2 className="font-bold">Current Status</h2>
                                    <span
                                        className="text-xs text-gray-500">{formatDateTimeForUserTZ(latest.time)}</span>
                                </div>
                                <div className="grid grid-cols-2 gap-2 text-sm">
                                    {(["temperature", "humidity", "airPressure", "airQuality"] as const).map((field) => (
                                        <div key={field} className="flex justify-between">
                                            <span className="capitalize font-medium">{field}:</span>
                                            <span>{(latest as any)[field]?.toFixed(2)} {unitMap[field]}</span>
                                        </div>
                                    ))}
                                </div>
                            </>
                        );
                    })()}
                </div>

                {/* Device selector */}
                <div
                    className="flex flex-col sm:flex-row sm:items-center gap-2 sm:w-auto w-full min-w-[200px]">

                    <label className="font-medium">Select Device:</label>
                    <select
                        className="border rounded p-2 min-w-[120px] max-w-[200px] truncate"
                        value={selectedDeviceId || ""}
                        onChange={(e) => setSelectedDeviceId(e.target.value)}
                        disabled={loadingDevices}
                    >
                        {devices.length === 0 ? (
                            <option>No devices</option>
                        ) : (
                            devices.map(d => (
                                <option key={d.deviceId} value={d.deviceId} title={d.deviceName ?? 'Unnamed Device'}>
                                    {(d.deviceName?.length ?? 0) > 15 ? `${d.deviceName?.slice(0, 15)}...` : d.deviceName ?? 'Unnamed Device'}
                                </option>
                            ))
                        )}
                    </select>
                </div>

            </div>

            {loadingData ? Spinner : isEmpty ? (
                <div className="p-8 text-center text-gray-500">
                    No data available for the selected date range.
                </div>
            ) : (
                <>
                    <Chart data={series.temperature} label="Temperature" chartRef={chartRefs.temperature}/>
                    <Chart data={series.humidity} label="Humidity" chartRef={chartRefs.humidity}/>
                    <Chart data={series.airPressure} label="Air Pressure" chartRef={chartRefs.airPressure}/>
                    <Chart data={series.airQuality} label="Air Quality" chartRef={chartRefs.airQuality}/>
                </>
            )}
        </div>
    );
}
