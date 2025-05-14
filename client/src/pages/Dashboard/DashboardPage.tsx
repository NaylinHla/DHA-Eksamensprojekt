import React, { useEffect, useMemo, useState } from "react";
import {
    CategoryScale,
    Chart as ChartJS,
    LinearScale,
    LineElement,
    PointElement,
    Tooltip,
    Legend,
} from "chart.js";
import { Line } from "react-chartjs-2";
import toast from "react-hot-toast";
import { useAtom } from "jotai";
import {
    TitleTimeHeader,
    JwtAtom,
    SelectedDeviceIdAtom,
    GreenhouseSensorDataAtom,
    SensorHistoryDto,
    SensorHistoryWithDeviceDto,
    UserDevice,
} from "../import";
import {
    greenhouseDeviceClient,
    plantClient,
    userDeviceClient,
} from "../../apiControllerClients.ts";
import { cssVar } from "../../components/utils/Theme/theme.ts";
ChartJS.register(
    CategoryScale,
    LinearScale,
    PointElement,
    LineElement,
    Tooltip,
    Legend,
);

// Helpers
const greeting = () => {
    const h = new Date().getHours();
    if (h < 5) return "night";
    if (h < 12) return "morning";
    if (h < 18) return "afternoon";
    return "evening";
};

type PlantStatus = { id: string; name: string; needsWater: boolean };
type Point = { time: string; value: number };


