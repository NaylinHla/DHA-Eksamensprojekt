import {AlertClient, AuthClient, GreenhouseDeviceClient, PlantClient, SubscriptionClient, UserDeviceClient} from "./generated-client.ts";

const baseUrl = import.meta.env.VITE_API_BASE_URL
const prod = import.meta.env.PROD

export const subscriptionClient = new SubscriptionClient(prod ? "https://" + baseUrl : "http://" + baseUrl);
export const greenhouseDeviceClient = new GreenhouseDeviceClient(prod ? "https://" + baseUrl : "http://" + baseUrl);
export const userDeviceClient = new UserDeviceClient(prod ? "https://" + baseUrl : "http://" + baseUrl);
export const authClient = new AuthClient(prod ? "https://" + baseUrl : "http://" + baseUrl);
export const alertClient = new AlertClient(prod ? "https://" + baseUrl : "http://" + baseUrl);
export const plantClient = new PlantClient(prod ? "https://" + baseUrl : "http://" + baseUrl);