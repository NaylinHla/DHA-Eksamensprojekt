import React, {useEffect, useLayoutEffect, useRef, useState} from "react";
import logo from "../../assets/Favicon/favicon.svg";
import {AuthLoginDto, AuthRegisterDto} from "../../generated-client.ts";
import {authClient} from "../../apiControllerClients.ts";
import {PasswordField} from "../../components/utils/PasswordField/PasswordField.tsx";
import {JwtAtom, useAtom, UserIdAtom} from "../../components/import";
import toast from "react-hot-toast";
import countries from "./countries.json";
import {useTopicManager} from "../../hooks";

type AuthScreenProps = {
    onLogin?: () => void;
};

interface RegisterErrors {
    firstName?: string;
    lastName?: string;
    email?: string;
    birthday?: string;
    country?: string;
    password?: string;
    confirmPassword?: string;
}

interface ValidationErrors {
    email?: string;
    password?: string;

    [key: string]: string | undefined;
}

const AuthScreen: React.FC<AuthScreenProps> = ({onLogin}) => {
    const [mode, setMode] = useState<"idle" | "login" | "register">("idle");
    const [loggedIn] = useState(false);
    const [jwt, setJwt] = useAtom(JwtAtom);
    const [, setUserId] = useAtom(UserIdAtom);
    const [registerErrors, setRegisterErrors] = useState<RegisterErrors>({});
    const errorClass = "border-red-500 focus:border-red-600";
    const wrapperRef = useRef<HTMLDivElement>(null);
    const registerFormRef = useRef<HTMLFormElement>(null);
    const loginFormRef = useRef<HTMLFormElement>(null);
    const [loginErrors, setLoginErrors] = useState<{ email?: string; password?: string }>({});
    const { subscribe } = useTopicManager();
    const passwordRegex = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*]).{6,}$/;
    const passwordErrorMsg =
        "≥6 chars, incl. min. 1 uppercase, lowercase, digit & special.";

    // ANIMATION ---
    useLayoutEffect(() => {
        const wrapper = wrapperRef.current;
        const register = registerFormRef.current;

        if (!wrapper) return;

        if (mode === "register" && register) {
            wrapper.style.height = "7rem";
        } else if (mode === "login") {
            wrapper.style.height = "7rem";
        } else {
            wrapper.style.height = "5rem";
        }

        // Only clear errors if there are errors currently set
        setRegisterErrors((prev) => {
            if (Object.keys(prev).length === 0) return prev;
            return {};
        });

        setLoginErrors((prev) => {
            if (Object.keys(prev).length === 0) return prev;
            return {};
        });
    }, [mode]);

    useEffect(() => {
        if (mode !== "login") {
            loginFormRef.current?.reset();
        }
        if (mode !== "register") {
            registerFormRef.current?.reset();
            setRegisterErrors({});
        }
    }, [mode]);

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

    const reset = () => {
        setMode("idle");
        setRegisterErrors({});
    };

    const fade = (visible: boolean) =>
        visible ? "opacity-100 pointer-events-auto" : "opacity-0 pointer-events-none";

    const validateEmailAndPassword = (email: string, password: string): ValidationErrors => {
        const errors: ValidationErrors = {};

        if (!email) {
            errors.email = "Email is required";
        } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) {
            errors.email = "Email is not valid";
        }

        if (!password) {
            errors.password = "Password is required";
        } else if (!passwordRegex.test(password)) {
            errors.password = passwordErrorMsg;
        }

        return errors;
    };


    // HANDLERS ---
    const handleLogin = async (e: React.FormEvent<HTMLFormElement>) => {
        e.preventDefault();
        const formData = new FormData(e.currentTarget);
        const email = formData.get("email") as string;
        const password = formData.get("password") as string;

        const errors = validateEmailAndPassword(email, password);
        setLoginErrors(errors);
        if (Object.keys(errors).length > 0) return;

        try {
            const loginDto: AuthLoginDto = { email, password };
            const response = await authClient.login(loginDto);
            const { jwt } = response;

            localStorage.setItem("jwt", jwt);
            setJwt(jwt);

            const { Id } = JSON.parse(atob(jwt.split(".")[1]));
            setUserId(Id);

            onLogin?.();
        } catch (err: any) {
            console.error("Login error", err);
            setLoginErrors({
                email: " ",
                password: "Incorrect email or password.",
            });
        }
    };

    useEffect(() => {
        if (!jwt) return; // skip if JWT not available

        try {
            const { Id } = JSON.parse(atob(jwt.split(".")[1]));
            subscribe(`alerts-${Id}`).then();
        } catch (err) {
            console.error("Subscription error", err);
        }
    }, [jwt, subscribe]);

    const handleRegister = async (e: React.FormEvent<HTMLFormElement>) => {
        e.preventDefault();
        const formData = new FormData(e.currentTarget);

        const firstName = formData.get("firstName")?.toString().trim() || "";
        const lastName = formData.get("lastName")?.toString().trim() || "";
        const email = formData.get("email")?.toString().trim() || "";
        const birthdayRaw = formData.get("birthday") as string;
        const country = formData.get("country")?.toString().trim() || "";
        const password = formData.get("password")?.toString().trim() || "";
        const confirmPassword = formData.get("confirmPassword")?.toString().trim() || "";

        const errors: RegisterErrors = {};  // declare errors **first**

        const emailPasswordErrors = validateEmailAndPassword(email, password);
        Object.assign(errors, emailPasswordErrors);  // merge errors from that function

        // FRONTEND VALIDATION Should match backend validation
        if (!firstName) {
            errors.firstName = "First name is required";
        } else if (firstName.length < 2 || firstName.length > 30) {
            errors.firstName = "2–30 characters required.";
        }

        if (!lastName) {
            errors.lastName = "Last name is required";
        } else if (lastName.length < 2 || lastName.length > 30) {
            errors.lastName = "2–30 characters required.";
        }

        if (!birthdayRaw) {
            errors.birthday = "Birthday is required";
        } else {
            const birthday = new Date(birthdayRaw);
            const ageLimit = new Date();
            ageLimit.setFullYear(ageLimit.getFullYear() - 5);
            if (birthday > ageLimit) {
                errors.birthday = "You must be at least 5 years old.";
            }
        }

        if (!country) {
            errors.country = "Country is required";
        }

        if (!confirmPassword) {
            errors.confirmPassword = "Confirm password is required";
        } else if (password !== confirmPassword) {
            errors.confirmPassword = "Passwords do not match";
        }

        setRegisterErrors(errors);
        if (Object.keys(errors).length > 0) return;

        try {
            const birthday = new Date(birthdayRaw);
            await authClient.register({
                firstName,
                lastName,
                email,
                birthday,
                country,
                password,
            } as AuthRegisterDto);
            reset();
            toast.success("Registered successfully! You can now log in.", { id: "register-success" });
        } catch (error: any) {
            console.error("Registration failed:", error);

            let title = "";

            try {
                const parsed = JSON.parse(error.response);
                title = parsed.title || "";
            } catch (e) {
                console.warn("Failed to parse error.response as JSON:", error.response);
            }

            if (title.includes("Email: User with that email already exists")) {
                setRegisterErrors({email: "User with that email already exists"});
            } else {
                toast.error("Registration failed. Try again.", { id: "register-failed" });
            }
        }
    };


    const requiredHint = (msg?: string) => (
        <p
            className={`text-red-500 text-xs text-left transition-opacity duration-200 h-[1.25rem] ${
                msg ? "opacity-100" : "opacity-0"
            }`}
        >
            {msg || " "}
        </p>
    );

    return (
        <main className="relative flex min-h-screen flex-col items-center bg-primary font-display text-base-100 py-50 lg:py-70">
            {/* Header */}
            <h1 className="absolute top-5 text-xl tracking-wider font-bold lg:text-3xl sm:text-3xl text-white">
                Greenhouse Application
            </h1>

            {/* Body */}
            <section
                className="flex w-full max-w-1xl flex-col items-center justify-center gap-10 px-6 md:flex-row md:gap-20">
                <img
                    src={logo}
                    alt="Greenhouse"
                    className={`w-40 sm:w-48 md:w-64 lg:w-96 select-none transition-transform duration-300 ${mobileLogoLift} md:translate-y-0`}
                />

                {/* AUTH COLUMN */}
                <div
                    ref={wrapperRef}
                    className="relative text-white flex w-full max-w-xs flex-col items-center text-center md:max-w-sm overflow-visible transition-[height] duration-300 ease-in-out"
                >
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
                        <span className="mt-1 h-px w-32 bg-white"/>
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
                        <span className="mt-1 h-px w-32 bg-white"/>
                    </div>

                    {/* Login Form */}
                    <form
                        ref={loginFormRef}
                        onSubmit={handleLogin}
                        className={`absolute top-0 w-full space-y-1 transition-opacity duration-300 ${fade(
                            mode === "login",
                        )}`}
                        noValidate
                    >
                        <label className="label py-0 text-white">Email</label>
                        <input
                            name="email"
                            placeholder="Email"
                            type="email"
                            className={`bg-white input input-bordered input-sm w-full text-black ${loginErrors.email ? errorClass : ""}`}
                            required
                        />
                        {requiredHint(loginErrors.email)}

                        <label className="label -mt-8 py-0 text-white">Password</label>
                        <PasswordField
                            name="password"
                            placeholder="Password"
                            className={`bg-white ${loginErrors.password && errorClass}`}
                        />
                        {requiredHint(loginErrors.password)}

                        <button className="btn text-white border-white bg-transparent btn-sm w-full">Login</button>
                    </form>

                    {/* Register Form */}
                    <form
                        ref={registerFormRef}
                        onSubmit={handleRegister}
                        noValidate
                        className={`absolute top-0 w-full -translate-y-24 space-y-1 transition-opacity duration-300 ${
                            mode === "register" ? "opacity-100 pointer-events-auto" : "opacity-0 pointer-events-none"
                        }`}
                    >
                        {/* First / Last name */}
                        <div className="flex gap-2">
                            <div className="flex-1">
                                <label className="label py-0 text-white">First Name</label>
                                <input
                                    name="firstName"
                                    placeholder="First Name"
                                    className={`input input-bordered bg-white input-sm w-full text-black ${
                                        registerErrors.firstName ? errorClass : ""
                                    }`}
                                />
                                {requiredHint(registerErrors.firstName)}
                            </div>

                            <div className="flex-1">
                                <label className="label py-0 text-white">Last Name</label>
                                <input
                                    name="lastName"
                                    placeholder="Last Name"
                                    className={`input input-bordered bg-white input-sm w-full text-black ${
                                        registerErrors.lastName ? errorClass : ""
                                    }`}
                                />
                                {requiredHint(registerErrors.lastName)}
                            </div>
                        </div>

                        {/* Email */}
                        <label className="label py-0 text-white">Email</label>
                        <input
                            name="email"
                            type="email"
                            placeholder="Email"
                            className={`input input-bordered bg-white input-sm w-full text-black ${
                                registerErrors.email ? errorClass : ""
                            }`}
                        />
                        {requiredHint(registerErrors.email)}

                        {/* Birthday / Country */}
                        <div className="flex gap-2">
                            <div className="flex-1">
                                <label className="label py-0 text-white">Birthday</label>
                                <input
                                    name="birthday"
                                    type="date"
                                    className={`input input-bordered bg-white input-sm w-full text-black ${
                                        registerErrors.birthday ? errorClass : ""
                                    }`}
                                />
                                {requiredHint(registerErrors.birthday)}
                            </div>

                            <div className="flex-1 relative overflow-visible">
                                <label className="label py-0 text-white">Country</label>
                                <select
                                    name="country"
                                    className="select select-bordered bg-white select-sm w-full text-black"
                                    required
                                >
                                    <option value="">Select country</option>
                                    {countries.map((country) => (
                                        <option
                                            key={country}
                                            value={country}
                                        >
                                            {country}
                                        </option>
                                    ))}
                                </select>
                                {requiredHint(registerErrors.country)}
                            </div>
                        </div>

                        {/* Password */}
                        <label className="label py-0 text-white">Password</label>
                        <PasswordField
                            name="password"
                            placeholder="Password"
                            className={`bg-white ${registerErrors.password && errorClass}`}
                        />
                        {requiredHint(registerErrors.password)}

                        {/* Confirm password */}
                        <label className="label py-0 text-white">Confirm Password</label>
                        <PasswordField
                            name="confirmPassword"
                            placeholder="Password"
                            className={`bg-white ${registerErrors.confirmPassword && errorClass}`}
                        />
                        {requiredHint(registerErrors.confirmPassword)}

                        <div className="flex gap-2 pt-1">
                            <button type="submit" className="btn bg-transparent text-white border-white btn-xs flex-1">
                                Register
                            </button>
                        </div>
                    </form>
                </div>
            </section>

            {/* Logged-in Notice */
            }
            {
                loggedIn && (
                    <p className="absolute bottom-4 text-xs italic opacity-60">
                        You are now logged in (placeholder)
                    </p>
                )}
        </main>
    );
};

export default AuthScreen;