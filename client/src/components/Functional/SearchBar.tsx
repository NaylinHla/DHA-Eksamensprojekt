import React from 'react';
import { Search, SlidersHorizontal } from 'lucide-react';

interface SearchBarProps {
    searchTerm: string;
    onSearch: (value: string) => void;
}

const SearchBar: React.FC<SearchBarProps> = ({ searchTerm, onSearch }) => {
    return (
        <div className="relative w-full max-w-xs flex items-center bg-[var(--color-surface)] rounded-2xl px-3 py-2 border">
            <Search className="w-4 h-4 text-muted-foreground mr-2" />
            <input
                type="text"
                placeholder="Search devices…"
                value={searchTerm}
                onChange={(e) => onSearch(e.target.value)}
                className="flex-1 bg-transparent outline-none text-sm"
            />
            <SlidersHorizontal className="w-4 h-4 text-muted-foreground ml-2" />
        </div>
    );
};

export default SearchBar;
