export type WXResp = {
    current_weather: {
        temperature: number;
        windspeed: number;
        weathercode: number;
    };
    hourly: {
        time: string[];
        temperature_2m: number[];
        precipitation_probability: number[];
        windspeed_10m: number[];
        weathercode: number[];
    };
    daily: {
        time: string[];
        temperature_2m_max: number[];
        temperature_2m_min: number[];
        weathercode: number[];
    };
};

export type CityHit = { 
    name: string; 
    latitude: number; 
    longitude: number; 
    country: string 
};