export interface JwtPayload { 
    country?: string; 
    Country?: string; 
}

function decodeSegment(seg: string): string {
    // 1. URL → standard alphabet
    const base64 = seg.replace(/-/g, "+").replace(/_/g, "/")
        // 2. restore padding
        .padEnd(Math.ceil(seg.length / 4) * 4, "=");
    return atob(base64);
}

export function parseJwt(token: string | undefined): JwtPayload | null {
    if (!token) return null;
    try {
        const [, payload] = token.split(".");
        return JSON.parse(decodeSegment(payload)) as JwtPayload;
    } catch {
        return null;
    }
}