import {useEffect} from "react";
import {weatherStationClient} from "../apiControllerClients.ts";
import {useAtom} from "jotai";
import {DeviceLogsAtom, JwtAtom} from "../atoms.ts";

export default function useInitializeData() {

    const [jwt] = useAtom(JwtAtom);
    const [, setDeviceLogs] = useAtom(DeviceLogsAtom)

    useEffect(() => {
        if (jwt == null || jwt.length < 1)
            return;
        weatherStationClient.getLogs(jwt).then(r => {
            setDeviceLogs(r || []);
        })
    }, [jwt])

}