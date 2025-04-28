import {useEffect} from "react";
import {greenhouseDeviceClient} from "../apiControllerClients.ts";
import {useAtom} from "jotai";
import {GreenhouseSensorDataAtom, JwtAtom} from "../atoms";

export default function useInitializeData() {

    const [jwt] = useAtom(JwtAtom);
    const [, setGreenhouseSensorDataAtom] = useAtom(GreenhouseSensorDataAtom)

    useEffect(() => {
        if (jwt == null || jwt.length < 1)
            return;
        greenhouseDeviceClient.getSensorDataByDeviceId("cd3b92b4-c156-4a60-b557-d46f64e5b5a4", jwt).then(r => {
            setGreenhouseSensorDataAtom        })
    }, [jwt])

}