export default function DashboardPage() {
    // Atoms
    const [jwt] = useAtom(JwtAtom);
    const [selectedDeviceId, setDeviceId] = useAtom(SelectedDeviceIdAtom);

    // States
    const [devices, setDevices] = useState<UserDevice[]>([]);
    const [loadingDev, setLD] = useState(true);
    const [weather, setWeather] = useState<{
        temp: number;
        humidity: number;
    } | null>(null);
    const [loadingWX, setLW] = useState(true);
    const [plants, setPlants] = useState<PlantStatus[]>([]);
    const [loadingPlants, setLP] = useState(true);
    const [series, setSeries] = useState<Point[]>([]);
    const [loadingChart, setLC] = useState(true);
    const [latest, setLatest] = useState<
        Record<string, SensorHistoryWithDeviceDto>
    >({});

    // Fetch devices list
    useEffect(() => {
        if (!jwt) return;
        setLD(true);
        userDeviceClient
            .getAllUserDevices(jwt)
            .then((raw) => {
                const list = Array.isArray(raw) ? (raw as any) : [];
                setDevices(list);
            })
            .catch(() => toast.error("Failed to load devices"))
            .finally(() => setLD(false));
    }, [jwt]);

    // Fetch recent greenhouse data
    useEffect(() => {
        if (!jwt) return;

        let cancelled = false;

        async function loadLatest() {
            try {
                const res =
                    await greenhouseDeviceClient.getRecentSensorDataForAllUserDevice(jwt);

                const recs = res?.sensorHistoryWithDeviceRecords ?? [];
                if (cancelled) return;

                /* map deviceId -> latest record */
                const map = Object.fromEntries(
                    recs.filter((r) => r.deviceId).map((r) => [r.deviceId!, r]),
                );
                setLatest(map);

                /* pick first device that has data (only once) */
                if (!selectedDeviceId && recs.length) {
                    setDeviceId(recs[0].deviceId!);
                }
            } catch {
                toast.error("Failed to load latest greenhouse data");
            }
        }

        loadLatest();
        const id = setInterval(loadLatest, 30_000); // refresh every 30 s
        return () => {
            cancelled = true;
            clearInterval(id);
        };
    }, [jwt]);

    // Weather outside
    useEffect(() => {
        (async () => {
            try {
                setLW(true);
                const { latitude, longitude } = { latitude: 55.6761, longitude: 12.5683 }; // Copenhagen
                const res = await fetch(
                    `https://api.open-meteo.com/v1/forecast?latitude=${latitude}&longitude=${longitude}`
                    + `&current_weather=true&hourly=relativehumidity_2m&timezone=auto`,
                );
                const json = await res.json();
                setWeather({
                    temp: json.current_weather.temperature,
                    humidity: json.hourly.relativehumidity_2m?.[0] ?? 0,
                });
            } catch {
                toast.error("Weather fetch failed");
            } finally {
                setLW(false);
            }
        })();
    }, []);

    // Plants
    useEffect(() => {
        if (!jwt) return;
        (async () => {
            try {
                setLP(true);
                const { sub, Id } = JSON.parse(atob(jwt.split(".")[1]));
                const uid = (sub || Id) ?? "";
                const list = await plantClient.getAllPlants(uid, jwt);
                const mapped: PlantStatus[] = list.map((p: any) => {
                    const daysSinceWater = p.lastWatered
                        ? Math.floor(
                            (Date.now() - new Date(p.lastWatered).getTime()) / 86_400_000,
                        )
                        : Number.MAX_SAFE_INTEGER;
                    return {
                        id: p.plantId!,
                        name: p.plantName!,
                        needsWater: p.waterEvery != null && daysSinceWater >= p.waterEvery,
                    };
                });
                setPlants(mapped);
            } catch {
                toast.error("Plant fetch failed");
            } finally {
                setLP(false);
            }
        })();
    }, [jwt]);

    const needsWater = useMemo(() => plants.some((p) => p.needsWater), [plants]);

    // Today's chart
    useEffect(() => {
        // Stop early if we don’t have both a token and a chosen device
        if (!jwt || !selectedDeviceId) {
            setSeries([]);
            setLC(false);
            return;
        }

        (async () => {
            try {
                setLC(true);
                const now       = new Date();
                const startUTC  = new Date(Date.UTC(
                    now.getUTCFullYear(),
                    now.getUTCMonth(),
                    now.getUTCDate(),
                    0, 0, 0, 0
                ));
                const endUTC    = new Date(Date.UTC(
                    now.getUTCFullYear(),
                    now.getUTCMonth(),
                    now.getUTCDate(),
                    23, 59, 59, 999
                ));

                const res = await greenhouseDeviceClient
                    .getAllSensorHistoryByDeviceAndTimePeriodIdDto(
                        selectedDeviceId,
                        startUTC,
                        endUTC,
                        jwt
                    );
                
                let recs: SensorHistoryDto[] =
                    res.find(r => r.deviceId === selectedDeviceId)
                        ?.sensorHistoryRecords ?? [];

                if (recs.length === 0 && latest[selectedDeviceId]) {
                    recs = [{
                        temperature : latest[selectedDeviceId].temperature,
                        time        : latest[selectedDeviceId].time,
                    }] as SensorHistoryDto[];
                }

                const pts: Point[] = recs.map(r => ({
                    time : (r.time instanceof Date
                        ? r.time
                        : new Date(r.time!)).toISOString(),
                    value: Number(r.temperature),
                }));

                setSeries(pts);
            } catch {
                toast.error("Sensor fetch failed");
            } finally {
                setLC(false);
            }
        })();
    }, [jwt, selectedDeviceId, latest]);

    // Chart config
    const chartConfig = useMemo(() => {
        if (!series.length) return null;
        return {
            labels: series.map((p) => p.time),
            datasets: [{
                label: "Temperature (°C)",
                data: series.map((p) => p.value),
                tension: 0.3,
                borderWidth: 2,
                pointRadius: 0,
                borderColor: cssVar("--color-primary"),
            }],
        };
    }, [series]);
    
    const twoPlants =
        (plants.filter((p) => p.needsWater).slice(0, 2).length
            ? plants.filter((p) => p.needsWater)
            : plants).slice(0, 2);

    const greet = greeting();

    return (
        <div className="min-h-[calc(100vh-64px)] flex flex-col bg-base-200 font-display">
            <TitleTimeHeader title="Dashboard" />

            {/* greeting */}
            <h2 className="text-2xl font-bold px-6 pt-4 pb-2">
                {`Good ${greet}!`}
            </h2>

            {/* stat cards */}
            <div className="grid gap-6 px-6 md:grid-cols-3">
                <StatCard
                    title="Temperature"
                    loading={loadingWX}
                    value={`${Math.round(weather?.temp ?? 0)}°C`}
                />
                <StatCard
                    title="Humidity"
                    loading={loadingWX}
                    value={`${Math.round(weather?.humidity ?? 0)}%`}
                />
                <StatCard
                    title="Need Watering"
                    loading={loadingPlants}
                    value={needsWater ? "Yes" : "No"}
                    cls={needsWater ? "text-error" : "text-success"}
                />
            </div>

            {/* main row */}
            <main className="flex-1 flex flex-col lg:flex-row gap-6 px-6 py-6 overflow-y-auto">

                {/* chart card */}
                <div className="flex-1 card rounded-xl bg-[var(--color-surface)] shadow">
                    <div className="card-body">

                        <h3 className="text-lg font-semibold mb-3">
                            Today’s Readings
                        </h3>

                        {loadingChart ? (
                            <p className="text-center">Loading…</p>
                        ) : chartConfig ? (
                            <div className="h-64">
                                <Line
                                    data={chartConfig}
                                    options={{
                                        responsive: true,
                                        maintainAspectRatio: false,
                                        animation: false,
                                        plugins: { legend: { display: false } },
                                        scales: {
                                            x: { grid: { display: false }, ticks: { maxTicksLimit: 10 } },
                                            y: { grid: { display: false } },
                                        },
                                    }}
                                />
                            </div>
                        ) : (
                            <p className="text-center text-gray-500">
                                {Object.keys(latest).length ? "No data" : "No devices connected"}
                            </p>
                        )}
                    </div>
                </div>

                {/* plant carousel */}
                <PlantCarousel plants={twoPlants} />
            </main>
        </div>
    );
}

