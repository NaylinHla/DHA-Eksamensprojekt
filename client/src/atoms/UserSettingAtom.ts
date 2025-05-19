import { atomWithStorage, createJSONStorage } from 'jotai/utils';

export interface UserSettings {
    celsius: boolean;
    darkTheme: boolean;
    confirmDialog: boolean;
    secretMode: boolean;
}

const sessionStorageWithJson = createJSONStorage<UserSettings | null>(() => sessionStorage);

export const UserSettingsAtom = atomWithStorage<UserSettings | null>(
    'userSettings',
    null,
    sessionStorageWithJson
);
