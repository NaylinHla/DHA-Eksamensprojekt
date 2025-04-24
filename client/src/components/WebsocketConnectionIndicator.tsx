import {useWsClient} from "ws-request-hook";

export default function WebsocketConnectionIndicator(){
    
    const {readyState} = useWsClient();
    
    const badgeColor = readyState == 1 ? 'badge-success' : readyState == 2 ? 'badge-warning' : 'badge-error';
    
    return (<>
         <span className="absolute top-0 right-0 m-10">
        <div className="indicator">
            <span className={'indicator-item badge '+badgeColor }></span>
            <button className="btn">Websocket connection status</button>
        </div>
     </span>
    </>)
};