import { useState } from "react";
import logo from "../../assets/Favicon/favicon.svg";

type AuthScreenProps = {
    onLogin?: () => void;
};

const AuthScreen: React.FC<AuthScreenProps> = ({ onLogin }) => {
    // idle = main front page - login = login form is opened - register = register form is opened
    const [mode, setMode] = useState<"idle" | "login" | "register">("idle");
    const [loggedIn, setLoggedIn] = useState(false);

    // --- ANIMATION STUFF ---
    const lift = "-translate-y-10"; // How far up does the animation lift
    const reset = () => setMode("idle");
    const fade = (visible: boolean) =>
        visible
            ? "opacity-100 pointer-events-auto"
            : "opacity-0 pointer-events-none";

    // ------------------------

    const handleLogin = (e: React.FormEvent<HTMLFormElement>) => {
        e.preventDefault();
        setLoggedIn(true);

        onLogin?.();
    };

    return (
        <main className="relative flex min-h-screen flex-col items-center justify-center bg-primary font-display text-base-100">
            {/* Header */}
            <h1 className="absolute top-5 text-xl tracking-wider font-bold lg:text-3xl sm:text-3xl">
                Greenhouse Application
            </h1>

            {/* Body */}
            <section className="flex w-full max-w-1xl flex-col items-center justify-center gap-10 px-6 md:flex-row md:gap-20">
                <img
                    src={logo}
                     alt="Greenhouse"
                    className={
                        `w-40 select-none transition-transform duration-300 md:w-64 lg:w-72 ` +
                        (mode === "idle" ? "translate-y-0" : "-translate-y-10 md:translate-y-0")
                    }
                />

                {/* AUTH COLUMN */}
                <div className="relative flex w-full max-w-xs flex-col items-center text-center md:max-w-sm">
                    {/* Login / Register */}
                    <div
                        className={
                            "flex flex-col items-center transition-transform duration-300 " +
                            (mode === "login"
                                ? lift
                                : mode === "register"
                                    ? "hidden"
                                    : "translate-y-0")
                        }
                    >
                        {/* Label */}
                        <button
                            type="button"
                            className="text-xl font-medium"
                            onClick={() => setMode(mode === "login" ? "idle" : "login")}
                        >
                            Login
                        </button>

                        {/* Underline */}
                        <span className="mt-1 h-px w-32 bg-base-100"/>
                    </div>

                    <div
                        className={
                            "flex flex-col items-center transition-transform duration-300 " +
                            (mode === "register"
                                ? lift
                                : mode === "login"
                                    ? "hidden"
                                    : "translate-y-0")
                        }
                    >
                        {/* Label */}
                        <button
                            type="button"
                            className="text-xl font-medium"
                            onClick={() => setMode(mode === "register" ? "idle" : "register")}
                        >
                            Register
                        </button>

                        {/* Underline */}
                        <span className="mt-1 h-px w-32 bg-base-100"/>
                    </div>

                    {/* Forms */}
                    <form
                        onSubmit={handleLogin}
                        className={
                            `absolute top-0 w-full space-y-2 transition-opacity duration-300 ` +
                            fade(mode === "login")
                        }
                    >
                        <label className="label py-0 text-left">Email</label>
                        <input
                            type="email"
                            className="input input-bordered input-sm w-full text-black"
                            required
                        />
                        <label className="label py-0 text-left">Password</label>
                        <input
                            type="password"
                            className="input input-bordered input-sm w-full text-black"
                            required
                        />
                        <button className="btn btn-neutral btn-sm w-full">Login</button>
                        <button type="button" className="btn btn-link btn-xs w-full" onClick={reset}>
                            Back
                        </button>
                    </form>

                    <form
                        onSubmit={(e) => e.preventDefault()}
                        className={
                            `absolute top-0 w-full space-y-2 transition-opacity duration-300 ` +
                            fade(mode === "register")
                        }
                    >
                        {/* --- register fields --- */}
                        <label className="label py-0 text-left">Name</label>
                        <input className="input input-bordered input-sm w-full text-black" required/>
                        <label className="label py-0 text-left">Birthday</label>
                        <input type="date" className="input input-bordered input-sm w-full text-black" required/>
                        <label className="label py-0 text-left">Email</label>
                        <input type="email" className="input input-bordered input-sm w-full text-black" required/>
                        <label className="label py-0 text-left">Password</label>
                        <input type="password" className="input input-bordered input-sm w-full text-black" required/>
                        <label className="label py-0 text-left">Confirm password</label>
                        <input type="password" className="input input-bordered input-sm w-full text-black" required/>
                        <div className="flex gap-2 pt-1">
                            <button type="button" className="btn btn-neutral btn-xs flex-1" onClick={reset}>
                                Back
                            </button>
                            <button type="submit" className="btn btn-primary btn-xs flex-1">
                                Register
                            </button>
                        </div>
                    </form>
                </div>
            </section>

            {/* Fake logged‑in notice */}
            {loggedIn && (
                <p className="absolute bottom-4 text-xs italic opacity-60">
                    You are now logged in (placeholder)
                </p>
            )}
        </main>
    );
};

export default AuthScreen;