import React, { useEffect, useMemo, useState } from "react";
import toast from "react-hot-toast";
import { useAtom } from "jotai";
import {
    CategoryScale,
    LinearScale,
    Legend,
    Tooltip,
    Chart as ChartJS,
} from "chart.js";
import { useDisplayTemperature } from "../../hooks";
import {
    TitleTimeHeader,
    JwtAtom,
    SelectedDeviceIdAtom,
    SensorHistoryWithDeviceDto,
    UserDevice,
    StatCard,
    CircleStatGrid,
    CircleStat,
    PlantCarousel,
    UserSettingsAtom,
} from "../import";
import {
    greenhouseDeviceClient,
    plantClient,
    userDeviceClient,
} from "../../apiControllerClients.ts";
import { CardPlant } from "../../components/Plants/PlantCard.tsx";

ChartJS.register(CategoryScale, LinearScale, Legend, Tooltip);

const greeting = () => {
    const h = new Date().getHours();
    if (h < 5) return "night";
    if (h < 12) return "morning";
    if (h < 18) return "afternoon";
    return "evening";
};

type PlantStatus = CardPlant & { needsWater: boolean };

export default function DashboardPage() {

    const [jwt] = useAtom(JwtAtom);
    const [selectedDeviceId, setDeviceId] = useAtom(SelectedDeviceIdAtom);
    const [userSettings] = useAtom(UserSettingsAtom);
    const { convert, unit } = useDisplayTemperature();

    const [, setDevices] = useState<UserDevice[]>([]);
    const [, setLD] = useState(true);

    const [weather, setWeather] = useState<{ temp: number; humidity: number } | null>(null);
    const [loadingWX, setLW] = useState(true);

    const [plants, setPlants] = useState<PlantStatus[]>([]);
    const [loadingPlants, setLP] = useState(true);

    const [latest, setLatest] = useState<Record<string, SensorHistoryWithDeviceDto>>({});
    const [loadingLive, setLoadingLive] = useState(true);

    const [dashboardErrors, setDashboardErrors] = useState<string[]>([]);
    const addDashboardError = (msg: string) => {
        setDashboardErrors(prev => [...prev, msg]);
    };

    // Show one toast for dashboard errors
    useEffect(() => {
        if (dashboardErrors.length === 0) return;
        const id = setTimeout(() => {
            const readable = dashboardErrors
                .map(e => e.charAt(0).toUpperCase() + e.slice(1))
                .join(", ");
            toast.error(`Failed to load: ${readable}`);
            setDashboardErrors([]);
        }, 800);
        return () => clearTimeout(id);
    }, [dashboardErrors]);

    useEffect(() => {
        if (!jwt) return;
        setLD(true);
        userDeviceClient
            .getAllUserDevices(jwt)
            .then(list => setDevices(Array.isArray(list) ? list : []))
            .catch(() => addDashboardError("devices"))
            .finally(() => setLD(false));
    }, [jwt]);

    // Fetch latest snapshot of device readings
    useEffect(() => {
        if (!jwt) return;
        let cancelled = false;

        const loadLatest = async () => {
            setLoadingLive(true);
            try {
                const res = await greenhouseDeviceClient.getRecentSensorDataForAllUserDevice(jwt);
                const recs = res?.sensorHistoryWithDeviceRecords ?? [];
                if (cancelled) return;

                // Filter records with deviceId
                const validRecords = recs.filter(r => r.deviceId && r.time)

                validRecords.sort((a, b) => {
                    const aTime = a.time ? new Date(a.time).getTime() : 0;
                    const bTime = b.time ? new Date(b.time).getTime() : 0;
                    return bTime - aTime;
                });

                // Map deviceId -> latest record
                setLatest(Object.fromEntries(validRecords.map(r => [r.deviceId!, r])));

                if (!selectedDeviceId && validRecords.length) {
                    setDeviceId(validRecords[0].deviceId!);
                }

            } catch {
                addDashboardError("greenhouse data");
            } finally {
                if (!cancelled) setLoadingLive(false);
            }
        };

        loadLatest().then();
        const id = setInterval(loadLatest, 30_000);
        return () => { cancelled = true; clearInterval(id); };
    }, [jwt]);

    // Fetch Weather outside
    useEffect(() => {
        (async () => {
            try {
                setLW(true);
                const { latitude, longitude } = { latitude: 55.6761, longitude: 12.5683 };
                const temperatureUnit = unit === "°F" ? "fahrenheit" : "celsius";

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
                addDashboardError("weather");
            } finally {
                setLW(false);
            }
        })();
    }, [unit]);

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
                addDashboardError("plants");
            } finally {
                setLP(false);
            }
        })();
    }, [jwt]);

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
        humidity: live?.humidity ?? null,
        pressure: live?.airPressure ?? null,
        quality: live?.airQuality ?? null,
    }), [live]);

    const greet = greeting();

    return (
        <div className="min-h-[calc(100vh-64px)] flex flex-col font-display">
            <TitleTimeHeader title="Dashboard" />

            {/* greeting */}
            <h2 className="font-bold text-fluid-header px-6 pt-[clamp(0.75rem,1.5vw,1.5rem)] pb-[clamp(0.5rem,1vw,1rem)]">{`Good ${greet}!`}</h2>

            {/* stat cards */}
            <div className="grid gap-6 px-6 md:grid-cols-3">
                <StatCard title="Temperature" loading={loadingWX}
                          value={weather ? `${Math.round(weather.temp)}${unit}` : "–"} cls={""} />
                <StatCard title="Humidity" loading={loadingWX}
                          value={`${Math.round(weather?.humidity ?? 0)}%`} cls={""} />
                <StatCard title="Need Watering" loading={loadingPlants}
                          value={needsWater ? "Yes" : "No"} cls={needsWater ? "text-error" : "text-success"} />
            </div>

            {/* main row */}
            <main className="flex-1 flex flex-col lg:flex-row lg:items-stretch gap-fluid px-6 py-6">

            {/* circle card */}
                <div className="card flex-1 bg-[var(--color-surface)] shadow flex flex-col gap-fluid h-[clamp(12rem,30vw,33rem)]">
                    <h2 className="text-fluid-header text-center ">Your Device:</h2>
                    <p className="text-center text-lg font-semibold text-primary">
                        {selectedDeviceId && latest[selectedDeviceId]?.deviceName || ""}
                    </p>
                    <div className="card-body p-fluid">
                        {loadingLive ? (
                            <p className="text-center">Loading…</p>
                        ) : live ? (
                            <CircleStatGrid>
                                <CircleStat label="Temperature" unit={unit} colorToken="primary" value={convert(circleReadings.temperature)}/>
                                <CircleStat label="Humidity" unit="%" colorToken="success" value={circleReadings.humidity}/>
                                <CircleStat label="Pressure" unit="hPa" colorToken="info"    value={circleReadings.pressure}/>
                                <CircleStat label="Air Quality" unit="ppm" colorToken="warning" value={circleReadings.quality}/>
                            </CircleStatGrid>
                        ) : (
                            <p className="text-fluid text-center text-gray-500">
                                {Object.keys(latest).length ? "No data" : "No devices connected"}
                            </p>
                        )}
                    </div>
                </div>

                {/* plant carousel */}
                <PlantCarousel className="lg:flex-1 lg:basis-0 flex flex-col h-[clamp(13rem,30vw,33rem)]" plants={plants}/>
            </main>
        </div>
    );
}