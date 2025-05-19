import {
    AlertClient,
    AuthClient,
    GreenhouseDeviceClient,
    PlantClient,
    SubscriptionClient,
    UserDeviceClient,
    UserSettingsClient,
    UserClient,
    EmailClient, AlertConditionClient,
} from "./generated-client.ts";

const baseUrl = import.meta.env.VITE_API_BASE_URL;
const prod = import.meta.env.PROD;
const fullUrl = prod ? "https://" + baseUrl : "http://" + baseUrl;

export const subscriptionClient = new SubscriptionClient(fullUrl);
export const greenhouseDeviceClient = new GreenhouseDeviceClient(fullUrl);
export const userDeviceClient = new UserDeviceClient(fullUrl);
export const authClient = new AuthClient(fullUrl);
export const alertConditionClient = new AlertConditionClient(fullUrl);
export const alertClient = new AlertClient(fullUrl);
export const plantClient = new PlantClient(fullUrl);
export const userSettingsClient = new UserSettingsClient(fullUrl);
export const userClient = new UserClient(fullUrl);
export const emailClient = new EmailClient(fullUrl);