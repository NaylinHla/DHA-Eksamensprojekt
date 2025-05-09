import {atom} from "jotai";

// Holds the array of topics we’re currently subscribed to
export const SubscribedTopicsAtom = atom<string[]>([]);
