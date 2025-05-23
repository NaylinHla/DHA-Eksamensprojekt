import React, { useEffect, useState } from 'react';
import { formatDateTimeForUserTZ } from '../index';

interface TitleTimeHeaderProps {
    title: string;
}

const TitleTimeHeader: React.FC<TitleTimeHeaderProps> = ({ title }) => {
    const [currentTime, setCurrentTime] = useState(new Date());

    useEffect(() => {
        const id = setInterval(() => setCurrentTime(new Date()), 1000);
        return () => clearInterval(id);
    }, []);

    return (
        <header className="w-full shadow bg-[var(--color-surface)] flex items-center justify-between px-6 py-3">
            <h1 className="font-bold text-fluid-header text-[--color-primary]">{title}</h1>
            <span className="text-fluid text-[--color-primary] whitespace-nowrap">
                {formatDateTimeForUserTZ(currentTime)}
            </span>
        </header>
    );
};

export default TitleTimeHeader;
