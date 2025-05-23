import {useEffect, useMemo, useState} from 'react';
import {
    ConditionAlertPlantResponseDto,
    ConditionAlertUserDeviceResponseDto,
    UserDeviceResponseDto,
} from '../generated-client';
import {alertConditionClient, plantClient, userDeviceClient} from '../apiControllerClients';
import {useAtom} from 'jotai';
import {JwtAtom} from '../atoms';
import toast from "react-hot-toast";

export interface EnrichedConditionAlertUserDevice extends ConditionAlertUserDeviceResponseDto {
    deviceName?: string;
}

// Discriminated union for filtered results
export type FilteredCondition =
    | (ConditionAlertPlantResponseDto & { plantName: string; __type: 'plant' })
    | (EnrichedConditionAlertUserDevice & { __type: 'device' });

export function useAlertConditions(
    view: 'plants' | 'devices',
    selectedDeviceId: string,
    selectedSensorType: string | null,
    searchTerm: string
) {
    const [plantConditions, setPlantConditions] = useState<(ConditionAlertPlantResponseDto & {
        plantName?: string
    })[]>([]);
    const [deviceConditions, setDeviceConditions] = useState<EnrichedConditionAlertUserDevice[]>([]);
    const [devices, setDevices] = useState<UserDeviceResponseDto[]>([]);
    const [loading, setLoading] = useState(true);
    const [jwt] = useAtom(JwtAtom);

    // Extract userId from JWT payload
    const userId = (() => {
        try {
            const payload = JSON.parse(atob(jwt?.split('.')[1] || '{}'));
            return payload.sub || payload.Id || '';
        } catch {
            return '';
        }
    })();

    // Fetch all user devices once on mount
    useEffect(() => {
        async function fetchDevices() {
            try {
                const data = await userDeviceClient.getAllUserDevices(jwt);
                setDevices(Array.isArray(data) ? data : []);
            } catch (err) {
                console.error('Failed to fetch devices:', err);
                setDevices([]);
                toast.error("Failed to load user devices", {id: "device-load-error"});
            }
        }

        fetchDevices().then();
    }, [jwt]);

    // Fetch conditions function – declared outside useEffect, so it can be returned
    async function fetchConditions() {
        if (!jwt || !userId) return;
        setLoading(true);
        try {
            if (view === 'plants') {
                const data = await alertConditionClient.getConditionAlertPlants(userId, jwt);
                const withNames = await Promise.all(
                    (data || []).map(async (item) => {
                        try {
                            const plantInfo = await plantClient.getPlant(item.plantId, jwt);
                            return {...item, plantName: plantInfo.plantName || 'Unnamed Plant'};
                        } catch {
                            return {...item, plantName: 'Unknown Plant'};
                        }
                    })
                );
                setPlantConditions(withNames);
            } else {
                const data = (await alertConditionClient.getConditionAlertUserDevices(userId, jwt)) || [];
                const enriched = data.map((c) => {
                    const match = devices.find((d) => d.deviceId === c.userDeviceId);
                    return {...c, deviceName: match?.deviceName || 'Unknown Device'};
                });
                setDeviceConditions(enriched);
            }
        } catch (err) {
            console.error('Failed to fetch alert conditions:', err);
            toast.error("Failed to fetch alert conditions", {id: "alert-data-load-error"});
        } finally {
            setLoading(false);
        }
    }

    // Fetch conditions on deps change
    useEffect(() => {
        fetchConditions().then();
    }, [jwt, userId, view, selectedDeviceId, devices]);

    // Filtered results based on view, selectedDeviceId, selectedSensorType, and conditions
    const filteredConditions: FilteredCondition[] = useMemo(() => {
        const term = searchTerm.trim().toLowerCase();

        if (view === 'plants') {
            return plantConditions
                .map(item => ({
                    ...item,
                    plantName: item.plantName || 'Unnamed Plant',
                    __type: 'plant' as const,
                }))
                .filter(item => {
                    const text = `${item.plantName} ${item.waterNotify}`.toLowerCase();
                    return text.includes(term);
                });
        } else {
            return deviceConditions
                .filter(c => !selectedDeviceId || c.userDeviceId === selectedDeviceId)
                .filter(c => !selectedSensorType || c.sensorType === selectedSensorType)   // ← SENSOR filter
                .map(item => ({...item, __type: 'device' as const}))
                .filter(item => {
                    const text = `${item.deviceName} ${item.sensorType} ${item.condition}`.toLowerCase();
                    return text.includes(term);
                });
        }
    }, [
        view,
        selectedDeviceId,
        selectedSensorType,
        plantConditions,
        deviceConditions,
        searchTerm
    ]);

    return {
        devices,
        loading,
        fetchConditions,
        setPlantConditions,
        plantConditions,
        setDeviceConditions,
        deviceConditions,
        filteredConditions,
    };
}
