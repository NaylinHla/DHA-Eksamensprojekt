import React, { useEffect, useMemo, useState } from "react";
import {
    CategoryScale,
    LinearScale,
    Legend,
    Tooltip,
    Chart as ChartJS,
} from "chart.js";
import toast from "react-hot-toast";
import { useAtom } from "jotai";
import {
    TitleTimeHeader,
    JwtAtom,
    SelectedDeviceIdAtom,
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
ChartJS.register(CategoryScale, LinearScale, Legend, Tooltip);

// Helpers
const greeting = () => {
    const h = new Date().getHours();
    if (h < 5) return "night";
    if (h < 12) return "morning";
    if (h < 18) return "afternoon";
    return "evening";
};

type PlantStatus = { id: string; name: string; needsWater: boolean };

export default function DashboardPage() {
    // Atoms
    const [jwt] = useAtom(JwtAtom);
    const [selectedDeviceId, setDeviceId] = useAtom(SelectedDeviceIdAtom);

    // States
    const [devices,       setDevices]       = useState<UserDevice[]>([]);
    const [loadingDev,    setLD]            = useState(true);

    const [weather,       setWeather]       = useState<{ temp: number; humidity: number } | null>(null);
    const [loadingWX,     setLW]            = useState(true);

    const [plants,        setPlants]        = useState<PlantStatus[]>([]);
    const [loadingPlants, setLP]            = useState(true);

    /* latest snapshot for circles */
    const [latest,        setLatest]        = useState<Record<string, SensorHistoryWithDeviceDto>>({});
    const [loadingLive,   setLoadingLive]   = useState(true);
    
    // Fetch Devices
    useEffect(() => {
        if (!jwt) return;
        setLD(true);
        userDeviceClient
            .getAllUserDevices(jwt)
            .then(list => setDevices(Array.isArray(list) ? list : []))
            .catch(() => toast.error("Failed to load devices"))
            .finally(() => setLD(false));
    }, [jwt]);

    // Fetch latest snapshot of device readings
    useEffect(() => {
        if (!jwt) return;
        let cancelled = false;

        const loadLatest = async () => {
            setLoadingLive(true);
            try {
                const res  = await greenhouseDeviceClient.getRecentSensorDataForAllUserDevice(jwt);
                const recs = res?.sensorHistoryWithDeviceRecords ?? [];
                if (cancelled) return;

                /* map deviceId -> latest record */
                setLatest(Object.fromEntries(recs.filter(r => r.deviceId).map(r => [r.deviceId!, r])));

                /* pick a default device on first run */
                if (!selectedDeviceId && recs.length) setDeviceId(recs[0].deviceId!);
            } catch {
                toast.error("Failed to load latest greenhouse data");
            } finally {
                if (!cancelled) setLoadingLive(false);
            }
        };

        loadLatest();
        const id = setInterval(loadLatest, 30_000);
        return () => { cancelled = true; clearInterval(id); };
    }, [jwt]);

    // Fetch Weather outside
    useEffect(() => {
        (async () => {
            try {
                setLW(true);
                const { latitude, longitude } = { latitude: 55.6761, longitude: 12.5683 };
                const url = `https://api.open-meteo.com/v1/forecast?latitude=${latitude}&longitude=${longitude}` +
                    `&current_weather=true&hourly=relativehumidity_2m&timezone=auto`;
                const json = await (await fetch(url)).json();
                setWeather({ temp: json.current_weather.temperature,
                    humidity: json.hourly.relativehumidity_2m?.[0] ?? 0 });
            } catch { toast.error("Weather fetch failed"); }
            finally { setLW(false); }
        })();
    }, []);

    // Fetch Plants
    useEffect(() => {
        if (!jwt) return;
        (async () => {
            try {
                setLP(true);
                const { sub, Id } = JSON.parse(atob(jwt.split(".")[1]));
                const uid = (sub || Id) ?? "";
                const list = await plantClient.getAllPlants(uid, jwt);
                setPlants(list.map((p: any) => {
                    const days = p.lastWatered
                        ? Math.floor((Date.now() - new Date(p.lastWatered).getTime()) / 86_400_000)
                        : Number.MAX_SAFE_INTEGER;
                    return { id: p.plantId!, name: p.plantName!, needsWater: p.waterEvery != null && days >= p.waterEvery };
                }));
            } catch { toast.error("Plant fetch failed"); }
            finally  { setLP(false); }
        })();
    }, [jwt]);

    const needsWater = plants.some(p => p.needsWater);

    // Live data reading for circles
    const live = latest[selectedDeviceId ?? ""];

    const circleReadings = useMemo(() => ({
        temperature: live?.temperature ?? null,
        humidity   : live?.humidity ?? null,
        pressure   : live?.airPressure ?? null,
        quality    : live?.airQuality ?? null,
    }), [live]);

    const greet = greeting();
    
    return (
        <div className="min-h-[calc(100vh-64px)] flex flex-col font-display">
            <TitleTimeHeader title="Dashboard" />

            {/* greeting */}
            <h2 className="text-2xl font-bold px-6 pt-4 pb-2">{`Good ${greet}!`}</h2>

            {/* stat cards */}
            <div className="grid gap-6 px-6 md:grid-cols-3">
                <StatCard title="Temperature" loading={loadingWX} value={`${Math.round(weather?.temp ?? 0)}°C`} />
                <StatCard title="Humidity"    loading={loadingWX} value={`${Math.round(weather?.humidity ?? 0)}%`} />
                <StatCard title="Need Watering" loading={loadingPlants}
                          value={needsWater ? "Yes" : "No"} cls={needsWater ? "text-error":"text-success"} />
            </div>

            {/* main row */}
            <main className="flex-1 flex flex-col lg:flex-row gap-6 px-6 py-6 overflow-y-auto">

                {/* circle card */}
                <div className="flex-1 card rounded-xl bg-[var(--color-surface)] shadow">
                    <div className="card-body">
                        <h3 className="text-lg font-semibold mb-6 text-center">Your Device:</h3>

                        {loadingLive ? (
                            <p className="text-center">Loading…</p>
                        ) : live ? (
                            <div className="grid grid-cols-2 sm:grid-cols-4 gap-6 place-items-center">
                                <CircleStat label="Temperature"   unit="°C"  color={cssVar("--color-primary")}  value={circleReadings.temperature}/>
                                <CircleStat label="Humidity"    unit="%"   color={cssVar("--color-success")} value={circleReadings.humidity}/>
                                <CircleStat label="Pressure"  unit="hPa" color={cssVar("--color-info")}    value={circleReadings.pressure}/>
                                <CircleStat label="Air Quality"  unit="ppm" color={cssVar("--color-warning")} value={circleReadings.quality}/>
                            </div>
                        ) : (
                            <p className="text-center text-gray-500">
                                {Object.keys(latest).length ? "No data" : "No devices connected"}
                            </p>
                        )}
                    </div>
                </div>

                {/* plant carousel */}
                <PlantCarousel plants={(plants.some(p => p.needsWater) ? plants.filter(p => p.needsWater) : plants).slice(0,2)}/>
            </main>
        </div>
    );
}

// Sub Components
const StatCard: React.FC<{ title:string; loading:boolean; value:string; cls?:string; }> =
    ({ title, loading, value, cls="" }) => (
        <div className="card shadow rounded-xl bg-[var(--color-surface)]">
            <div className="card-body text-center">
                <p className="text-lg">{title}</p>
                <p className={`text-5xl font-bold ${cls}`}>{loading ? "–" : value}</p>
            </div>
        </div>
    );

const CircleStat: React.FC<{ label:string; value:number|null; unit:string; color:string }> =
    ({ label, value, unit, color }) => (
        <div className="flex flex-col items-center">
            <div className="relative w-28 h-28 rounded-full border-4 flex items-center justify-center" style={{ borderColor: color }}>
                <span className="text-xl font-bold select-none">
                    {value == null ? "—" : `${value.toFixed(1)}${unit}`}
                </span>
            </div>
            <p className="mt-2 text-sm text-center text-gray-500">{label}</p>
        </div>
    );

const PlantCarousel: React.FC<{ plants: PlantStatus[] }> = ({ plants }) => {
    if (!plants.length)
        return (
            <div className="w-full lg:w-80 card bg-base-100 shadow flex items-center justify-center">
                <p className="text-gray-500">No plants</p>
            </div>
        );

    return (
        <div className="w-full lg:w-80 card rounded-xl bg-[var(--color-surface)] shadow flex flex-col">
            <div className="card-body pb-4">
                <h3 className="text-lg font-semibold text-center">Plants</h3>
                <div className="carousel w-full">
                    {plants.map((p,i) => (
                        <div key={p.id} id={`p-${i}`}
                             className="carousel-item w-full flex flex-col items-center">
                            <p className="text-xl font-medium mt-4 mb-1">{p.name}</p>
                            <p className={`text-2xl font-bold ${p.needsWater ? "text-error":"text-success"}`}>{p.needsWater ? "Needs water" : "Okay"}</p>
                        </div>
                    ))}
                </div>
                {plants.length > 1 && (
                    <div className="flex justify-center gap-2 mt-2">
                        {plants.map((_,i) => (
                            <a key={i} href={`#p-${i}`} className="btn btn-xs btn-circle">{i+1}</a>
                        ))}
                    </div>
                )}
            </div>
        </div>
    );
}
