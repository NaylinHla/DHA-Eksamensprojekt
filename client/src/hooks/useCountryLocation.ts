import { useEffect, useState } from "react";
import { useAtomValue } from "jotai";
import { JwtAtom } from "../atoms";
import { parseJwt } from "../components/utils/parseJwt.ts";
import { DEFAULT_CITY } from "../pages/Weather/WeatherView.tsx"
import { CityHit } from "../types/WeatherTypes";

export function useCountryLocation(): CityHit {
    const jwt = useAtomValue<string>(JwtAtom);
    const [city, setCity] = useState<CityHit>(DEFAULT_CITY);

    useEffect(() => {
        const payload = parseJwt(jwt);
        if (!payload) return;
        console.log("JWT payload in useCountryLocation →", payload);
        const country = payload.country ?? payload.Country;
        if (!country) return;
        
        const abort   = new AbortController();

        (async () => {
            try {
                const res = await fetch(
                    `https://geocoding-api.open-meteo.com/v1/search` +
                    `?name=${encodeURIComponent(country)}` +
                    `&count=5&language=en`,
                    { signal: abort.signal }
                );
                const json = await res.json();
                if (!json.results?.length) return;

                const hit = json.results.find((r: any) => r.feature_code === "PCLI")
                    ?? json.results[0];

                setCity({
                    name: hit.name,
                    country: hit.name,
                    latitude: hit.latitude,
                    longitude: hit.longitude,
                });
            } catch {/* ignore network errors */}
        })();

        return () => abort.abort();
    }, [jwt]);

    return city;
}
