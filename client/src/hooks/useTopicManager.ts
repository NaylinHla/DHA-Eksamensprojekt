import {useCallback, useEffect, useRef} from "react";
import {useAtom} from "jotai";
import {subscriptionClient} from "../apiControllerClients";
import {JwtAtom, RandomUidAtom, SubscribedTopicsAtom} from "./import";
import {useWsClient} from "ws-request-hook"; // assuming this returns readyState

export default function useTopicManager() {
    const [jwt] = useAtom(JwtAtom);
    const [subscribedTopics, setSubscribedTopics] = useAtom(SubscribedTopicsAtom);
    const {readyState} = useWsClient();
    const [clientId] = useAtom(RandomUidAtom); // Extracting the value of RandomUidAtom

    const prevReadyStateRef = useRef(readyState);

    const subscribe = useCallback(async (topic: string) => {
        if (readyState !== 1) {
            console.warn("WebSocket not ready, cannot subscribe:", topic);
            return;
        }
        if (!jwt) {
            console.warn("Cannot subscribe without JWT");
            return;
        }
        try {
            await subscriptionClient.subscribe(jwt, {clientId, topicIds: [topic]});
            setSubscribedTopics(prev => prev.includes(topic) ? prev : [...prev, topic]);
            console.log(`✅ Subscribed to topic: ${topic}`);
        } catch (err) {
            console.error(`❌ Failed to subscribe to topic: ${topic}`, err);
        }
    }, [jwt, readyState, setSubscribedTopics, clientId]); // Added clientId to the dependencies

    const unsubscribe = useCallback(async (topic: string) => {
        if (readyState !== 1) {
            console.warn("WebSocket not ready, cannot unsubscribe:", topic);
            return;
        }
        if (!jwt) {
            console.warn("Cannot unsubscribe without JWT");
            return;
        }
        try {
            await subscriptionClient.unsubscribe(jwt, {clientId, topicIds: [topic]});
            setSubscribedTopics(prev => prev.filter(t => t !== topic));
            console.log(`✅ Unsubscribed from topic: ${topic}`);
        } catch (err) {
            console.error(`❌ Failed to unsubscribe from topic: ${topic}`, err);
        }
    }, [jwt, readyState, setSubscribedTopics, clientId]); // Added clientId to the dependencies

    // Resubscribe to all topics after reconnect
    const resubscribeAll = useCallback(async () => {
        if (readyState !== 1) return;
        if (!jwt) return;

        for (const topic of subscribedTopics) {
            try {
                await subscriptionClient.subscribe(jwt, { clientId, topicIds: [topic] });
                console.log(`🔄 Resubscribed to topic: ${topic}`);
            } catch (err) {
                console.error(`❌ Failed to resubscribe to topic: ${topic}`, err);
            }
        }
    }, [jwt, readyState, subscribedTopics, clientId]);

    // Watch for websocket reconnection
    useEffect(() => {
        if (prevReadyStateRef.current !== 1 && readyState === 1) {
            // Just reconnected
            resubscribeAll().then();
        }
        prevReadyStateRef.current = readyState;
    }, [readyState, resubscribeAll]);

    return { subscribe, unsubscribe, resubscribeAll };
}
