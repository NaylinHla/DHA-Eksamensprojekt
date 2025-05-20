import React, {useEffect, useState} from 'react';
import {formatDateTimeForUserTZ} from '../index';

// Define props type
interface TitleTimeHeaderProps {
    title: string;
}

const TitleTimeHeader: React.FC<TitleTimeHeaderProps> = ({title}) => {
    const [currentTime, setCurrentTime] = useState(new Date());

    useEffect(() => {
        const id = setInterval(() => setCurrentTime(new Date()), 1000);
        return () => clearInterval(id);
    }, []);

    return (
        <header
            className="w-full shadow bg-[var(--color-surface)] flex items-center justify-between p-fluid"
            style={{ padding: "clamp(0.75rem,1.5vw,1.5rem) clamp(1.5rem,3.5vw,4rem)" }}
        >
            <h1 className="font-bold text-fluid-header text-[--color-primary]">{title}</h1>
            <span className="text-fluid text-gray-600 whitespace-nowrap">
        {formatDateTimeForUserTZ(currentTime)}
      </span>
        </header>
    );
};

export default TitleTimeHeader;
