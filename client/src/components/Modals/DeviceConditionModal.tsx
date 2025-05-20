import React, {useEffect, useRef, useState} from "react";
import {Check, X} from "lucide-react";
import {useAtom} from "jotai";
import {ConditionAlertPlantResponseDto, JwtAtom} from "../../atoms";
import {alertConditionClient, plantClient, userDeviceClient} from "../../apiControllerClients";
import {
    ConditionAlertPlantCreateDto,
    ConditionAlertUserDeviceCreateDto,
    PlantResponseDto,
    UserDeviceResponseDto,
} from "../../generated-client";
import toast from "react-hot-toast";
import useCloseOnEscapeOrBackdrop from "../Functional/UseCloseOnEscapeOrBackdrop";
import {useNavigate} from "react-router-dom";
import {MyDeviceOverviewInRoute, PlantRoute} from "../../routeConstants";

interface Props {
    isOpen: boolean;
    onClose: () => void;
    onCreated: () => void;
    view: "plants" | "devices";
    selectedDeviceId?: string;
    existingConditions: ConditionAlertPlantResponseDto[];
}

const DeviceConditionModal: React.FC<Props> = ({
                                                   isOpen,
                                                   onClose,
                                                   onCreated,
                                                   view,
                                                   selectedDeviceId = "",
                                                   existingConditions = [],
                                               }) => {
    const [jwt] = useAtom(JwtAtom);
    const backdrop = useRef<HTMLDivElement>(null);

    const navigate = useNavigate();

    const [saving, setSaving] = useState(false);
    const [errors, setErrors] = useState<{ [key: string]: string }>({});

    // Devices state
    const [devices, setDevices] = useState<UserDeviceResponseDto[]>([]);
    const [deviceId, setDeviceId] = useState("");
    const [sensorType, setSensorType] = useState("Temperature");
    const [operator, setOperator] = useState(">=");
    const [threshold, setThreshold] = useState<number | "">("");

    // Plants state
    const [plants, setPlants] = useState<PlantResponseDto[]>([]);
    const [selectedPlantId, setSelectedPlantId] = useState("");

    useEffect(() => {
        if (!isOpen) return;

        setErrors({});
        setSaving(false);

        if (view === "devices") {
            fetchDevices().then();
            setDeviceId(selectedDeviceId || "");
            setSensorType("Temperature");
            setOperator(">=");
            setThreshold("");
        } else {
            fetchPlants().then();
            setSelectedPlantId("");
        }
    }, [isOpen, view, selectedDeviceId]);

    async function fetchDevices() {
        try {
            const res = await userDeviceClient.getAllUserDevices(jwt);
            setDevices(Array.isArray(res) ? res : [res]);

        } catch {
            toast.error("Failed to load devices");
        }
    }

    async function fetchPlants() {
        try {
            const {sub, Id} = JSON.parse(atob(jwt.split(".")[1]));
            const uid = (sub || Id) ?? "";
            const allPlants = await plantClient.getAllPlants(uid, jwt);

            const existingIds = new Set(existingConditions.map(c => c.plantId));
            setPlants(allPlants.filter(p => !existingIds.has(p.plantId)));
        } catch {
            toast.error("Failed to load plant data");
        }
    }

    function validate() {
        const errs: { [key: string]: string } = {};

        if (view === "devices") {
            if (threshold === "" || isNaN(Number(threshold))) {
                errs.threshold = "Threshold must be a valid number.";
            } else {
                const val = Number(threshold);
                const limits: Record<string, [number, number?]> = {
                    Temperature: [-40, 130],
                    Humidity: [0, 100],
                    AirPressure: [0.000001], // > 0
                    AirQuality: [0, 2000],
                };

                const [min, max] = limits[sensorType] || [];

                if (min !== undefined && val < min) errs.threshold = `Value must be ≥ ${min}.`;
                else if (max !== undefined && val > max) errs.threshold = `Value must be ≤ ${max}.`;
                else if (!limits[sensorType]) errs.sensorType = "Invalid sensor type.";
            }

            if (operator !== ">=" && operator !== "<=") errs.operator = "Operator must be >= or <=.";
            if (!deviceId) errs.deviceId = "Please select a device.";
        } else {
            if (!selectedPlantId) errs.selectedPlantId = "Please select a plant.";
        }

        setErrors(errs);
        return Object.keys(errs).length === 0;
    }

    async function handleSave() {
        if (!validate()) return;

        setSaving(true);

        try {
            if (view === "devices") {
                const dto: ConditionAlertUserDeviceCreateDto = {
                    userDeviceId: deviceId,
                    sensorType,
                    condition: `${operator}${threshold}`,
                };
                await alertConditionClient.createConditionAlertUserDevice(dto, jwt);
                toast.success("Device condition created");
            } else {
                const dto: ConditionAlertPlantCreateDto = {plantId: selectedPlantId};
                await alertConditionClient.createConditionAlertPlant(dto, jwt);
                toast.success("Plant condition created");
            }

            onCreated();
            onClose();
        } catch (err: any) {
            toast.error(err?.message || "Error creating condition");
        } finally {
            setSaving(false);
        }
    }

    useCloseOnEscapeOrBackdrop(isOpen, onClose, backdrop);

    if (!isOpen) return null;

    const renderError = (field: string) => (
        <p className={`text-sm h-5 ml-2 block ${errors[field] ? "text-red-600 visible" : "invisible"}`}>
            {errors[field] || "\u00A0"}
        </p>
    );

    // Determine if we have items (devices or plants) to show the form or not
    const hasItems = view === "devices" ? devices.length > 0 : plants.length > 0;

    // Determine redirect route based on view
    const redirectRoute = view === "devices" ? MyDeviceOverviewInRoute : PlantRoute;

    return (
        <div
            ref={backdrop}
            className="fixed inset-0 bg-black/40 flex items-center justify-center z-50 px-4"
        >
            <div className="bg-[var(--color-cream)] rounded-2xl w-full max-w-md p-6 relative flex flex-col gap-4">
                <button className="absolute right-4 top-4 text-muted-foreground" onClick={onClose} type="button">
                    <X size={20}/>
                </button>

                <h2 className="font-semibold text-fluid">
                    Add {view === "plants" ? "Plant" : "Device"} Condition
                </h2>

                <div className="flex flex-col gap-4 overflow-y-auto max-h-[70vh] pr-1">
                    {view === "devices" ? (
                        hasItems ? (
                            <>
                                <label className="text-fluid flex flex-col gap-1">
                                    <span>Device:</span>
                                    <select
                                        className="border border-gray-300 rounded px-3 py-2 w-full text-[--color-primary] bg-[--color-background]"
                                        value={deviceId}
                                        onChange={e => setDeviceId(e.target.value)}
                                        disabled={saving}
                                    >
                                        <option value="">Select a device</option>
                                        {devices.map(d => (
                                            <option key={d.deviceId} value={d.deviceId}>
                                                {d.deviceName}
                                            </option>
                                        ))}
                                    </select>
                                    {renderError("deviceId")}
                                </label>

                                <label className="text-fluid flex flex-col gap-1">
                                    <span>Sensor Type:</span>
                                    <select
                                        className="border border-gray-300 rounded px-3 py-2 w-full text-[--color-primary] bg-[--color-background]"
                                        value={sensorType}
                                        onChange={e => setSensorType(e.target.value)}
                                        disabled={saving}
                                    >
                                        {["Temperature", "Humidity", "AirPressure", "AirQuality"].map(type => (
                                            <option key={type} value={type}>
                                                {type}
                                            </option>
                                        ))}
                                    </select>
                                    {renderError("sensorType")}
                                </label>

                                <div className="flex space-x-2">
                                    <select
                                        className="border border-gray-300 rounded px-3 py-2 flex-shrink-0 text-[--color-primary] bg-[--color-background]"
                                        value={operator}
                                        onChange={e => setOperator(e.target.value)}
                                        disabled={saving}
                                    >
                                        <option value=">=">{">="}</option>
                                        <option value="<=">{"<="}</option>
                                    </select>
                                    <input
                                        type="number"
                                        step="any"
                                        className="border border-gray-300 rounded px-3 py-2 flex-grow text-[--color-primary] bg-[--color-background]"
                                        placeholder="Threshold"
                                        value={threshold}
                                        onChange={e => setThreshold(e.target.value === "" ? "" : Number(e.target.value))}
                                        disabled={saving}
                                    />
                                </div>
                                <p
                                    className={`text-sm h-5 ml-2 block ${
                                        errors.operator || errors.threshold ? "text-red-600 visible" : "invisible"
                                    }`}
                                >
                                    {errors.operator || errors.threshold || "\u00A0"}
                                </p>
                            </>
                        ) : (
                            <p className="text-sm text-red-600 ml-1 mt-1">
                                No devices available, add one to make an alert condition
                            </p>
                        )
                    ) : hasItems ? (
                        <label className="text-fluid flex flex-col gap-1">
                            <span>Plant:</span>
                            <select
                                className="border border-gray-300 rounded px-3 py-2 w-full text-[--color-primary] bg-[--color-background]"
                                value={selectedPlantId}
                                onChange={e => setSelectedPlantId(e.target.value)}
                                disabled={saving}
                            >
                                <option value="">Select a plant</option>
                                {plants.map(p => (
                                    <option key={p.plantId} value={p.plantId}>
                                        {p.plantName}
                                    </option>
                                ))}
                            </select>
                            {renderError("selectedPlantId")}
                        </label>
                    ) : (
                        <p className="text-sm text-red-600 ml-1 mt-1">
                            No plants available, add one to make an alert condition
                        </p>
                    )}
                </div>

                <div className="flex justify-end pt-4">
                    {hasItems ? (
                        <button
                            className="btn btn-primary flex items-center gap-1"
                            onClick={handleSave}
                            disabled={saving}
                            type="button"
                        >
                            <Check size={14}/>
                            {saving ? "Saving…" : "Save"}
                        </button>
                    ) : (
                        <button
                            className="px-8 py-3 text-sm sm:text-base font-semibold btn btn-neutral bg-transparent btn-sm"
                            onClick={() => navigate(redirectRoute)}
                            type="button"
                        >
                            Click to add one?
                        </button>
                    )}
                </div>
            </div>
        </div>
    );
};

export default DeviceConditionModal;
