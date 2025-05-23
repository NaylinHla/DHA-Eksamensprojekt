import { atomWithStorage, createJSONStorage } from 'jotai/utils';

export interface IUserSettings {
    celsius: boolean;
    darkTheme: boolean;
    confirmDialog: boolean;
    secretMode: boolean;
}

const sessionStorageWithJson = createJSONStorage<IUserSettings | null>(() => sessionStorage);

export const UserSettingsAtom = atomWithStorage<IUserSettings | null>(
    'userSettings',
    null,
    sessionStorageWithJson
);
