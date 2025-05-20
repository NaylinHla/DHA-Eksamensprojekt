import React, {useEffect, useState} from 'react';
import {Plus, Trash2} from 'lucide-react';
import {ConfirmModal, DeviceConditionModal, SearchBar, TitleTimeHeader} from '../../components';
import {useAtom} from 'jotai';
import {JwtAtom} from '../../atoms';
import {useAlertConditions} from '../../hooks/useAlertConditions';
import {alertConditionClient} from '../../apiControllerClients';
import {useLocation} from "react-router-dom";
import {useDisplayTemperature} from '../import';

export default function AlertConditionsPage() {
    const location = useLocation();
    const {autoSelectDevice} = location.state || {};
    const [view, setView] = useState<'plants' | 'devices'>('plants');
    const [selectedDeviceId, setSelectedDeviceId] = useState('');
    const [selectedSensorType, setSelectedSensorType] = useState<string | null>(null);
    const [deleteId, setDeleteId] = useState<string | null>(null);
    const [deleteLoading, setDeleteLoading] = useState(false);
    const [showCreateModal, setShowCreateModal] = useState(false);
    const [searchTerm, setSearchTerm] = useState('');
    const [jwt] = useAtom(JwtAtom);
    const [showSpinner, setShowSpinner] = useState(false);
    const {convert, unit} = useDisplayTemperature();
    const SENSOR_TYPES = ['Temperature', 'Humidity', 'AirPressure', 'AirQuality'];

    useEffect(() => {
        if (autoSelectDevice) {
            setView('devices');
        } else {
            setView('plants');
        }
    }, [autoSelectDevice]);

    const {
        devices,
        loading,
        fetchConditions,
        setPlantConditions,
        setDeviceConditions,
        filteredConditions,
    } = useAlertConditions(view, selectedDeviceId, selectedSensorType, searchTerm);

    useEffect(() => {
        let timer: NodeJS.Timeout;
        if (loading) {
            timer = setTimeout(() => setShowSpinner(true), 300);
        } else {
            setShowSpinner(false);
        }
        return () => clearTimeout(timer);
    }, [loading]);

    const Spinner = (
        <div className="flex justify-center items-center h-32">
            <svg className="animate-spin h-8 w-8 mr-3 text-gray-500" viewBox="0 0 24 24">
                <circle cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" fill="none"
                        className="opacity-25"/>
                <path fill="currentColor" d="M4 12a8 8 0 018-8v4a4 4 0 00-4 4H4z" className="opacity-75"/>
            </svg>
            <span className="text-gray-500">Loading…</span>
        </div>
    );

    const handleDelete = async () => {
        if (!deleteId) return;
        setDeleteLoading(true);
        try {
            if (view === 'plants') {
                await alertConditionClient.deleteConditionAlertPlant(deleteId, jwt);
                setPlantConditions(prev => prev.filter(c => c.conditionAlertPlantId !== deleteId));
            } else {
                await alertConditionClient.deleteConditionAlertUserDevice(deleteId, jwt);
                setDeviceConditions(prev => prev.filter(c => c.conditionAlertUserDeviceId !== deleteId));
            }
            setDeleteId(null);
        } catch (err) {
            console.error('Delete failed', err);
        } finally {
            setDeleteLoading(false);
        }
    };

    return (
        <div className="min-h-[calc(100vh-64px)] flex flex-col font-display">
            <TitleTimeHeader title="Alert Conditions"/>

            {/* Controls */}
            <div className="flex flex-col lg:flex-row items-center justify-between gap-4 px-4 md:px-6 py-4">
                <div className="flex flex-wrap sm:flex-nowrap items-center gap-4 w-full lg:w-auto">
                    {/* Search Bar */}
                    <div className="flex-grow min-w-0 sm:min-w-[200px] max-w-full">
                        <SearchBar searchTerm={searchTerm} onSearch={setSearchTerm}/>
                    </div>

                    {/* Checkbox and Add Button Wrapper */}
                    <div className="flex items-center justify-center gap-4 text-sm w-full sm:w-auto">
                        {/* Checkbox */}
                        <label className="inline-flex items-center gap-2 whitespace-nowrap">
                            <input
                                type="checkbox"
                                className="checkbox checkbox-xs"
                                checked={view === 'devices'}
                                onChange={e => setView(e.target.checked ? 'devices' : 'plants')}
                            />
                            View Device Condition
                        </label>

                        {/* Add Button */}
                        <button
                            className="flex-none flex items-center gap-2 px-4 py-2 bg-[--color-primary] text-[--color-primary-foreground] rounded whitespace-nowrap"
                            onClick={() => setShowCreateModal(true)}
                        >
                            <Plus className="w-4 h-4"/>
                            Add {view === 'devices' ? 'Device' : 'Plant'} Condition
                        </button>
                    </div>
                </div>

                {/* Device selector dropdown */}
                {view === 'devices' && (
                    <div className="flex-none w-full sm:w-auto lg:ml-auto mt-4 lg:mt-0">
                        <div className={`overflow-hidden transition-all duration-10000 ease-in-out${
                            devices.length > 0
                                ? 'max-h-40 opacity-100'
                                : 'max-h-12 opacity-100' /* keep some height for "No devices found" message */
                        }`}>
                            {devices.length > 0 ? (
                                <select
                                    className="border px-3 py-2 rounded w-full sm:w-auto md:max-w-[200px] truncate max-w-full bg-[--color-background]"
                                    value={selectedDeviceId}
                                    onChange={e => setSelectedDeviceId(e.target.value)}
                                >
                                    <option value="">All Devices</option>
                                    {devices.map(d => (
                                        <option key={d.deviceId} value={d.deviceId}>
                                            {d.deviceName}
                                        </option>
                                    ))}
                                </select>
                            ) : (
                                <div className="text-sm text-gray-500 italic py-2 px-2">No devices found</div>
                            )}
                        </div>
                    </div>
                )}
            </div>


            {/* Content */}
            <div className="relative flex-1 overflow-hidden px-4 sm:px-6 pb-6">
                {view === 'devices' && (
                    <div
                        className="w-full z-2 sm:absolute sm:left-0 sm:top-4 sm:bottom-4 pl-8 md:pl-12 sm:w-32 sm:pr-6 pb-4 overflow-x-auto sm:overflow-visible">
                        <aside
                            className="flex sm:flex-col flex-row sm:space-y-2 gap-2 sm:gap-0 p-2 sm:p-0 items-center p-fluid space-y-2 text-fluid">
                            {selectedSensorType && (
                                <button
                                    onClick={() => setSelectedSensorType(null)}
                                    className="text-center px-4 py-2 rounded-md text-gray-500 hover:text-white hover:bg-neutral transition font-semibold min-w-[8rem] whitespace-nowrap w-auto md:w-full"
                                    title="Clear sensor filter"
                                >
                                    Clear
                                </button>
                            )}
                            {SENSOR_TYPES.map(sensor => (
                                <button
                                    key={sensor}
                                    onClick={() => setSelectedSensorType(sensor)}
                                    className={`px-4 py-2 rounded-md whitespace-nowrap min-w-[8rem] text-center transition ${
                                        selectedSensorType === sensor
                                            ? "bg-primary text-white font-semibold"
                                            : "hover:bg-neutral hover:text-white text-gray-500"
                                    }`}
                                >
                                    {sensor}
                                </button>
                            ))}
                        </aside>
                    </div>
                )}

                <main
                    className={`transition-all duration-300 ease-in-out h-full overflow-y-auto relative ${
                        view === 'devices' && filteredConditions.length > 0 ? 'sm:ml-32' : ''
                    }`}
                >
                    {showSpinner ? (
                        <div className="flex justify-center items-center w-full h-28">{Spinner}</div>
                    ) : !loading && filteredConditions.length === 0 ? (
                        <div className="flex justify-center items-center h-48">
                            <p className="text-gray-400">
                                No {view === 'plants' ? 'plant' : 'device'} conditions found.
                            </p>
                        </div>
                    ) : (
                        <ul className="space-y-3">
                            {filteredConditions.map(cond =>
                                cond.__type === 'plant' ? (
                                    <li
                                        key={cond.conditionAlertPlantId}
                                        className="bg-[var(--color-surface)] p-4 shadow rounded-xl flex flex-col sm:flex-row justify-between items-start sm:items-center gap-2"
                                    >
                                        <div>
                                            <strong>{cond.plantName}</strong> Water
                                            Notify {cond.waterNotify ? 'On' : 'Off'}
                                        </div>
                                        <button onClick={() => setDeleteId(cond.conditionAlertPlantId ?? null)}>
                                            <Trash2/>
                                        </button>
                                    </li>
                                ) : (
                                    <li
                                        key={cond.conditionAlertUserDeviceId}
                                        className="bg-[var(--color-surface)] p-4 shadow rounded-xl flex flex-col sm:flex-row justify-between items-start sm:items-center gap-2"
                                    >
                                        <div>
                                            <strong>{cond.sensorType}</strong>{' '}
                                            {(() => {
                                                if (!cond.condition) return '';

                                                const conditionStr = cond.condition;

                                                // Only convert temperature conditions and parse operators like <=30
                                                if (cond.sensorType === 'Temperature') {
                                                    const match = conditionStr.match(/^([<>=!]*)(-?\d+(\.\d+)?)$/);
                                                    if (!match) return conditionStr;

                                                    const [, operator, numStr] = match;
                                                    const num = Number(numStr);
                                                    if (Number.isNaN(num)) return conditionStr;

                                                    const converted = convert(num);
                                                    return `${operator}${converted}${unit}`;
                                                }

                                                // For other sensor types, just append proper unit without conversion
                                                let unitStr: string;
                                                switch (cond.sensorType) {
                                                    case 'Humidity':
                                                        unitStr = '%';
                                                        break;
                                                    case 'AirPressure':
                                                        unitStr = 'hPa';
                                                        break;
                                                    case 'AirQuality':
                                                        unitStr = 'ppm';
                                                        break;
                                                    default:
                                                        unitStr = '';
                                                }

                                                return `${conditionStr}${unitStr}`;
                                            })()}
                                        </div>
                                        <div
                                            className={`flex items-center max-w-[150px] ${selectedDeviceId ? 'justify-center' : 'space-x-2 truncate'}`}>
                                            {/* Show device name only if all devices selected */}
                                            {!selectedDeviceId && (
                                                <span className="truncate">({cond.deviceName})</span>
                                            )}
                                            <button
                                                onClick={() => setDeleteId(cond.conditionAlertUserDeviceId ?? null)}
                                                className="flex items-center justify-center"
                                                aria-label="Delete condition"
                                            >
                                                <Trash2/>
                                            </button>
                                        </div>
                                    </li>

                                )
                            )}
                        </ul>
                    )}
                </main>
            </div>

            {/* Modals */}
            <ConfirmModal
                isOpen={!!deleteId}
                title="Confirm Delete"
                subtitle="Are you sure you want to delete this alert condition? This action cannot be undone."
                loading={deleteLoading}
                onConfirm={handleDelete}
                onCancel={() => setDeleteId(null)}
            />
            <DeviceConditionModal
                isOpen={showCreateModal}
                onClose={() => setShowCreateModal(false)}
                onCreated={fetchConditions}
                view={view}
                selectedDeviceId={selectedDeviceId}
                existingConditions={filteredConditions.filter(c => c.__type === 'plant').map(c => ({
                    plantId: c.plantId!,
                    conditionAlertPlantId: c.conditionAlertPlantId,
                    waterNotify: c.waterNotify
                }))}
            />
        </div>
    );
}
