import {atom, AlertResponseDto, GetAllSensorHistoryByDeviceIdDto} from "./import";

export const JwtAtom = atom<string>(localStorage.getItem('jwt') || '')

export const GreenhouseSensorDataAtom = atom<GetAllSensorHistoryByDeviceIdDto []>([]);
export const AlertsAtom = atom<AlertResponseDto[]>([]);