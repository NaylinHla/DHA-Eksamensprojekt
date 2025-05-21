import {useWebSocketMessage} from "./index";
import {StringConstants} from "../atoms";
import toast from "react-hot-toast";
import {useLocation} from "react-router";
import {useEffect, useRef} from "react";

const TOAST_ID = "global-alert-toast";
const BATCH_WINDOW_MS = 1500;

export function useGlobalAlertToasts() {
    const location = useLocation();
    const alertQueue = useRef<any[]>([]);
    const timerRef = useRef<NodeJS.Timeout | null>(null);

    useWebSocketMessage(StringConstants.ServerBroadcastsLiveAlertToAlertView, (dto: any) => {
        const alerts = dto.alerts || [];
        if (!alerts.length) return;

        // Don't toast if already on the alerts page
        if (location.pathname.includes("/alerts")) return;

        // Add to the queue (without triggering render)
        alertQueue.current.push(...alerts);

        // Start or reset the timer
        if (timerRef.current) clearTimeout(timerRef.current);

        timerRef.current = setTimeout(() => {
            const batchSize = alertQueue.current.length;
            if (batchSize > 0) {
                toast.dismiss(TOAST_ID);
                toast.success(`${batchSize} new alert${batchSize > 1 ? "s" : ""} received`, {
                    id: TOAST_ID,
                    duration: 5000,
                });
                alertQueue.current = [];
            }
            timerRef.current = null;
        }, BATCH_WINDOW_MS);
    });

    useEffect(() => {
        return () => {
            if (timerRef.current) clearTimeout(timerRef.current);
        };
    }, []);
}
