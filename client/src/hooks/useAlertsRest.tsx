import { useEffect, useState } from "react";
import { JwtAtom } from "../atoms/atoms.ts";
import { useAtom } from "jotai";
import toast from "react-hot-toast";
import { alertClient } from "../apiControllerClients";

export interface Alert {
    alertId: string;
    alertName: string;
    alertDesc: string;
    alertTime: string;
    alertPlant?: string;
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
                    alertPlant: a.alertPlant
                }));
                setAlerts(mapped);
            })
            .catch((err) => {
                console.error("Alert fetch failed:", err);
                toast.error("Failed to fetch alerts.");
            })
            .finally(() => setLoading(false));
    }, [jwt]);

    return { alerts, loading };
}
