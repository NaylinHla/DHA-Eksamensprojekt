import React, { useEffect, useMemo, useState } from "react";
import { format } from "date-fns";
import { Search, ArrowLeftRight } from "lucide-react";
import toast from "react-hot-toast";
import {
    Chart as ChartJS, LineElement, LinearScale, PointElement, CategoryScale
} from "chart.js";
import { Line } from "react-chartjs-2";
import { iconFromCode, cToF } from "../../components/utils/weather/weather.ts";
import {formatDateTimeForUserTZ, TitleTimeHeader} from "../../components";
import {CityHit, WXResp} from "../../types/WeatherTypes.ts";
import {cssVar} from "../../components/utils/Theme/theme.ts";

ChartJS.register(LineElement, LinearScale, PointElement, CategoryScale);
const DEFAULT_CITY: CityHit = {
    name: "Copenhagen",
    country: "Denmark",
    latitude: 55.6761,
    longitude: 12.5683,
};

type Tab = "temp" | "rain" | "wind";

const WeatherView: React.FC = () => {
    const [city, setCity] = useState(DEFAULT_CITY);
    const [q, setQ]       = useState("");
    const [hits, setHits] = useState<CityHit[]>([]);
    const [unitC, setUnitC] = useState(true);
    const [tab,  setTab]  = useState<Tab>("temp");
    const [loading, setLoading] = useState(false);
    const [wx, setWx] = useState<WXResp | null>(null);

    // Live Search City
    useEffect(() => {
        if (q.length < 2) { setHits([]); return; }
        const id = setTimeout(async () => {
            try {
                const res = await fetch(
                    `https://geocoding-api.open-meteo.com/v1/search?name=${encodeURIComponent(q)}&count=5`
                );
                const json = await res.json();
                setHits(json.results ?? []);
            } catch { /* ignore  */ }
        }, 400);
        return () => clearTimeout(id);
    }, [q]);

    // Fetch Weather DAta
    useEffect(() => {
        (async () => {
            try {
                setLoading(true);
                const url =
                    `https://api.open-meteo.com/v1/forecast?latitude=${city.latitude}&longitude=${city.longitude}` +
                    `&current_weather=true&hourly=temperature_2m,precipitation_probability,windspeed_10m,weathercode` +
                    `&daily=temperature_2m_max,temperature_2m_min,weathercode&timezone=auto`;
                const res = await fetch(url);
                if (!res.ok) throw new Error("Weather error");
                setWx(await res.json());
            } catch (e:any) {
                toast.error(e.message ?? "Weather fetch failed");
            } finally { setLoading(false); }
        })();
    }, [city]);

    // Chart Data
    const chart = useMemo(() => {
        if (!wx) return null;
        const nowIdx = wx.hourly.time.findIndex(t => new Date(t) > new Date());
        const slice = (arr:number[]) => arr.slice(nowIdx, nowIdx+24);

        const labels = wx.hourly.time.slice(nowIdx, nowIdx+24).map(t =>
            format(new Date(t), "HH")
        );
        const values = slice(
            tab === "temp" ? wx.hourly.temperature_2m :
                tab === "rain" ? wx.hourly.precipitation_probability :
                    wx.hourly.windspeed_10m
        );

        return {
            labels,
            datasets: [{
                data: values,
                tension: 0.3,
                borderWidth: 2,
                pointRadius: 0,
                borderColor: cssVar("--color-primary"),
            }],
        };
    }, [wx, tab]);

    // Helpers
    const toUnit = (c:number) => unitC ? c : cToF(c);
    const unitLabel = unitC ? "°C" : "°F";

    return (
        <div className="min-h-[calc(100vh-64px)] flex flex-col font-display">

            {/* Header */}
            <TitleTimeHeader title="Weather"/>
            
            {/* Search bar */}
            <div className="px-6 pt-4">
                <div className="relative max-w-sm">
                    <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground"/>
                    <input
                        value={q}
                        onChange={e => setQ(e.target.value)}
                        placeholder="Search city / country"
                        className="pl-9 input input-sm w-full bg-[var(--color-surface)]"
                    />
                    {!!hits.length && (
                        <ul className="absolute z-10 bg-[var(--color-surface)] shadow rounded-lg mt-1 w-full max-h-44 overflow-auto">
                            {hits.map(h => (
                                <li
                                    key={h.name+ h.latitude}
                                    className="px-3 py-1 hover:bg-base-200 cursor-pointer text-sm"
                                    onClick={() => { setCity(h); setQ(""); setHits([]); }}
                                >
                                    {h.name}, {h.country}
                                </li>
                            ))}
                        </ul>
                    )}
                </div>
            </div>

            {/* Body */}
            <main className="flex-1 overflow-y-auto px-6 py-4 space-y-6">
                {/* Current Card */}
                {wx && (
                    <div className="rounded-2xl p-6 bg-[var(--color-surface)] shadow flex items-center gap-6">
                        <div className="text-6xl">{iconFromCode(wx.current_weather.weathercode)}</div>
                        <div>
                            <div className="text-5xl font-bold leading-none">
                                {Math.round(toUnit(wx.current_weather.temperature))}{unitLabel}
                            </div>
                            <div className="text-sm text-muted-foreground">
                                Wind: {wx.current_weather.windspeed} m/s
                            </div>
                        </div>
                        <button
                            className="btn btn-xs ml-auto"
                            onClick={() => setUnitC(!unitC)}
                        >
                            {unitC ? "°F" : "°C"}
                        </button>
                    </div>
                )}

                {/* Data Card */}
                {chart && (
                    <div className="bg-[var(--color-surface)] shadow rounded-2xl overflow-hidden">

                        {/* tab bar sits flush with the top edge */}
                        <div className="tabs tabs-bordered rounded-t-2xl">
                            {(["temp","rain","wind"] as Tab[]).map(t => (
                                <a
                                    key={t}
                                    className={`tab flex-1 tab-bordered${tab===t?" tab-active":""}`}
                                    onClick={()=>setTab(t)}
                                >
                                    {t==="temp" ? "Temperature" : t==="rain" ? "Rain %" : "Wind"}
                                </a>
                            ))}
                        </div>
                        
                        <hr className="border-primary" />
                        
                        <div className="p-4 rounded-t-none">
                            <h3 className="text-lg font-semibold mb-3">
                                {city.name}, {city.country}
                            </h3>

                            <div className="h-40 w-full">
                                <Line
                                    data={chart}
                                    options={{
                                        responsive: true,
                                        maintainAspectRatio: false,
                                        animation: false,
                                        devicePixelRatio: 1.5,
                                        elements: { point: { radius: 0 } },
                                        scales: {
                                            x: { grid: { display: false }, ticks: { autoSkip: true, maxTicksLimit: 12 } },
                                            y: { grid: { display: false }, ticks: { display: false } },
                                        },
                                        plugins: {
                                            legend: { display: false },
                                            tooltip: { enabled: true, intersect: false, mode: "index" },
                                        },
                                    }}
                                />
                            </div>
                        </div>
                    </div>
                )}

                {/* 7‑day */}
                {wx && (
                    <div className="grid gap-4 auto-rows-fr grid-cols-[repeat(auto-fill,minmax(5.5rem,1fr))]">
                        {wx.daily.time.map((d, i) => (
                            <div key={d}
                                 className="rounded-xl bg-[var(--color-surface)] p-3 shadow flex flex-col items-center gap-1">
                                <div className="text-sm">{format(new Date(d), "EEE")}</div>
                                <div className="text-2xl">{iconFromCode(wx.daily.weathercode[i])}</div>
                                <div className="text-sm">
                                    {Math.round(toUnit(wx.daily.temperature_2m_max[i]))}°/
                                    {Math.round(toUnit(wx.daily.temperature_2m_min[i]))}°
                                </div>
                            </div>
                        ))}
                    </div>
                )}

                {loading && <p className="text-center">Loading…</p>}
            </main>

            {/* Refresh chart */}
            <button
                className="btn btn-circle btn-primary fixed bottom-6 right-6 shadow-lg"
                onClick={()=>setCity({...city})}
                disabled={loading}
            >
                <ArrowLeftRight/>
            </button>
        </div>
    );
};

export default WeatherView;
