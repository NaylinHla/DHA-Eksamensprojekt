import { useEffect, useState } from "react";
import { JwtAtom } from "../atoms/atoms.ts";
import { useAtom } from "jotai";

export interface Alert {
    alertID: string;
    alertName: string;
    alertDesc: string;
    alertTime: string;
    alertPlant?: string;
}

export default function useAlertsRest(selectedYear: number | null) {
    const [alerts, setAlerts] = useState<Alert[]>([]);
    const [jwt] = useAtom(JwtAtom);

    useEffect(() => {
        if (!jwt) return;

        let url = "/api/Alert/GetAlerts";
        if (selectedYear) {
            url += `?year=${selectedYear}`;
        }

        fetch(url, {
            headers: {
                Authorization: jwt
            }
        })
            .then((res) => res.json())
            .then(setAlerts)
            .catch(console.error);

    }, [jwt, selectedYear]);

    return { alerts };
}
