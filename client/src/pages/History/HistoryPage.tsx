import {useEffect, useMemo, useRef, useState} from "react";
import {CartesianGrid, Legend, Line, LineChart, ResponsiveContainer, Tooltip, XAxis, YAxis} from "recharts";
import toast from "react-hot-toast";
import {useAtom} from "jotai";
import {
    AdminHasDeletedData,
    ConfirmModal,
    GetAllSensorHistoryByDeviceIdDto,
    GreenhouseSensorDataAtom,
    JwtAtom,
    SelectedDeviceIdAtom,
    SensorHistoryDto,
    StringConstants,
    TrashBinIcon,
    UserDevice,
    useTopicManager,
    useWebSocketMessage,
} from "../import";
import {greenhouseDeviceClient} from "../../apiControllerClients.ts";

export default function DeviceHistory() {
    const [greenhouseSensorDataAtom, setGreenhouseSensorDataAtom] = useAtom(GreenhouseSensorDataAtom);
    const [jwt] = useAtom(JwtAtom);
    const [isModalOpen, setModalOpen] = useState(false);
    const [devices, setDevices] = useState<UserDevice[]>([]);
    const [selectedDeviceId, setSelectedDeviceId] = useAtom(SelectedDeviceIdAtom);
    const prevId = useRef<string | null>(null);
    const {subscribe, unsubscribe} = useTopicManager();
    // WebSocket topic subscribe/unsubscribe

    useEffect(() => {

        if (!selectedDeviceId) return;

        // Correctly interpolate the selectedDeviceId in the topic string
        const topic = `GreenhouseSensorData/${selectedDeviceId}`;
        // Unsubscribe from the previous device if it's different from the new one
        if (prevId.current && prevId.current !== selectedDeviceId) {
            unsubscribe(`GreenhouseSensorData/${prevId.current}`).then();
        }
        subscribe(topic).then();
        prevId.current = selectedDeviceId;

        // Cleanup on unmount
        return () => {
            unsubscribe(topic).then();
        };
    }, [selectedDeviceId, subscribe, unsubscribe]);

    // Fetch devices when the component mounts
    useEffect(() => {

        console.log("JWT ER: " + jwt)

        if (!jwt) return;

        greenhouseDeviceClient.getAllUserDevices(jwt).then((res: any) => {
            const list = res.allUserDevice || [];
            setDevices(list);
            if (!selectedDeviceId && list.length) {
                setSelectedDeviceId(list[0].deviceId!); // If no device is selected, pick the first one
            }
        }).catch(() => {
            toast.error("Failed to load devices");
        });
    }, [jwt, selectedDeviceId, setSelectedDeviceId]);


    // Fetch data for selected device once the device is selected
    useEffect(() => {
        if (!jwt || !selectedDeviceId) return; // Ensure both JWT and deviceId are available

        greenhouseDeviceClient
            .getSensorDataByDeviceId(selectedDeviceId, jwt)
            .then(response => {
                setGreenhouseSensorDataAtom(response);  // Assuming you want to set it to an atom
            })
            .catch(error => {
                console.error("Error fetching data:", error);
            });
    }, [jwt, selectedDeviceId, setGreenhouseSensorDataAtom]); // Re-run when either jwt or selectedDeviceId changes


// Inside the useWebSocketMessage hook
    useWebSocketMessage(StringConstants.ServerBroadcastsLiveDataToDashboard, (dto: any) => {
        console.log("Received WebSocket message:", dto); //Debug Stuff
        const newLogs: SensorHistoryDto[] = dto.logs?.[0]?.sensorHistoryRecords || [];

        if (newLogs.length > 0) {
            // Filter out duplicate logs
            const uniqueNewLogs = newLogs.filter((newLog: SensorHistoryDto) => {
                const logTime = newLog.time ? new Date(newLog.time) : null;
                if (!logTime || isNaN(logTime.getTime())) {
                    console.error("Invalid time value", newLog.time);
                    return false;
                }
                return !greenhouseSensorDataAtom.some((deviceData: GetAllSensorHistoryByDeviceIdDto) =>
                    deviceData.sensorHistoryRecords?.some((existingLog) => {
                        const existingLogTime = existingLog.time ? new Date(existingLog.time) : null;
                        return existingLogTime && existingLogTime.getTime() === logTime.getTime();
                    })
                );
            });

            if (uniqueNewLogs.length > 0) {
                // Only update the relevant device data
                const updatedData = greenhouseSensorDataAtom.map((deviceData: GetAllSensorHistoryByDeviceIdDto) => {
                    if (deviceData.deviceId === selectedDeviceId) {
                        return {
                            ...deviceData,
                            sensorHistoryRecords: [...deviceData.sensorHistoryRecords || [], ...uniqueNewLogs],
                        };
                    }
                    return deviceData;
                });

                // Only update the atom if data has changed
                if (updatedData !== greenhouseSensorDataAtom) {
                    setGreenhouseSensorDataAtom(updatedData);
                }

                toast.success(`📡 New data for device ${selectedDeviceId}`);
            }
        }
    });

    // Deleted data broadcast
    useWebSocketMessage(StringConstants.AdminHasDeletedData, (_: AdminHasDeletedData) => {
        toast("Someone has deleted everything.");
        setGreenhouseSensorDataAtom([]);
    });

    // Optimized chart data per selected device
    const chartDataByKey = useMemo(() => {
        const deviceData = greenhouseSensorDataAtom.find(r => r.deviceId === selectedDeviceId);
        const records = deviceData?.sensorHistoryRecords || [];

        const format = (key: keyof SensorHistoryDto) =>
            records.map(e => ({
                time: e.time ? new Date(e.time).toLocaleString() : "",
                value: e[key] ?? NaN,
            }));


        return {
            temperature: format("temperature"),
            humidity: format("humidity"),
            airPressure: format("airPressure"),
            airQuality: format("airQuality"),
        };
    }, [greenhouseSensorDataAtom, selectedDeviceId]);

    const renderChart = (data: any[], label: string) => (
        <div className="mb-10 px-2">
            <h2 className="text-xl font-semibold mb-2">{label}</h2>
            <ResponsiveContainer width="100%" height={400}>
                <LineChart data={data}>
                    <CartesianGrid strokeDasharray="3 3"/>
                    <XAxis dataKey="time"/>
                    <YAxis/>
                    <Tooltip/>
                    <Legend/>
                    <Line
                        type="monotone"
                        dataKey="value"
                        name={label}
                        stroke="var(--color-primary)"
                        dot={false}
                        isAnimationActive={false}
                        animationDuration={500}
                    />
                </LineChart>
            </ResponsiveContainer>
        </div>
    );

    return (
        <div>


            <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4 p-4">
                <h1 className="text-2xl font-bold text-[var(--color-textprimary)]">Overview:</h1>
                
                <button
                    onClick={() => setModalOpen(true)}
                    className="btn btn-secondary btn-xl flex items-center justify-center gap-2"
                >
                    <span>Delete All Data For All User</span>
                    <TrashBinIcon size={20}/>
                </button>

                <div className="flex flex-col sm:flex-row sm:items-center gap-2">
                    <label className="font-medium">Select Device:</label>
                    <select
                        className="border rounded p-2"
                        value={selectedDeviceId || ""}
                        onChange={e => setSelectedDeviceId(e.target.value)}
                    >
                        {/* Check if there are devices */}
                        {devices.length === 0 ? (
                            <option value="" disabled>No device</option> // Disabled option when no devices are available
                        ) : (
                            devices.map(d => (
                                <option key={d.deviceId} value={d.deviceId}>
                                    {d.deviceName}
                                </option>
                            ))
                        )}
                    </select>
                </div>

            </div>

            {renderChart(chartDataByKey.temperature || [], "Temperature")}
            {renderChart(chartDataByKey.humidity || [], "Humidity")}
            {renderChart(chartDataByKey.airPressure || [], "Air Pressure")}
            {renderChart(chartDataByKey.airQuality || [], "Air Quality")}

            <ConfirmModal
                isOpen={isModalOpen}
                title="Confirm Deletion"
                subtitle="Are you sure you want to delete all data?"
                onConfirm={() => {
                    greenhouseDeviceClient
                        .deleteData(jwt!)
                        .then(() => toast.success("Deleted"))
                        .catch(() => toast.error("Failed"));
                    setModalOpen(false);
                }}
                onCancel={() => setModalOpen(false)}
            />
        </div>
    );
}
