import { useEffect, useState } from "react";
import { JwtAtom } from "../atoms/atoms.ts";
import { useAtom } from "jotai";
import toast from "react-hot-toast";
import { alertClient } from "../apiControllerClients";

// Local stricter Alert interface
export interface Alert {
    alertID: string;
    alertName: string;
    alertDesc: string;
    alertTime: string;
    alertPlant?: string;
}

export default function useAlertsRest(selectedYear: number | null) {
    const [alerts, setAlerts] = useState<Alert[]>([]);
    const [loading, setLoading] = useState(true);
    const [jwt] = useAtom(JwtAtom);
    
    useEffect(() => {
        if (!jwt) {
            setLoading(false);
            return;
        }

        setLoading(true);

        alertClient.getAlerts(jwt, selectedYear)
            .then((data) => {
                console.log("Raw data from server:", data);

                const mapped = data.map((a) => ({
                    alertID: a.alertID ?? "",
                    alertName: a.alertName ?? "",
                    alertDesc: a.alertDesc ?? "",
                    alertTime: a.alertTime?.toString() ?? "",
                    alertPlant: a.alertPlant
                }));

                console.log("Mapped alerts:", mapped);
                setAlerts(mapped);
            })
            .catch((err) => {
                console.error("Alert fetch failed:", err);
                toast.error("Failed to fetch alerts.");
            })
            .finally(() => setLoading(false));
    }, [jwt, selectedYear]);

    return { alerts, loading };
}
