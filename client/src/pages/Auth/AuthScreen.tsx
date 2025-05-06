import {useEffect, useLayoutEffect, useRef, useState} from "react";
import logo from "../../assets/Favicon/favicon.svg";
import {
    AuthClient,
    AuthLoginDto,
    AuthRegisterDto
} from "../../generated-client.ts";
import {PasswordField} from "../../components/utils/PasswordField/PasswordField.tsx";
import {JwtAtom, useAtom} from "../../components/import";
import toast from "react-hot-toast";

type AuthScreenProps = {
    onLogin?: () => void;
};

interface RegisterErrors {
    firstName?: boolean;
    lastName?: boolean;
    email?: boolean;
    birthday?: boolean;
    country?: boolean;
    password?: boolean;
    confirmPassword?: "required" | "mismatch";
}

const authClient = new AuthClient("http://localhost:5000");

const AuthScreen: React.FC<AuthScreenProps> = ({ onLogin }) => {
    const [mode, setMode] = useState<"idle" | "login" | "register">("idle");
    const [loggedIn, setLoggedIn] = useState(false);
    const [, setJwt] = useAtom(JwtAtom);

    const [registerErrors, setRegisterErrors] = useState<RegisterErrors>({});
    const errorClass = "border-red-500 focus:border-red-600";
    const wrapperRef   = useRef<HTMLDivElement>(null);
    const registerFormRef  = useRef<HTMLFormElement>(null);
    const loginFormRef     = useRef<HTMLFormElement>(null);
    
    // ANIMATION ---
    useLayoutEffect(() => {
        const wrapper  = wrapperRef.current;
        const register = registerFormRef.current;

        if (!wrapper) return;

        if (mode === "register" && register) {
            wrapper.style.height = "7rem";
        } else if (mode === "login") {
            wrapper.style.height = "7rem";
        } else {
            wrapper.style.height = "5rem";
        }
    }, [mode, registerErrors]);

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

    
    // HANDLERS ---
    const handleLogin = async (e: React.FormEvent<HTMLFormElement>) => {
        e.preventDefault();
        const formData = new FormData(e.currentTarget);
        const email = formData.get("email") as string;
        const password = formData.get("password") as string;

        try {
            const loginDto: AuthLoginDto = { email, password };
            const response = await authClient.login(loginDto);
            const { jwt } = response;
            setJwt(jwt);
            localStorage.setItem("jwt", jwt);
            setLoggedIn(true);
            onLogin?.();
        } catch (error) {
            console.error("Login failed", error);
            toast.error("Login failed. Please check your credentials.");
        }
    };

    const handleRegister = async (e: React.FormEvent<HTMLFormElement>) => {
        e.preventDefault();
        const formData = new FormData(e.currentTarget);
        const firstName = (formData.get("firstName") as string)?.trim();
        const lastName = (formData.get("lastName") as string)?.trim();
        const email = (formData.get("email") as string)?.trim();
        const birthdayRaw = formData.get("birthday") as string;
        const country = (formData.get("country") as string)?.trim();
        const password = (formData.get("password") as string)?.trim();
        const confirmPassword = (formData.get("confirmPassword") as string)?.trim();

        const errors: RegisterErrors = {};
        if (!firstName) errors.firstName = true;
        if (!lastName) errors.lastName = true;
        if (!email) errors.email = true;
        if (!birthdayRaw) errors.birthday = true;
        if (!country) errors.country = true;
        if (!password) errors.password = true;
        if (!confirmPassword) errors.confirmPassword = "required";
        if (
            password &&
            confirmPassword &&
            password !== confirmPassword
        ) {
            errors.confirmPassword = "mismatch";
        }

        setRegisterErrors(errors);
        if (Object.keys(errors).length) return;

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
            toast.success("Registered successfully! You can now log in.");
        } catch (error) {
            console.error("Registration failed", error);
            toast.error("Registration failed. Try again.");
        }
    };

    const requiredHint = (flag?: boolean | string) => (
        <p
            className={`text-white text-xs text-left ${
                flag ? "block mt-1" : "hidden"
            }`}
        >
            {flag === "mismatch" ? "*Mismatch" : "*Required"}
        </p>
    );

    return (
        <main className="relative flex min-h-screen flex-col items-center bg-primary font-display text-base-100 py-50">
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
                        className={`absolute top-0 w-full space-y-2 transition-opacity duration-300 ${fade(
                            mode === "login",
                        )}`}
                    >
                        <label className="label py-0 text-white">Email</label>
                        <input
                            name="email"
                            type="email"
                            placeholder="Email"
                            className="bg-white input input-bordered input-sm w-full text-black"
                            required
                        />
                        <label className="label py-0 text-white">Password</label>
                        <PasswordField
                            name="password"
                            placeholder="Password"
                            required
                            className={"bg-white"}
                        />
                        <button className="btn text-white border-white bg-transparent btn-sm w-full">Login</button>
                    </form>

                    {/* Register Form */}
                    <form
                        ref={registerFormRef}
                        onSubmit={handleRegister}
                        noValidate
                        className={`absolute top-0 w-full -translate-y-24 space-y-2 transition-opacity duration-300 ${
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
                                        registerErrors.firstName && errorClass
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
                                        registerErrors.lastName && errorClass
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
                                registerErrors.email && errorClass
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
                                        registerErrors.birthday && errorClass
                                    }`}
                                />
                                {requiredHint(registerErrors.birthday)}
                            </div>

                            <div className="flex-1">
                                <label className="label py-0 text-white">Country</label>
                                <input
                                    name="country"
                                    placeholder="Country"
                                    className={`input input-bordered bg-white input-sm w-full text-black ${
                                        registerErrors.country && errorClass
                                    }`}
                                />
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