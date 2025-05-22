// Holds the array of topics we’re currently subscribed to
import {atomWithStorage} from "jotai/utils";

export const SubscribedTopicsAtom = atomWithStorage<string[]>("subscribedTopics", []);

