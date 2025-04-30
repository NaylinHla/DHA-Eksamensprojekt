import {atom, atomWithStorage, AlertResponseDto} from "./import";



const getInitialJwt = () => {
    if (typeof window !== 'undefined') {
        const stored = localStorage.getItem('jwt');
        return stored ?? '';
    }
    return ''; // fallback for SSR
};

export const JwtAtom = typeof window !== 'undefined'
    ? atomWithStorage<string>('jwt', getInitialJwt())
    : atom(''); // SSR fallback

export const AlertsAtom = atom<AlertResponseDto[]>([]);

export const RandomUidAtom = atom<string>(getOrGenerateUid());


function getOrGenerateUid(): string {
    const existing = localStorage.getItem('randomUid');
    if (existing) return existing;

    const newUid = crypto.randomUUID();
    localStorage.setItem('randomUid', newUid);
    return newUid;
}
