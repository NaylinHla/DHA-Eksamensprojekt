import React from 'react';
import {Search, SlidersHorizontal} from 'lucide-react';

interface SearchBarProps {
    searchTerm: string;
    onSearch: (value: string) => void;
}

const SearchBar: React.FC<SearchBarProps> = ({searchTerm, onSearch}) => {
    return (
        <div className="relative bg-[var(--color-surface)] rounded-2xl w-full sm:w-[clamp(12rem,28vw,20rem)]">
            <Search className="absolute left-3 top-1/2 -translate-y-1/2 placeholder h-[clamp(0.75rem,1vw,1.25rem)] w-[clamp(0.75rem,1vw,1.50rem)] text-muted-foreground"/>
            <input
                type="text"
                placeholder="Search"
                value={searchTerm}
                onChange={(e) => onSearch(e.target.value)}
                className="pl-9 w-full text-[clamp(0.85rem,0.5vw,1.25rem)] py-[clamp(0.35rem,0.8vw,1rem)]"
            />
        </div>
    );
};

export default SearchBar;
