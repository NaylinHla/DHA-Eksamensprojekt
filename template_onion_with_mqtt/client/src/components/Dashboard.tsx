import {useWsClient} from "ws-request-hook";
import {useEffect} from "react";
import {Bar, BarChart, CartesianGrid, Legend, Rectangle, ResponsiveContainer, Tooltip, XAxis, YAxis} from "recharts";
import {AdminHasDeletedData, ServerBroadcastsLiveDataToDashboard, StringConstants,} from "../generated-client.ts";
import toast from "react-hot-toast";
import {useAtom} from "jotai";
import {DeviceLogsAtom, JwtAtom} from "../atoms.ts";
import {weatherStationClient} from "../apiControllerClients.ts";

const baseUrl = import.meta.env.VITE_API_BASE_URL
const prod = import.meta.env.PROD;


export default function AdminDashboard() {

    const {onMessage, readyState} = useWsClient()
    const [deviceLogs, setDeviceLogs] = useAtom(DeviceLogsAtom)
    const [jwt, setJwt] = useAtom(JwtAtom)

    if (!jwt || jwt.length < 1) {
        return (<div className="flex flex-col items-center justify-center h-screen">please sign in to continue</div>)
    }

    //Broadcast reaction hook
    useEffect(() => {
        if (readyState != 1 || jwt == null || jwt.length < 1)
            return;
        const reactToMessageSetup = onMessage<ServerBroadcastsLiveDataToDashboard>
        (StringConstants.ServerBroadcastsLiveDataToDashboard, (dto) => {
            console.log(dto)
            toast("New data from IoT device!")
            setDeviceLogs(dto.logs || []);
        })
        return () => reactToMessageSetup();
    }, [readyState, jwt]);

    useEffect(() => {
        if (readyState != 1 || jwt == null || jwt.length < 1)
            return;
        const reactToMessageSetup = onMessage<AdminHasDeletedData>
        (StringConstants.AdminHasDeletedData, (dto) => {
            console.log(dto)
            toast("someone has deleted everything")
            setDeviceLogs([]);
        })
        return () => reactToMessageSetup();
    }, [readyState, jwt]);



    return (<>
        
        <h1 className=" text-2xl font-bold mb-4  p-20  ">Data logs from weather station devices</h1>

        <div>
            <button onClick={() => {
                weatherStationClient.deleteData(localStorage.getItem('jwt')!).then(success => {
                    toast.success("wow you just deleted everything man")
                }).catch(failure => {
                    toast.error("you failed to delete everything")
                })
            }} className="btn btn-secondary btn-xl m-10">click here to delete data</button>
        </div>

        <ResponsiveContainer width="100%" height={400}>
            <BarChart
                width={500}
                height={300}
                data={deviceLogs}
                margin={{
                    top: 5,
                    right: 30,
                    left: 20,
                    bottom: 5,
                }}
            >
                <CartesianGrid strokeDasharray="3 3"/>
                <XAxis dataKey="formattedTime"/>
                <YAxis/>
                <Tooltip/>
                <Legend/>
                <Bar dataKey="value" name="Temperature" fill="#8884d8"
                     activeBar={<Rectangle fill="pink" stroke="blue"/>}/>
            </BarChart>
        </ResponsiveContainer>


    </>)
}