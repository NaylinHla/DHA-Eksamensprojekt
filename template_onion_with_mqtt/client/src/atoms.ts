import {atom} from 'jotai';
import {Devicelog} from "./generated-client.ts";

export const JwtAtom = atom<string>(localStorage.getItem('jwt') || '')

export const DeviceLogsAtom = atom<Devicelog[]>([]);