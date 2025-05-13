import { atom } from 'jotai';

export type UserSettings = {
    confirmDialog: boolean;
    fahrenheit: boolean;
    darkTheme: boolean;
};

export const UserSettingsAtom = atom<UserSettings | null>(null);
