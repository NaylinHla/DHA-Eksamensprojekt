import React, { useState, useEffect } from 'react';
import { formatDateTimeForUserTZ } from '../index';

const TitleTimeHeader: React.FC = () => {
    const [currentTime, setCurrentTime] = useState(new Date());

    // Update time every minute (or interval of your choice)
    useEffect(() => {
        const intervalId = setInterval(() => {
            setCurrentTime(new Date());
        }, 1000);

        return () => clearInterval(intervalId);
    }, []);

    return (
        <header className="w-full bg-background shadow px-6 py-4 flex justify-between items-center">
            <h1 className="text-2xl font-bold text-[--color-primary]">My Device</h1>
            <span className="text-sm text-gray-600">
                {formatDateTimeForUserTZ(currentTime)}
            </span>
        </header>
    );
};

export default TitleTimeHeader;