// Sub-Components
const StatCard: React.FC<{
    title: string;
    loading: boolean;
    value: string;
    cls?: string;
}> = ({ title, loading, value, cls = "" }) => (
    <div className="card shadow rounded-xl bg-[var(--color-surface)]">
        <div className="card-body text-center ">
            <p className="text-lg">{title}</p>
            <p className={`text-5xl font-bold ${cls}`}>{loading ? "–" : value}</p>
        </div>
    </div>
);

// Quick Plant Carousel (needs updating)
const PlantCarousel: React.FC<{ plants: PlantStatus[] }> = ({ plants }) => {
    if (!plants.length) {
        return (
            <div className="w-full lg:w-80 card bg-base-100 shadow flex items-center justify-center">
                <p className="text-gray-500">No plants</p>
            </div>
        );
    }
    return (
        <div className="w-full lg:w-80 card rounded-xl bg-[var(--color-surface)] shadow flex flex-col">
            <div className="card-body pb-4">
                <h3 className="text-lg font-semibold text-center">Plants</h3>
                <div className="carousel w-full">
                    {plants.map((p, i) => (
                        <div
                            key={p.id}
                            id={`p-${i}`}
                            className="carousel-item w-full flex flex-col items-center"
                        >
                            <p className="text-xl font-medium mt-4 mb-1">{p.name}</p>
                            <p
                                className={`text-2xl font-bold ${
                                    p.needsWater ? "text-error" : "text-success"
                                }`}
                            >
                                {p.needsWater ? "Needs water" : "Okay"}
                            </p>
                        </div>
                    ))}
                </div>
                {plants.length > 1 && (
                    <div className="flex justify-center gap-2 mt-2">
                        {plants.map((_, i) => (
                            <a
                                key={i}
                                href={`#p-${i}`}
                                className="btn btn-xs btn-circle"
                            >
                                {i + 1}
                            </a>
                        ))}
                    </div>
                )}
            </div>
        </div>
    );
};