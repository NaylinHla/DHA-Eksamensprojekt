import React, { useState, useEffect } from 'react';
import { formatDateTimeForUserTZ } from '../index';

// Define props type
interface TitleTimeHeaderProps {
    title: string;
}

const TitleTimeHeader: React.FC<TitleTimeHeaderProps> = ({ title }) => {
    const [currentTime, setCurrentTime] = useState(new Date());

    useEffect(() => {
        const intervalId = setInterval(() => {
            setCurrentTime(new Date());
        }, 1000);

        return () => clearInterval(intervalId);
    }, []);

    return (
        <header className="w-full bg-[var(--color-surface)] shadow px-6 py-4 flex justify-between items-center">
            <h1 className="text-2xl font-bold text-[--color-primary]">{title}</h1>
            <span className="text-sm text-gray-600">
        {formatDateTimeForUserTZ(currentTime)}
      </span>
        </header>
    );
};

export default TitleTimeHeader;
