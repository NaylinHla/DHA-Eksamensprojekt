import { Link, NavLink } from "react-router";
import React from "react";
import { links } from "../../types/NavLinks.ts"
import logo from "../../assets/Favicon/favicon.svg";

export default function NavBar() {
    return (
        <header className="navbar bg-primary text-white sticky top-0 z-50 px-4 shadow-md">
            <div className="navbar-start">
                <Link
                    to="/"
                    className="normal-case lg:text-3xl sm:text-2xl font-bold flex items-center gap-2"
                >
                    {/* Logo */}
                    <img
                        src={logo}
                        alt="Greenhouse logo"
                        className="h-8 w-8 lg:h-10 lg:w-10 shrink-0"
                    />

                    {/* Application name */}
                    <span className="inline whitespace-nowrap">Greenhouse&nbsp;Application</span>
                </Link>
            </div>

            {/* Desktop links */}
            <div className="navbar-end">
                <nav className="hidden lg:flex">
                    <ul className="menu menu-horizontal p-0 gap-2">
                        {links.map(({ to, label }) => (
                            <li key={to}>
                                <NavLink
                                    to={to}
                                    className={({ isActive }) =>
                                        isActive ? "font-semibold underline" : undefined
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
            <div className="flex-none lg:hidden">
                <details className="dropdown dropdown-end">
                    <summary className="btn btn-ghost">
                        <svg
                            xmlns="http://www.w3.org/2000/svg"
                            className="h-5 w-5"
                            fill="none"
                            viewBox="0 0 24 24"
                            stroke="currentColor"
                        >
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 6h16M4 12h16M4 18h16" />
                        </svg>
                    </summary>
                    <ul className="menu dropdown-content mt-3 p-2 shadow bg-base-100 rounded-box w-44 text-green-600">
                        {links.map(({ to, label }) => (
                            <li key={to}>
                                <NavLink to={to}>{label}</NavLink>
                            </li>
                        ))}
                    </ul>
                </details>
            </div>

            {/* User avatar and profile menu */}
            <div className="ml-3">
                <details className="dropdown dropdown-end">
                    <summary className="btn btn-ghost btn-circle avatar placeholder">
                        <div className="bg-white text-green-600 rounded-full w-10">
                            <svg
                                xmlns="http://www.w3.org/2000/svg"
                                className="w-full h-full p-1"
                                fill="none"
                                viewBox="0 0 24 24"
                                stroke="currentColor"
                            >
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5.121 17.804A7 7 0 0112 14a7 7 0 016.879 3.804M15 9a3 3 0 11-6 0 3 3 0 016 0z" />
                            </svg>
                        </div>
                    </summary>
                    <ul className="menu dropdown-content mt-3 p-2 shadow bg-base-100 rounded-box w-44 text-green-600">
                        <li>
                            <Link to="/profile">Profile</Link>
                        </li>
                        <li>
                            <button>Logout</button>
                        </li>
                    </ul>
                </details>
            </div>
        </header>
    );
}
