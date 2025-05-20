import {useEffect, useState} from "react";
import {JwtAtom, StringConstants} from "../atoms";
import {useAtom} from "jotai";
import toast from "react-hot-toast";
import {alertClient} from "../apiControllerClients";
import {useWebSocketMessage} from "./index";

export interface Alert {
    alertId: string;
    alertName: string;
    alertDesc: string;
    alertTime: string;
    alertPlantId?: string;
    alertUserDeviceId?: string;
}

export default function useAlertsRest() {
    const [alerts, setAlerts] = useState<Alert[]>([]);
    const [loading, setLoading] = useState(true);
    const [jwt] = useAtom(JwtAtom);

    useEffect(() => {
        if (!jwt) {
            setLoading(false);
            return;
        }

        setLoading(true);

        alertClient.getAlerts(jwt, null)
            .then((data) => {
                const mapped = data.map((a) => ({
                    alertId: a.alertId ?? "",
                    alertName: a.alertName ?? "",
                    alertDesc: a.alertDesc ?? "",
                    alertTime: a.alertTime?.toString() ?? "",
                    alertPlantId: a.alertPlantConditionId ?? "",
                    alertUserDeviceId: a.alertDeviceConditionId ?? "",
                }));
                setAlerts(mapped);
            })
            .catch((err) => {
                console.error("Alert fetch failed:", err);
                toast.error("Failed to fetch alerts.");
            })
            .finally(() => setLoading(false));
    }, [jwt]);

    useWebSocketMessage(StringConstants.ServerBroadcastsLiveAlertToAlertView, (dto: any) => {

        const alertMessages: Alert[] = dto.alerts || [];
        if (!alertMessages.length) return;

        setAlerts(prevAlerts => {
            // Optional: merge new alerts, avoiding duplicates by alertId
            const existingIds = new Set(prevAlerts.map(a => a.alertId));
            const newAlerts = alertMessages.filter(a => !existingIds.has(a.alertId));
            if (!newAlerts.length) return prevAlerts;

            return [...newAlerts, ...prevAlerts];
        });

    });



    return { alerts, loading };
}
