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
    UserDevice, StatCard, CircleStatGrid, CircleStat, PlantCarousel,
} from "../import";
import {
    greenhouseDeviceClient,
    plantClient,
    userDeviceClient,
} from "../../apiControllerClients.ts";
import { cssVar } from "../../components/utils/Theme/theme.ts";
import {CardPlant} from "../../components/Plants/PlantCard.tsx";
ChartJS.register(CategoryScale, LinearScale, Legend, Tooltip);

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

    // States
    const [devices, setDevices] = useState<UserDevice[]>([]);
    const [loadingDev, setLD] = useState(true);

    const [weather,       setWeather] = useState<{ temp: number; humidity: number } | null>(null);
    const [loadingWX, setLW] = useState(true);

    const [plants, setPlants] = useState<PlantStatus[]>([]);
    const [loadingPlants, setLP] = useState(true);

    /* latest snapshot for circles */
    const [latest, setLatest]        = useState<Record<string, SensorHistoryWithDeviceDto>>({});
    const [loadingLive, setLoadingLive]   = useState(true);
    
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
                    const next = p.waterEvery != null ? Math.max(p.waterEvery - days, 0) : 0;
                    return {id: p.plantId!, name: p.plantName!, nextWaterInDays: next, isDead: p.isDead ?? false, needsWater: next === 0,
                } as PlantStatus;
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
            <h2 className="font-bold text-fluid-header px-6 pt-[clamp(0.75rem,1.5vw,1.5rem)] pb-[clamp(0.5rem,1vw,1rem)]">{`Good ${greet}!`}</h2>

            {/* stat cards */}
            <div className="grid auto-rows gap-fluid px-6 sm:grid-cols-[repeat(auto-fit,minmax(14rem,1fr))]">
                <StatCard title="Temperature"   loading={loadingWX} value={`${Math.round(weather?.temp ?? 0)}°C`} />
                <StatCard title="Humidity"      loading={loadingWX} value={`${Math.round(weather?.humidity ?? 0)}%`} />
                <StatCard title="Need Watering" loading={loadingPlants} value={needsWater ? "Yes" : "No"}
                          emphasisClass={needsWater ? "text-error" : "text-success"} />
            </div>
            
            {/* main row */}
            <main className="flex-1 flex flex-col lg:flex-row lg:items-stretch gap-fluid px-6 py-6">

            {/* circle card */}
                <div className="card flex-1 bg-[var(--color-surface)] shadow flex flex-col gap-fluid h-[clamp(12rem,20vw,20rem)]">
                    <div className="card-body p-fluid">
                        {loadingLive ? (
                            <p className="text-center">Loading…</p>
                        ) : live ? (
                            <CircleStatGrid>
                                <CircleStat label="Temperature" unit="°C" colorToken="primary" value={circleReadings.temperature}/>
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
                <PlantCarousel className="lg:flex-1 lg:basis-0 flex flex-col h-[clamp(10rem,20vw,20rem)]" plants={plants}/>
            </main>
        </div>
    );
}