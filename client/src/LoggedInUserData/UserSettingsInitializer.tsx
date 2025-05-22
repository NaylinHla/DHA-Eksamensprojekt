import {useEffect} from 'react';
import {useAtom, useSetAtom} from 'jotai';
import {JwtAtom, UserSettingsAtom} from '../atoms';
import {userSettingsClient} from '../apiControllerClients';

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
                console.error(e);
            }
        };

        fetchUserSettings().then();
    }, [jwt]);

    return null;
}
