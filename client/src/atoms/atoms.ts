import { AlertResponseDto } from "./import";
import { atom } from "jotai";
import { atomWithStorage } from "jotai/utils";

const getInitialJwt = () => {
    if (typeof window !== "undefined") {
        const stored = localStorage.getItem("jwt");
        return stored ?? "";
    }
    return ""; // fallback for SSR
};

export const JwtAtom = typeof window !== "undefined"
    ? atomWithStorage<string>("jwt", "")
    : atom("");

export const AlertsAtom = atom<AlertResponseDto[]>([]);

export const RandomUidAtom = atom<string>(getOrGenerateUid());

export const UserIdAtom = atomWithStorage<string>("userId", ""); // ✅ New Atom

function getOrGenerateUid(): string {
    if (typeof window === "undefined") return "";

    const existing = localStorage.getItem("randomUid");
    if (existing) return existing;

    const newUid = crypto.randomUUID();
    localStorage.setItem("randomUid", newUid);
    return newUid;
}
