import {AdminChangesPreferencesDto} from "../generated-client.ts";
import {greenhouseDeviceClient, userDeviceClient} from "../apiControllerClients.ts";
import toast from "react-hot-toast";
import {useState} from "react";
import {useAtom} from "jotai";
import {JwtAtom} from "../atoms/atoms.ts";

export default function Settings() {
    
    const [jwt] = useAtom(JwtAtom);

    if (!jwt || jwt.length < 1) {
        return (<div className="flex flex-col items-center justify-center h-screen">please sign in to continue</div>)
    }
    
    return (<div className="flex flex-row items-center justify-around h-screen">

        {
            ["Device A: Weather station A: Moss", "Device B: Weather station B: Oslo"].map((device, index) => {

                //Here i just use some hardcoded values for the device details
                const [updateState, setUpdateState] = useState<AdminChangesPreferencesDto>({
                    interval: "Minute",
                    unit: "Celcius",
                    deviceId: device
                });

                return <div key={device} className="flex flex-col">
                    <p className="">{device}</p>
                    <img className="w-32"
                         src={"https://joy-it.net/files/files/Produkte/SBC-NodeMCU-ESP32/SBC-NodeMCU-ESP32-01.png"}/>
                    <button className="btn" onClick={() => {
                        userDeviceClient.adminChangesPreferences(updateState, localStorage.getItem('jwt')!).then(resp => {
                            toast('API sent preference change to edge devices')
                        }).catch(e => {
                            toast.error(JSON.parse(e))
                        })
                    }}>Change preferences for device
                    </button>
                    <label className="input">
                        <b>Interval:</b>
                        <input value={updateState.interval} placeholder="Interval" type="text" className="grow"
                               onChange={event => setUpdateState({...updateState, interval: event.target.value})}/>
                    </label>
                    <label className="input">
                        <b>Unit:</b>
                        <input value={updateState.unit} placeholder="Unit" type="text" className="grow"
                               onChange={event => setUpdateState({...updateState, unit: event.target.value})}/>
                    </label>
                </div>
            })
        };


    </div>);
}