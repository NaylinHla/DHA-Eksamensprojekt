import React, {useEffect, useRef, useState} from "react";
import {Check, X} from "lucide-react";
import {JwtAtom, UserDeviceCreateDto, UserDeviceEditDto, UserDeviceResponseDto,} from "../../atoms";
import {useAtom} from "jotai";
import toast from "react-hot-toast";
import {userDeviceClient} from "../../apiControllerClients";
import {IntervalSelector} from "../index";
import useCloseOnEscapeOrBackdrop from "../Functional/UseCloseOnEscapeOrBackdrop";

const intervalMultipliers = {
    Second: 1,
    Minute: 60,
    Hour: 3600,
    Days: 86400,
    Week: 604800,
} as const;

type Unit = keyof typeof intervalMultipliers;


interface Props {
    open: boolean;
    device: { deviceId: string } | null;
    onClose: () => void;
    onSaved: () => void;
}

const toEditDto = (d: UserDeviceResponseDto): UserDeviceEditDto => ({
    deviceName: d.deviceName ?? "",
    deviceDescription: d.deviceDescription ?? "",
    waitTime: String(d.waitTime ?? "0"),
});

const emptyCreate: UserDeviceCreateDto = {
    deviceName: "",
    deviceDescription: "",
    waitTime: "600",
};

const UserDeviceModal: React.FC<Props> = ({open, device, onClose, onSaved}) => {
    const [jwt] = useAtom(JwtAtom);
    const [isEditing, setEditing] = useState(false);
    const [data, setData] = useState<UserDeviceEditDto>(emptyCreate);
    const [saving, setSaving] = useState(false);
    const [errors, setErrors] = useState<{ name: string; desc: string; wait: string }>({
        name: "",
        desc: "",
        wait: "",
    });
    const [waitUnit, setWaitUnit] = useState<Unit>("Second");
    const backdrop = useRef<HTMLDivElement>(null);
    const MAX_WAIT_SECONDS = 999999;

    // Reset & load on open/device change
    useEffect(() => {
        if (!open) {
            setEditing(false);
            setData(emptyCreate);
            setErrors({name: "", desc: "", wait: ""});
            return;
        }
        setEditing(device !== null);
        setErrors({name: "", desc: "", wait: ""});
        if (device) {
            userDeviceClient
                .getUserDevice(device.deviceId, jwt)
                .then((d) => {
                    setData(toEditDto(d));
                })
                .catch(() => toast.error("Failed to load device"));
        } else {
            setData(emptyCreate);
        }
    }, [open, device, jwt]);

    // Close on ESC or backdrop click
    useCloseOnEscapeOrBackdrop(open, onClose, backdrop);

    const upd = <K extends keyof UserDeviceEditDto>(k: K, v: UserDeviceEditDto[K]) =>
        setData((d) => ({ ...d, [k]: v }));

    const validate = (): boolean => {
        const errs: { name?: string; desc?: string; wait?: string } = {};

        if (!data.deviceName || data.deviceName.trim().length < 2 || data.deviceName.trim().length > 30) {
            errs.name = data.deviceName
                ? "Name must be between 2 and 30 characters."
                : "Device name is required";
        }

        if (data.deviceDescription && data.deviceDescription.length > 500) {
            errs.desc = "Description must be under 500 characters.";
        }

        const waitValue = parseInt(data.waitTime ?? "0", 10);
        const maxAllowed = Math.floor(MAX_WAIT_SECONDS / intervalMultipliers[waitUnit]);
        const minAllowed = 10;

        if (waitValue < minAllowed) {
            errs.wait = `Minimum ${minAllowed} ${waitUnit}${minAllowed !== 10 ? "s" : ""} allowed.`;
        } else if (waitValue > maxAllowed) {
            errs.wait = `Maximum ${maxAllowed} ${waitUnit}${maxAllowed !== 1 ? "s" : ""} allowed.`;
        }

        setErrors({
            name: errs.name || "",
            desc: errs.desc || "",
            wait: errs.wait || "",
        });

        return !errs.name && !errs.desc && !errs.wait;
    };


    const save = async () => {
        // Run validation first
        if (!validate()) return;

        try {
            setSaving(true);

            // Check if deviceName is missing
            if (!data.deviceName) {
                return;
            }

            // Prepare the data for either updating or creating a new device
            const deviceDto = {
                deviceName: data.deviceName,
                created: new Date(),
                deviceDescription: data.deviceDescription,
                waitTime: data.waitTime,
            };

            // If device exists, update it
            if (device) {
                await userDeviceClient.editUserDevice(device.deviceId, deviceDto, jwt);
                toast.success(`Device "${data.deviceName}" updated successfully!`);
            } else {
                // Otherwise, create a new device
                await userDeviceClient.createUserDevice(deviceDto, jwt);
                toast.success(`Device "${data.deviceName}" created successfully!`);
            }
            onSaved();
            onClose();
        } catch (err: any) {
            toast.error(err?.message ?? "Something went wrong");
        } finally {
            setSaving(false);
        }
    };

    if (!open) return null;
    return (
        <div
            ref={backdrop}
            className="fixed inset-0 bg-black/40 flex items-center justify-center z-50 p-3"
        >
            <div className="relative bg-[var(--color-cream)] w-full max-w-md p-6 rounded-2xl flex flex-col gap-4">
                <button className="absolute right-4 top-4" onClick={onClose}>
                    <X size={20}/>
                </button>
                <h2 className="text-xl font-semibold truncate">
                    {device ? (isEditing ? "Edit device" : "Device details") : "Add device"}
                </h2>

                <div className="flex flex-col gap-4 overflow-y-auto max-h-[60vh] pr-2">
                    {/* Name */}
                    <label className="flex flex-col gap-1">
                        <span>Name:</span>
                        <input
                            value={data.deviceName ?? ""}
                            onChange={(e) => upd("deviceName", e.target.value)}
                            className="rounded-xl px-4 py-1 bg-[var(--color-surface)] shadow-sm"
                        />
                        <span className="text-red-500 text-sm h-5 ml-2 block">{errors.name || "\u00A0"}</span>
                    </label>

                    {/* Description */}
                    <label className="flex flex-col gap-1">
                        <span>Description:</span>
                        <textarea
                            rows={6}
                            value={data.deviceDescription ?? ""}
                            onChange={(e) => upd("deviceDescription", e.target.value)}
                            className="rounded-xl px-4 py-2 bg-[var(--color-surface)] shadow-sm resize-y"
                        />
                        <span className="text-red-500 text-sm h-5 ml-2 block">{errors.desc || "\u00A0"}</span>
                    </label>

                    {/* Wait time */}
                    <label className="flex flex-col gap-1">
                        <span>Wait time:</span>
                        <IntervalSelector
                            totalSeconds={parseInt(data.waitTime ?? "0", 10)}
                            onChange={(secs) => upd("waitTime", String(secs))}
                            onUnitChange={(unit) => setWaitUnit(unit)}
                        />
                        <span className="text-red-500 text-sm h-5 ml-2 block">{errors.wait || "\u00A0"}</span>
                    </label>
                </div>
                <div className="flex justify-end pt-4">
                    <button
                        className="btn btn-primary"
                        onClick={save}
                        disabled={saving}
                    >
                        <Check size={14}/> {saving ? "Saving…" : "Save"}
                    </button>
                </div>
            </div>
        </div>
    );
};

export default UserDeviceModal;
