import { useState } from "react";
import { useAtom } from "jotai";
import { JwtAtom } from "../atoms";

export interface CreateAlertDto {
    alertName: string;
    alertDesc: string;
    alertPlant?: string;
}

export interface AlertResponseDto {
    alertID: string;
    alertName: string;
    alertDesc: string;
    alertTime: string;
    alertPlant?: string;
}

export default function useCreateAlert() {
    const [jwt] = useAtom(JwtAtom);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);

    const createAlert = async (dto: CreateAlertDto): Promise<AlertResponseDto | null> => {
        setLoading(true);
        setError(null);

        try {
            const response = await fetch("/CreateAlert", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                    Authorization: jwt,
                },
                body: JSON.stringify(dto),
            });

            if (!response.ok) {
                throw new Error("Failed to create alert");
            }

            return await response.json();
        } catch (err: any) {
            setError(err.message || "Unknown error");
            return null;
        } finally {
            setLoading(false);
        }
    };

    return { createAlert, loading, error };
}
