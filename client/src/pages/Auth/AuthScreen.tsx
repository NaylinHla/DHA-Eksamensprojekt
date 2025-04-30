import { useState } from "react";
import logo from "../../assets/Favicon/favicon.svg";
import { PasswordField } from "../../components/PasswordField/PasswordField.tsx"
import { JwtAtom } from "../../atoms/atoms.ts"
import {useAtom} from "jotai";
import toast from "react-hot-toast";

type AuthScreenProps = {
    onLogin?: () => void;
};

const AuthScreen: React.FC<AuthScreenProps> = ({ onLogin }) => {
    // idle = main front page - login = login form is opened - register = register form is opened
    const [mode, setMode] = useState<"idle" | "login" | "register">("idle");
    const [, setJwt]   = useAtom(JwtAtom);
    const [email, setEmail]       = useState("");
    const [password, setPassword] = useState("");

    // --- ANIMATION STUFF ---
    const lift = {
        idle: "translate-y-0",
        login: "-translate-y-10",
        register: "-translate-y-44",
    } as const;
    const mobileLogoLift =
        mode === "register" ? "-translate-y-32" : mode === "login" ? "-translate-y-12" : "translate-y-0";
    const reset = () => setMode("idle");
    const fade = (visible: boolean) =>
        visible
            ? "opacity-100 pointer-events-auto"
            : "opacity-0 pointer-events-none";

    // ------------------------

    const handleLogin = async (e: React.FormEvent<HTMLFormElement>) => {
        e.preventDefault();

        try {
            const res = await fetch("/api/auth/Login", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ email, password }),
            });

            if (!res.ok) {
                toast.error("Wrong e-mail or password");
                return;
            }

            const { jwt: token } = (await res.json()) as { jwt: string };

            if (!token) {
                toast.error("Server didn’t return a token");
                return;
            }

            setJwt(token);
            localStorage.setItem("jwt", token);
            
            onLogin?.();
        } catch (err) {
            toast.error("Couldn’t reach the server – try again later");
        }
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
                    className={`w-40 sm:w-48 md:w-64 lg:w-96 select-none transition-transform duration-300 ${mobileLogoLift} md:translate-y-0`}
                />

                {/* AUTH COLUMN */}
                <div className="relative flex w-full max-w-xs flex-col items-center text-center md:max-w-sm">
                    {/* Login / Register */}
                    <div
                        className={`flex flex-col items-center transition-transform duration-300 ${
                            lift[mode === "login" ? "login" : "idle"]
                        } ${mode === "register" ? "opacity-0 pointer-events-none" : ""}`}
                    >
                        <button
                            type="button"
                            className="text-xl font-medium"
                            onClick={() => setMode(mode === "login" ? "idle" : "login")}
                        >
                            Login
                        </button>
                        <span className="mt-1 h-px w-32 bg-base-100"/>
                    </div>

                    <div
                        className={`flex flex-col items-center transition-transform duration-300 ${
                            lift[mode === "register" ? "register" : "idle"]
                        } ${mode === "login" ? "opacity-0 pointer-events-none" : ""}`}
                    >
                        <button
                            type="button"
                            className="text-xl font-medium"
                            onClick={() => setMode(mode === "register" ? "idle" : "register")}
                        >
                            Register
                        </button>
                        <span className="mt-1 h-px w-32 bg-base-100"/>
                    </div>

                    {/* Login Form */}
                    <form
                        onSubmit={handleLogin}
                        className={
                            `absolute top-0 w-full space-y-2 transition-opacity duration-300 ` +
                            fade(mode === "login")
                        }
                    >
                        <label className="label py-0 text-white">Email</label>
                        <input
                            type="email"
                            value={email}
                            onChange={e => setEmail(e.target.value)}
                            placeholder="Email"
                            className="input input-bordered input-sm w-full text-black"
                            required
                        /> {/* change type to email later when not in testing, this is just to "login" faster */}
                        <label className="label py-0 text-white">Password</label>
                        <PasswordField
                            value={password}
                            onChange={(e) => setPassword(e.target.value)}
                            placeholder="Password"
                            required
                        />
                        <button className="btn btn-neutral btn-sm w-full">Login</button>
                        <button type="button" className="btn btn-link btn-xs w-full" onClick={reset}>
                            Back
                        </button>
                    </form>

                    {/* Register Form */}
                    <form
                        onSubmit={(e) => e.preventDefault()}
                        className={`absolute top-0 w-full -translate-y-24 space-y-2 transition-opacity duration-300 ${fade(
                            mode === "register",
                        )}`}
                    >
                        <div className="flex gap-2">
                            <div className="flex-1">
                                <label className="label py-0 text-white">First Name</label>
                                <input
                                    placeholder="First Name"
                                    className="input input-bordered input-sm w-full text-black"
                                    required
                                />
                            </div>
                            <div className="flex-1">
                                <label className="label py-0 text-white">Last Name</label>
                                <input
                                    placeholder="Last Name"
                                    className="input input-bordered input-sm w-full text-black"
                                    required
                                />
                            </div>
                        </div>

                        <label className="label py-0 text-white">Email</label>
                        <input
                            type="email"
                            placeholder="Email"
                            className="input input-bordered input-sm w-full text-black"
                            required/>

                        <div className="flex gap-2">
                            <div className="flex-1">
                                <label className="label py-0 text-white">Birthday</label>
                                <input
                                    type="date"
                                    className="input input-bordered input-sm w-full text-black"
                                    required/>
                            </div>
                            <div className="flex-1">
                                <label className="label py-0 text-white">Country</label>
                                <select className="select select-bordered select-sm w-full text-black" required>
                                    <option disabled selected>
                                        Choose...
                                    </option>
                                    <option>Country 1</option>
                                    <option>Country 2</option>
                                </select>
                            </div>
                        </div>

                        <label className="label py-0 text-white">Password</label>
                        <PasswordField
                            placeholder="Password"
                            required
                        />

                        <label className="label py-0 text-white">Confirm password</label>
                        <PasswordField
                            placeholder="Password"
                            required
                        />

                        <div className="flex gap-2 pt-1">
                            <button type="button" className="btn btn-neutral btn-xs flex-1" onClick={reset}>
                                Back
                            </button>
                            <button type="submit" className="btn btn-primary border-white btn-xs flex-1">
                                Register
                            </button>
                        </div>
                    </form>
                </div>
            </section>
        </main>
    );
};

export default AuthScreen;