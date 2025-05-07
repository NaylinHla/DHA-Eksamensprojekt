import {useAtom} from 'jotai';
import {useEffect, useState} from 'react';
import {WsClientProvider} from 'ws-request-hook';
import ApplicationRoutes from './ApplicationRoutes';
import {DevTools} from 'jotai-devtools';
import 'jotai-devtools/styles.css';
import {RandomUidAtom} from './import';

const baseUrl = import.meta.env.VITE_API_BASE_URL;
const prod = import.meta.env.PROD;

export default function App() {
    const [serverUrl, setServerUrl] = useState<string>();
    const [randomUid, setRandomUid] = useAtom(RandomUidAtom);

    useEffect(() => {
        if (!randomUid) {
            const newUid = crypto.randomUUID();
            setRandomUid(newUid);
            console.log("Generated and set new UID:", newUid);
        } else {
            console.log("UID already present:", randomUid);
        }
    }, [randomUid]);

    useEffect(() => {
        const finalUrl = prod ? 'wss://' + baseUrl + '?id=' + randomUid : 'ws://' + baseUrl + '?id=' + randomUid;
        setServerUrl(finalUrl);
    }, [prod, baseUrl]);
    
    return (<>
        {serverUrl && <WsClientProvider url={serverUrl}>
            <ApplicationRoutes/>
        </WsClientProvider>}
        {!prod && <DevTools/>}


        <div
            id="toast-container"
            className="fixed top-6 left-1/2 -translate-x-1/2 z-50 flex flex-col items-center space-y-2"
        ></div>
    </>)
}