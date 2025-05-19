import { useEffect } from 'react';
import { useSetAtom, useAtom } from 'jotai';
import { UserSettingsAtom, JwtAtom } from '../atoms';
import { userSettingsClient } from './../apiControllerClients';
import toast from 'react-hot-toast';

export default function UserSettingsInitializer() {
    const setUserSettings = useSetAtom(UserSettingsAtom);
    const [jwt] = useAtom(JwtAtom);

    useEffect(() => {
        if (!jwt || jwt.trim() === '') return;

        const fetchUserSettings = async () => {
            try {
                const data = await userSettingsClient.getAllSettings(jwt);
                setUserSettings({
                    celsius: data.celsius ?? false,
                    darkTheme: data.darkTheme ?? false,
                    confirmDialog: data.confirmDialog ?? false,
                    secretMode: data.secretMode ?? false,
                });
            } catch (e) {
                toast.error('Failed to fetch user settings');
                console.error(e);
            }
        };

        fetchUserSettings();
    }, [jwt]);

    return null;
}
