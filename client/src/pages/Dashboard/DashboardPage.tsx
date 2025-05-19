import { useSetAtom } from "jotai";
import { userSettingsClient } from "../../apiControllerClients";
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
    UserSettingsAtom,
} from "../import";
import {
    greenhouseDeviceClient,
    plantClient,
    userDeviceClient,
} from "../../apiControllerClients.ts";
import { cssVar } from "../../components/utils/Theme/theme.ts";
ChartJS.register(CategoryScale, LinearScale, Legend, Tooltip);
import { CardPlant } from "../../components/Modals/PlantCard";
import PlantCarousel from "../../components/Modals/PlantCarousel.tsx";

// Helpers
const greeting = () => {
    const h = new Date().getHours();
    if (h < 5) return "night";
    if (h < 12) return "morning";
    if (h < 18) return "afternoon";
    return "evening";
};

type PlantStatus = CardPlant & { needsWater: boolean };

export default function DashboardPage() {
    // Atoms
    const [jwt] = useAtom(JwtAtom);
    const [selectedDeviceId, setDeviceId] = useAtom(SelectedDeviceIdAtom);
    const [userSettings] = useAtom(UserSettingsAtom);
    const useCelsius = userSettings?.celsius ?? true;

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

    // Fetch latest greenhouse data
    useEffect(() => {
        if (!jwt) return;
        let cancelled = false;

        const loadLatest = async () => {
            setLoadingLive(true);
            try {
                const res  = await greenhouseDeviceClient.getRecentSensorDataForAllUserDevice(jwt);
                const recs = res?.sensorHistoryWithDeviceRecords ?? [];
                if (cancelled) return;

                setLatest(Object.fromEntries(recs.filter(r => r.deviceId).map(r => [r.deviceId!, r])));
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

    // Fetch weather data using Celsius or Fahrenheit
    useEffect(() => {
        (async () => {
            try {
                setLW(true);
                const { latitude, longitude } = { latitude: 55.6761, longitude: 12.5683 };
                const temperatureUnit = userSettings?.celsius === false ? "fahrenheit" : "celsius";

                const query = new URLSearchParams({
                    latitude: latitude.toString(),
                    longitude: longitude.toString(),
                    current: "temperature_2m",
                    hourly: "relativehumidity_2m",
                    temperature_unit: temperatureUnit,
                    timezone: "auto",
                });

                const url = `https://api.open-meteo.com/v1/forecast?${query.toString()}`;
                const response = await fetch(url);
                const json = await response.json();

                setWeather({
                    temp: json.current.temperature_2m,
                    humidity: json.hourly.relativehumidity_2m?.[0] ?? 0,
                });
            } catch {
                toast.error("Weather fetch failed");
            } finally {
                setLW(false);
            }
        })();
    }, [userSettings]);

    // Fetch plant list
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
                    const next = p.waterEvery != null ? Math.max(p.waterEvery - days, 0) : 0;
                    return {
                        id: p.plantId!,
                        name: p.plantName!,
                        nextWaterInDays: next,
                        isDead: p.isDead ?? false,
                        needsWater: next === 0,
                    } as PlantStatus;
                }));
            } catch {
                toast.error("Plant fetch failed");
            } finally {
                setLP(false);
            }
        })();
    }, [jwt]);

    // Switch to dark mode if darktheme = true
    useEffect(() => {
        if (!userSettings) return;

        const theme = userSettings.darkTheme ? "dark" : "light";
        document.documentElement.setAttribute("data-theme", theme);
        localStorage.setItem("theme", theme);
    }, [userSettings]);

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

            <div className="grid gap-6 px-6 md:grid-cols-3">
                <StatCard title="Temperature" loading={loadingWX}
                          value={weather ? `${Math.round(weather.temp)}°${useCelsius ? "C" : "F"}` : "–"} />
                <StatCard title="Humidity" loading={loadingWX}
                          value={`${Math.round(weather?.humidity ?? 0)}%`} />
                <StatCard title="Need Watering" loading={loadingPlants}
                          value={needsWater ? "Yes" : "No"} cls={needsWater ? "text-error":"text-success"} />
            </div>

            <main className="flex-1 flex flex-col lg:flex-row lg:items-stretch gap-6 px-6 py-6 overflow-y-auto">

                <div className="card w-full lg:flex-1 lg:basis-0 rounded-xl bg-[var(--color-surface)] shadow flex flex-col">
                    <div className="card-body">
                        <h3 className="text-lg font-semibold mb-6 text-center">Your Device:</h3>

                        {loadingLive ? (
                            <p className="text-center">Loading…</p>
                        ) : live ? (
                            <div className="grid grid-cols-2 sm:grid-cols-4 gap-6 place-items-center">
                                <CircleStat label="Temperature" unit={useCelsius ? "°C" : "°F"} color={cssVar("--color-primary")}
                                            value={live.temperature != null
                                                ? (useCelsius ? live.temperature : (live.temperature * 9 / 5 + 32))
                                                : null} />
                                <CircleStat label="Humidity" unit="%" color={cssVar("--color-success")}
                                            value={circleReadings.humidity} />
                                <CircleStat label="Pressure" unit="hPa" color={cssVar("--color-info")}
                                            value={circleReadings.pressure} />
                                <CircleStat label="Air Quality" unit="ppm" color={cssVar("--color-warning")}
                                            value={circleReadings.quality} />
                            </div>
                        ) : (
                            <p className="text-center text-gray-500">
                                {Object.keys(latest).length ? "No data" : "No devices connected"}
                            </p>
                        )}
                    </div>
                </div>

                <PlantCarousel className="lg:flex-1 lg:basis-0 flex flex-col" plants={plants}/>
            </main>
        </div>
    );
}

const StatCard: React.FC<{ title: string; loading: boolean; value: string; cls?: string; }> =
    ({ title, loading, value, cls = "" }) => (
        <div className="card shadow rounded-xl bg-[var(--color-surface)]">
            <div className="card-body text-center">
                <p className="text-lg">{title}</p>
                <p className={`text-5xl font-bold ${cls}`}>{loading ? "–" : value}</p>
            </div>
        </div>
    );

const CircleStat: React.FC<{ label: string; value: number | null; unit: string; color: string }> =
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
