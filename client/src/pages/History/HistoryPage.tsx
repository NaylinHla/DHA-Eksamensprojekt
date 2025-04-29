import { useWsClient } from "ws-request-hook";
import {useEffect, useMemo, useState} from "react";
import {
    CartesianGrid,
    Legend,
    Tooltip,
    XAxis,
    YAxis,
    ResponsiveContainer,
    Line,
    LineChart
} from "recharts";
import { AdminHasDeletedData, ServerBroadcastsLiveDataToDashboard, StringConstants } from "../../generated-client.ts";
import toast from "react-hot-toast";
import { useAtom } from "jotai";
import {ConfirmModal, GreenhouseSensorDataAtom, JwtAtom, TrashBinIcon} from "../import";
import { greenhouseDeviceClient } from "../../apiControllerClients.ts";
import useInitializeData from "../../hooks/useInitializeData";

// Function to handle websocket message
const useWebSocketMessage = (messageKey: string, callback: (dto: any) => void) => {
    const { onMessage, readyState } = useWsClient();
    const [jwt] = useAtom(JwtAtom);

    useEffect(() => {
        if (readyState !== 1 || !jwt) return;

        const reactToMessageSetup = onMessage<any>(messageKey, callback);

        return () => reactToMessageSetup();
    }, [readyState, jwt, messageKey, callback]);
};

// Format the data for charts
const formatDataForChart = (logs: any[], key: string) => {
    return logs?.map((record: any) => ({
        time: record.time ? new Date(record.time).toLocaleString() : '',
        value: record[key],
    })) || [];
};

export default function AdminDashboard() {
    const [greenhouseSensorDataAtom, setGreenhouseSensorDataAtom] = useAtom(GreenhouseSensorDataAtom);
    const [jwt] = useAtom(JwtAtom);
    const [isModalOpen, setIsModalOpen] = useState(false);
    useInitializeData();

    // Use WebSocket to handle live data updates
    useWebSocketMessage(
        StringConstants.ServerBroadcastsLiveDataToDashboard,
        (dto: ServerBroadcastsLiveDataToDashboard) => {
            toast("New data from IoT device!");

            // Ensure you're accessing the correct field from the payload
            if (dto.logs && dto.logs.length > 0) {
                const sensorHistoryRecords = dto.logs[0]?.sensorHistoryRecords || [];
                setGreenhouseSensorDataAtom(sensorHistoryRecords);
            }
        }
    );

    // Use WebSocket to handle deleted data updates
    useWebSocketMessage(StringConstants.AdminHasDeletedData, (dto: AdminHasDeletedData) => {
        toast("Someone has deleted everything.");
        setGreenhouseSensorDataAtom([]);
    });

    const handleDeleteData = () => {
        setIsModalOpen(true);
    };

    const handleConfirmDelete = () => {
        //Call the API to delete all data
        greenhouseDeviceClient.deleteData(jwt!)
            .then(() => {
                toast.success("Successfully deleted all data.");
            })
            .catch(() => {
                toast.error("Failed to delete data.");
            });
        setIsModalOpen(false);

        toast.success("Successfully deleted all data.");
    };

    const handleCancelDelete = () => {
        setIsModalOpen(false);
    };

    // Memoize the formatted data to optimize re-renders
    const formattedTemperatureData = useMemo(() => formatDataForChart(greenhouseSensorDataAtom, "temperature"), [greenhouseSensorDataAtom]);
    const formattedHumidityData = useMemo(() => formatDataForChart(greenhouseSensorDataAtom, "humidity"), [greenhouseSensorDataAtom]);
    const formattedAirPressureData = useMemo(() => formatDataForChart(greenhouseSensorDataAtom, "airPressure"), [greenhouseSensorDataAtom]);
    const formattedAirQualityData = useMemo(() => formatDataForChart(greenhouseSensorDataAtom, "airQuality"), [greenhouseSensorDataAtom]);

    return (
        <>
            <h1 className="text-2xl font-bold mb-4 p-10">Data logs from greenhouse station devices</h1>

            <div className="flex items-center space-x-4"> {/* Added flex and spacing for alignment */}
                <button
                    onClick={handleDeleteData}
                    className="btn btn-secondary btn-xl m-10 flex items-center space-x-2"
                >
                    <span>Click here to delete data</span>
                    <TrashBinIcon color="currentColor" size={20}/>
                </button>
            </div>

            {/* Temperature Line Chart */}
            <ResponsiveContainer width="100%" height={400}>
                <LineChart data={formattedTemperatureData} margin={{top: 5, right: 30, left: 20, bottom: 5}}>
                    <CartesianGrid strokeDasharray="3 3"/>
                    <XAxis dataKey="time"/>
                    <YAxis/>
                    <Tooltip/>
                    <Legend/>
                    <Line
                        type="monotone"
                        dataKey="value"
                        name="Temperature"
                        stroke="var(--color-primary)"
                        dot={false}  // Initially hide dots
                        isAnimationActive={true}  // Enable smooth animation
                        animationDuration={500}  // Set animation duration
                    />
                </LineChart>
            </ResponsiveContainer>

            {/* Humidity Line Chart */}
            <ResponsiveContainer width="100%" height={400}>
                <LineChart data={formattedHumidityData} margin={{top: 5, right: 30, left: 20, bottom: 5}}>
                    <CartesianGrid strokeDasharray="3 3"/>
                    <XAxis dataKey="time"/>
                    <YAxis/>
                    <Tooltip/>
                    <Legend/>
                    <Line
                        type="monotone"
                        dataKey="value"
                        name="Humidity"
                        stroke="var(--color-primary)"
                        dot={false}  // Initially hide dots
                        isAnimationActive={true}  // Enable smooth animation
                        animationDuration={500}  // Set animation duration
                    />
                </LineChart>
            </ResponsiveContainer>

            {/* Air Pressure Line Chart */}
            <ResponsiveContainer width="100%" height={400}>
                <LineChart data={formattedAirPressureData} margin={{top: 5, right: 30, left: 20, bottom: 5}}>
                    <CartesianGrid strokeDasharray="3 3"/>
                    <XAxis dataKey="time"/>
                    <YAxis/>
                    <Tooltip/>
                    <Legend/>
                    <Line
                        type="monotone"
                        dataKey="value"
                        name="Air Pressure"
                        stroke="var(--color-primary)"
                        dot={false}  // Initially hide dots
                        isAnimationActive={true}  // Enable smooth animation
                        animationDuration={500}  // Set animation duration
                    />
                </LineChart>
            </ResponsiveContainer>

            {/* Air Quality Line Chart */}
            <ResponsiveContainer width="100%" height={400}>
                <LineChart data={formattedAirQualityData} margin={{top: 5, right: 30, left: 20, bottom: 5}}>
                    <CartesianGrid strokeDasharray="3 3"/>
                    <XAxis dataKey="time"/>
                    <YAxis/>
                    <Tooltip/>
                    <Legend/>
                    <Line
                        type="monotone"
                        dataKey="value"
                        name="Air Quality"
                        stroke="var(--color-primary)"
                        dot={false}  // Initially hide dots
                        isAnimationActive={true}  // Enable smooth animation
                        animationDuration={500}  // Set animation duration
                    />
                </LineChart>
            </ResponsiveContainer>

            {/* Modal Component */}
            <ConfirmModal
                isOpen={isModalOpen}
                title="Confirm Deletion"
                subtitle="Are you sure you want to delete all data? This action cannot be undone."
                onConfirm={handleConfirmDelete}
                onCancel={handleCancelDelete}
            />
        </>

    );
}
