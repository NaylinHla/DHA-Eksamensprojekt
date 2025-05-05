import {useEffect, useMemo, useRef, useState} from "react";
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
    formatDateTimeForUserTZ,
    GreenhouseSensorDataAtom,
    JwtAtom,
    SelectedDeviceIdAtom,
    SensorHistoryDto,
    StringConstants,
    UserDevice,
    useThrottle,
    useTopicManager,
    useWebSocketMessage,
} from "../import";
import {greenhouseDeviceClient} from "../../apiControllerClients.ts";

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

    function appendPointToChart(type: keyof typeof chartRefs, point: Point) {
        const chart = chartRefs[type].current;
        if (!chart) return;

        chart.data.labels.push(point.time);
        chart.data.datasets[0].data.push(point.value);

        // Only update new point, no animation
        chart.update({
            duration: 0,
            lazy: true,
        });
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

        logs.forEach((r) => {
            const t = formatDateTimeForUserTZ(r.time);
            if (!t) return;

            appendPointToChart("temperature", { time: t, value: Number(r.temperature) || 0 });
            appendPointToChart("humidity", { time: t, value: Number(r.humidity) || 0 });
            appendPointToChart("airPressure", { time: t, value: Number(r.airPressure) || 0 });
            appendPointToChart("airQuality", { time: t, value: Number(r.airQuality) || 0 });
        });
    }, 1000);

    useWebSocketMessage(StringConstants.ServerBroadcastsLiveDataToDashboard, (dto: any) => {
        const todayDate = new Date();
        todayDate.setHours(0, 0, 0, 0); // Normalize time
        const rangeToDate = new Date(rangeToRef.current);
        rangeToDate.setHours(23, 59, 59, 999);

        if (todayDate > rangeToDate) return;

        const newLogs: SensorHistoryDto[] = dto.logs?.[0]?.sensorHistoryRecords || [];
        if (!newLogs.length) return;

        const existing = new Set(
            greenhouseData.flatMap((d) =>
                d.sensorHistoryRecords?.map((r) => new Date(r.time!).getTime()) || []
            )
        );

        const unique = newLogs
            .filter((l) => !existing.has(new Date(l.time!).getTime()))
            .filter((l) => {
                const logDate = new Date(l.time!);
                const from = new Date(rangeFromRef.current);
                const to = new Date(rangeToRef.current);
                to.setHours(23, 59, 59, 999);
                return logDate >= from && logDate <= to;
            });

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
        greenhouseDeviceClient
            .getAllUserDevices(jwt)
            .then((res: any) => {
                const list = res.allUserDevice || [];
                setDevices(list);
                if (!selectedDeviceId && list.length) setSelectedDeviceId(list[0].deviceId!);
            })
            .catch(() => toast.error("Failed to load devices", {id: "load-devices-error"}))
            .finally(() => setLoadingDevices(false));
    }, [jwt]);

    // load history data
    useEffect(() => {
        if (!jwt || !selectedDeviceId) return;
        setLoadingData(true);

        const timer = setTimeout(() => {
            const startDate = new Date(rangeFrom);
            const endDate = new Date(rangeTo);
            endDate.setHours(23, 59, 59, 999);

            greenhouseDeviceClient
                .getAllSensorHistoryByDeviceAndTimePeriodIdDto(selectedDeviceId, startDate, endDate, jwt)
                .then((data) => {
                    setGreenhouseData(data);
                })
                .catch(() => toast.error("Failed to load sensor data", {id: "load-sensor-error"}))
                .finally(() => setLoadingData(false));
        }, 300); // 300ms debounce
        return () => clearTimeout(timer);
    }, [jwt, selectedDeviceId, rangeFrom, rangeTo]);

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

    //No data for selected date range
    const isEmpty =
        series.temperature.length === 0 &&
        series.humidity.length === 0 &&
        series.airPressure.length === 0 &&
        series.airQuality.length === 0;

    return (
        <div>
            {/* Filters */}
            <div className="flex flex-wrap items-start gap-4 lg:items-center lg:justify-between p-4 w-full">

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
                <div
                    className="border rounded p-4 shadow min-w-[260px] flex flex-col justify-between flex-grow sm:max-w-[300px]">

                    {loadingDevices || loadingData ? Spinner : (() => {
                        const recs = greenhouseData.find((d) => d.deviceId === selectedDeviceId)?.sensorHistoryRecords || [];
                        const latest = recs.reduce((a, b) => new Date(b.time!).getTime() > new Date(a.time!).getTime() ? b : a, recs[0]);
                        if (!latest) return <p>No data available</p>;
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
                                            <span>{(latest as Record<typeof field, number>)[field]?.toFixed(2)}</span>
                                        </div>
                                    ))}
                                </div>
                            </>
                        );
                    })()}
                </div>

                {/* Device selector */}
                <div
                    className="flex flex-col sm:flex-row sm:items-center gap-2 sm:w-auto justify-leg w-full min-w-[200px]">

                    <label className="font-medium">Select Device:</label>
                    <select
                        className="border rounded p-2"
                        value={selectedDeviceId || ""}
                        onChange={(e) => setSelectedDeviceId(e.target.value)}
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
