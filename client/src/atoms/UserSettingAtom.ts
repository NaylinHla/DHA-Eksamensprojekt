import { atom } from 'jotai';

export type UserSettings = {
    confirmDialog: boolean;
    fahrenheit: boolean;
    darkTheme: boolean;
    //TODO: add secretmode
};

export const UserSettingsAtom = atom<UserSettings | null>(null);
