import { atom } from 'jotai';

export interface UserSettings {
    celsius: boolean;
    darkTheme: boolean;
    confirmDialog: boolean;
    secretMode: boolean;
}

export const UserSettingsAtom = atom<UserSettings | null>(null);