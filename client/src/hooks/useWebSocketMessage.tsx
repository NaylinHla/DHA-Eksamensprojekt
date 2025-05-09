import {useEffect} from "react";
import {useWsClient} from "ws-request-hook"; // Assuming you're using this or a similar library for WebSocket client

// Generic WebSocket hook
const useWebSocketMessage = (messageKey: string, callback: (data: any) => void) => {
    const {onMessage, readyState} = useWsClient(); // Assuming this provides WebSocket connection status and message handling

    useEffect(() => {
        if (readyState !== 1) return; // Ensure the WebSocket connection is open (readyState 1 is connected)

        // Subscribe to the message key
        const unsubscribe = onMessage(messageKey, callback);

        // Cleanup the subscription on component unmount or dependency change
        return () => {
            unsubscribe();
        };
    }, [readyState, messageKey, callback]); // Depend on WebSocket readiness, message key, and callback

    // No need to return anything unless needed by the component
};

export default useWebSocketMessage;
