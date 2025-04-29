import { useState } from "react";
import logo from "../../assets/Favicon/favicon.svg";
import { Api, AuthLoginDto, AuthRegisterDto } from "../../api/api.ts";

type AuthScreenProps = {
    onLogin?: () => void;
};

const api = new Api({ baseUrl: "http://localhost:5000" });

const AuthScreen: React.FC<AuthScreenProps> = ({ onLogin }) => {
    const [mode, setMode] = useState<"idle" | "login" | "register">("idle");
    const [loggedIn, setLoggedIn] = useState(false);

    const lift = {
        idle: "translate-y-0",
        login: "-translate-y-10",
        register: "-translate-y-44",
    } as const;

    const mobileLogoLift =
        mode === "register"
            ? "-translate-y-32"
            : mode === "login"
                ? "-translate-y-12"
                : "translate-y-0";

    const reset = () => setMode("idle");

    const fade = (visible: boolean) =>
        visible ? "opacity-100 pointer-events-auto" : "opacity-0 pointer-events-none";

    const handleLogin = async (e: React.FormEvent<HTMLFormElement>) => {
        e.preventDefault();
        const formData = new FormData(e.currentTarget);
        const email = formData.get("email") as string;
        const password = formData.get("password") as string;

        try {
            const loginDto: AuthLoginDto = { email, password };
            const response = await api.api.authLogin(loginDto);
            const { jwt } = response.data;
            localStorage.setItem("jwt", jwt);
            setLoggedIn(true);
            onLogin?.();
        } catch (error) {
            console.error("Login failed", error);
            alert("Login failed. Please check your credentials.");
        }
    };

    const handleRegister = async (e: React.FormEvent<HTMLFormElement>) => {
        e.preventDefault();
        const formData = new FormData(e.currentTarget);

        const firstName = formData.get("firstName") as string;
        const lastName = formData.get("lastName") as string;
        const email = formData.get("email") as string;
        const birthday = formData.get("birthday") as string;
        const country = formData.get("country") as string;
        const password = formData.get("password") as string;
        const confirmPassword = formData.get("confirmPassword") as string;

        if (password !== confirmPassword) {
            alert("Passwords do not match!");
            return;
        }

        try {
            const registerDto: AuthRegisterDto = {
                firstName,
                lastName,
                email,
                birthday,
                country,
                password,
            };

            await api.api.authRegister(registerDto);
            alert("Registered successfully! You can now log in.");
            reset();
        } catch (error) {
            console.error("Registration failed", error);
            alert("Registration failed. Try again.");
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
                        <span className="mt-1 h-px w-32 bg-base-100" />
                    </div>

                    {/* Login Form */}
                    <form
                        onSubmit={handleLogin}
                        className={`absolute top-0 w-full space-y-2 transition-opacity duration-300 ${fade(
                            mode === "login",
                        )}`}
                    >
                        <label className="label py-0 text-white">Email</label>
                        <input
                            name="email"
                            type="email"
                            placeholder="Email"
                            className="input input-bordered input-sm w-full text-black"
                            required
                        />
                        <label className="label py-0 text-white">Password</label>
                        <input
                            name="password"
                            type="password"
                            placeholder="Password"
                            className="input input-bordered input-sm w-full text-black"
                            required
                        />
                        <button className="btn btn-neutral btn-sm w-full">Login</button>
                        <button type="button" className="btn btn-link btn-xs w-full" onClick={reset}>
                            Back
                        </button>
                    </form>

                    {/* Register Form */}
                    <form
                        onSubmit={handleRegister}
                        className={`absolute top-0 w-full -translate-y-24 space-y-2 transition-opacity duration-300 ${fade(
                            mode === "register",
                        )}`}
                    >
                        <div className="flex gap-2">
                            <div className="flex-1">
                                <label className="label py-0 text-white">First Name</label>
                                <input
                                    name="firstName"
                                    placeholder="First Name"
                                    className="input input-bordered input-sm w-full text-black"
                                    required
                                />
                            </div>
                            <div className="flex-1">
                                <label className="label py-0 text-white">Last Name</label>
                                <input
                                    name="lastName"
                                    placeholder="Last Name"
                                    className="input input-bordered input-sm w-full text-black"
                                    required
                                />
                            </div>
                        </div>

                        <label className="label py-0 text-white">Email</label>
                        <input
                            name="email"
                            type="email"
                            placeholder="Email"
                            className="input input-bordered input-sm w-full text-black"
                            required
                        />

                        <div className="flex gap-2">
                            <div className="flex-1">
                                <label className="label py-0 text-white">Birthday</label>
                                <input
                                    name="birthday"
                                    type="date"
                                    className="input input-bordered input-sm w-full text-black"
                                    required
                                />
                            </div>
                            <div className="flex-1">
                                <label className="label py-0 text-white">Country</label>
                                <input
                                    name="country"
                                    placeholder="Country"
                                    className="input input-bordered input-sm w-full text-black"
                                    required
                                />
                            </div>
                        </div>

                        <label className="label py-0 text-white">Password</label>
                        <input
                            name="password"
                            type="password"
                            placeholder="Password"
                            className="input input-bordered input-sm w-full text-black"
                            required
                        />

                        <label className="label py-0 text-white">Confirm password</label>
                        <input
                            name="confirmPassword"
                            type="password"
                            placeholder="Confirm Password"
                            className="input input-bordered input-sm w-full text-black"
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

            {/* Logged-in Notice */}
            {loggedIn && (
                <p className="absolute bottom-4 text-xs italic opacity-60">
                    You are now logged in (placeholder)
                </p>
            )}
        </main>
    );
};

export default AuthScreen;
