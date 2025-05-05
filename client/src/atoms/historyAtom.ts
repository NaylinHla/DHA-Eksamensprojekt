import {atom, GetAllSensorHistoryByDeviceIdDto} from "./import";

export const GreenhouseSensorDataAtom = atom<GetAllSensorHistoryByDeviceIdDto []>([]);

export const SelectedDeviceIdAtom = atom<string | undefined>(undefined);