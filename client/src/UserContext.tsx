import React, { createContext, useContext, useState } from "react";

interface UserContextType {
    user: string | null;
    setUser: (value: string | null) => void;
    reset: () => void;
}

const UserContext = createContext<UserContextType | undefined>(undefined);

export const UserProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
    const [user, setUser] = useState<string | null>(null);

    const reset = () => {
        setUser(null);
    };

    return (
        <UserContext.Provider value={{ user, setUser, reset }}>
            {children}
        </UserContext.Provider>
    );
};

export const useUser = () => {
    const context = useContext(UserContext);
    if (!context) throw new Error("useUser must be used within UserProvider");
    return context;
};
