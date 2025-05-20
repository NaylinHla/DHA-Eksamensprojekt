import {Link, NavLink, useNavigate} from "react-router";
import React, {useEffect} from "react";
import {links} from "../../types/NavLinks.ts"
import logo from "../../assets/Favicon/favicon.svg";
import toast from "react-hot-toast";
import {useAtom} from "jotai";
import {useLogout} from "../index.ts";
import {JwtAtom} from "../../atoms";
import useDropdown from "../../hooks/useDropdown.ts";

export default function NavBar() {
    const [jwt] = useAtom(JwtAtom);
    const navigate = useNavigate();

    const mobileDrop = useDropdown();
    const profileDrop = useDropdown();
    const myDeviceDrop = useDropdown();
    const {logout} = useLogout();

    useEffect(() => {
        mobileDrop.setOpen(false);
        profileDrop.setOpen(false);
        myDeviceDrop.setOpen(false);
    }, [location.pathname]);

    const handleLogout = () => {
        logout();
        toast("Logged out");
    };

    const liClass = "px-4 py-2 hover:bg-gray-100";

    return (
        <header className="navbar bg-primary text-white sticky top-0 z-50 px-4 shadow-md" style={{ padding: "clamp(0.5rem,1.5vw,1rem) clamp(1rem,3vw,2rem)" }}>
            <div className="navbar-start">
                <Link
                    to="/"
                    className="normal-case font-bold flex items-center gap-fluid text-[clamp(1rem,2.8vw,2.25rem)]"
                >
                    {/* Logo */}
                    <img
                        src={logo}
                        alt="Greenhouse logo"
                        className="shrink-0 h-[clamp(3rem,5vw,6rem)] w-[clamp(3rem,5vw,6rem)]"
                    />

                    {/* Application name */}
                    <span className="inline whitespace-nowrap">Greenhouse&nbsp;Application</span>
                </Link>
            </div>

            {jwt && jwt.length > 0 && (
                <>
                    {/* Desktop Nav */}
                    <div className="navbar-end">
                        <nav className="hidden lg:flex">
                            <ul className="menu menu-horizontal p-0 gap-2">
                                {links.map(({to, label}) => (
                                    <li key={to}>
                                        <NavLink
                                            to={to}
                                            className={({isActive}) =>
                                                `${isActive ? "font-semibold underline " : ""}text-[clamp(0.9rem,1.3vw,1.50rem)]`
                                            }
                                        >
                                            {label}
                                        </NavLink>
                                    </li>
                                ))}
                            </ul>
                        </nav>
                    </div>

                    {/* Mobile hamburger */}
                    <div className="flex-none lg:hidden relative " ref={mobileDrop.ref}>
                        {/* toggle button */}
                        <button
                            onClick={() => mobileDrop.setOpen(!mobileDrop.open)}
                            className="btn btn-ghost"
                        >
                            <svg xmlns="http://www.w3.org/2000/svg" className="h-5 w-5" fill="none"
                                 viewBox="0 0 24 24" stroke="currentColor">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2}
                                      d="M4 6h16M4 12h16M4 18h16"/>
                            </svg>
                        </button>

                        {/* dropdown list */}
                        {mobileDrop.open && (
                            <ul
                                className="absolute right-0 mt-3 w-44 rounded-box bg-[var(--color-surface)]
                       shadow divide-y divide-gray-200"
                            >
                                {links.map(({to, label}) => (
                                    <li key={to}>
                                        <NavLink
                                            to={to}
                                            onClick={() => mobileDrop.setOpen(false)}
                                            className="block px-4 py-2 text-green-600 hover:bg-gray-100"
                                        >
                                            {label}
                                        </NavLink>
                                    </li>
                                ))}
                            </ul>
                        )}
                    </div>

                    {/* User avatar & profile menu */}
                    <div className="ml-3 relative" ref={profileDrop.ref}>
                        <button
                            onClick={() => profileDrop.setOpen(!profileDrop.open)}
                            className="btn btn-ghost btn-circle avatar placeholder"
                        >
                            <div className="bg-[var(--color-surface)] text-green-600 rounded-full w-10">
                                <svg xmlns="http://www.w3.org/2000/svg" className="w-full h-full p-1" fill="none"
                                     viewBox="0 0 24 24" stroke="currentColor">
                                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2}
                                          d="M5.121 17.804A7 7 0 0112 14a7 7 0 016.879 3.804M15 9a3 3 0 11-6 0 3 3 0 016 0z"/>
                                </svg>
                            </div>
                        </button>

                        {profileDrop.open && (
                            <ul
                                className="absolute right-0 mt-3 w-44 rounded-box bg-[var(--color-surface)]
                       shadow divide-y divide-gray-200"
                            >
                                <li>
                                    <Link
                                        to="/profile"
                                        onClick={() => profileDrop.setOpen(false)}
                                        className="block px-4 py-2 text-green-600 hover:bg-gray-100"
                                    >
                                        Settings
                                    </Link>
                                </li>
                                <li>
                                    <Link
                                        to="/mydevice"
                                        onClick={() => myDeviceDrop.setOpen(false)}
                                        className="block px-4 py-2 text-green-600 hover:bg-gray-100"
                                    >
                                        My device
                                    </Link>
                                </li>
                                <li>
                                    <button
                                        onClick={handleLogout}
                                        className="block w-full text-left px-4 py-2 text-green-600 hover:bg-gray-100"
                                    >
                                        Logout
                                    </button>
                                </li>
                            </ul>
                        )}
                    </div>
                </>
            )}

            {/* Login button for guests */}
            {(!jwt || jwt.length === 0) && (
                <div className="navbar-end ml-auto">
                    <Link
                        to="/signin"
                        className="btn text-white border-white bg-transparent btn-sm"
                    >
                        Login
                    </Link>
                </div>
            )}
        </header>
    );
}
