import {useCallback} from "react";
import {useAtom} from "jotai";
import {subscriptionClient} from "../apiControllerClients";
import {JwtAtom, RandomUidAtom, SubscribedTopicsAtom} from "./import";
import {useWsClient} from "ws-request-hook"; // assuming this returns readyState

export default function useTopicManager() {
    const [jwt] = useAtom(JwtAtom);
    const [, setSubscribedTopics] = useAtom(SubscribedTopicsAtom);
    const {readyState} = useWsClient();
    const [clientId] = useAtom(RandomUidAtom); // Extracting the value of RandomUidAtom

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

    return {subscribe, unsubscribe};
}
