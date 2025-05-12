import React from 'react';

interface IconProps extends React.SVGProps<SVGSVGElement> {
    className?: string;
    color?: string;
    size?: number;
}

const TrashBinIcon: React.FC<IconProps> = ({
                                               className,
                                               color = "currentColor", // Default color
                                               size = 24, // Default size
                                               ...props
                                           }) => (
    <svg
        xmlns="http://www.w3.org/2000/svg"
        width={size}
        height={size}
        viewBox="0 0 24 24"
        className={className}
        fill="none"
        stroke={color}
        aria-label="Trash Icon"
        {...props}
    >
        <path
            d="M3 6H5H21"
            stroke={color}
            strokeWidth="2"
            strokeLinecap="round"
            strokeLinejoin="round"
        />
        <path
            d="M8 6V4C8 3.44772 8.44772 3 9 3H15C15.5523 3 16 3.44772 16 4V6"
            stroke={color}
            strokeWidth="2"
            strokeLinecap="round"
            strokeLinejoin="round"
        />
        <path
            d="M5 6H19C19.5523 6 20 6.44772 20 7V21C20 21.5523 19.5523 22 19 22H5C4.44772 22 4 21.5523 4 21V7C4 6.44772 4.44772 6 5 6Z"
            stroke={color}
            strokeWidth="2"
            strokeLinecap="round"
            strokeLinejoin="round"
        />
        <path
            d="M10 11V17"
            stroke={color}
            strokeWidth="2"
            strokeLinecap="round"
            strokeLinejoin="round"
        />
        <path
            d="M14 11V17"
            stroke={color}
            strokeWidth="2"
            strokeLinecap="round"
            strokeLinejoin="round"
        />
    </svg>
);

export default TrashBinIcon;
