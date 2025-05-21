import {AlertsRoute, DashboardRoute, HistoryRoute, PlantRoute, WeatherRoute} from "../routeConstants";

export const links = [
    {to: DashboardRoute, label: "Dashboard"},
    {to: PlantRoute, label: "Plants"},
    {to: AlertsRoute, label: "Alerts"},
    {to: HistoryRoute, label: "History"},
    {to: WeatherRoute, label: "Weather"},
]