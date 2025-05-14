import { AlertResponseDto, atom, atomWithStorage } from "./import";

const getInitialJwt = (): string | null => {
    if (typeof window !== 'undefined') {
        return localStorage.getItem('jwt');
    }
    return null;
};

export const JwtAtom = atomWithStorage<string | null>('jwt', null);

export const AlertsAtom = atom<AlertResponseDto[]>([]);

export const RandomUidAtom = atom<string>(getOrGenerateUid());

function getOrGenerateUid(): string {
    if (typeof window === 'undefined') return '';

    const existing = localStorage.getItem('randomUid');
    if (existing) return existing;

    const newUid = crypto.randomUUID();
    localStorage.setItem('randomUid', newUid);
    return newUid;
}
