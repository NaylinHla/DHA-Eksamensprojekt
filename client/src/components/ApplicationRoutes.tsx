import {Route, Routes, useNavigate, useLocation} from "react-router";
import AdminDashboard from "./Dashboard.tsx";
import useInitializeData from "../hooks/useInitializeData.tsx";
import {DashboardRoute, SettingsRoute, SignInRoute} from '../routeConstants.ts';
import useSubscribeToTopics from "../hooks/useSubscribeToTopics.tsx";
import Settings from "./Settings.tsx";
import Dock from "./Dock.tsx";
import SignIn from "./SignIn.tsx";
import {useEffect} from "react";
import {useAtom} from "jotai";
import {JwtAtom} from "../atoms.ts";
import toast from "react-hot-toast";
import WebsocketConnectionIndicator from "./WebsocketConnectionIndicator.tsx";
import NavBar from "./templates/Navbar.tsx";
import AuthScreen from "./pages/AuthScreen.tsx";

export default function ApplicationRoutes() {
    
    const navigate = useNavigate();
    const location = useLocation();
    const [jwt] = useAtom(JwtAtom);
    useInitializeData();
    useSubscribeToTopics();

    useEffect(() => {
        if (jwt == undefined || jwt.length < 1) {
            navigate(SignInRoute)
            toast("Please sign in to continue")
        }
    }, [])

    const isAuthScreen = location.pathname === SignInRoute
    
    return (<>
        {!isAuthScreen && <NavBar/>}
            <Routes>
                <Route element={<AdminDashboard/>} path={DashboardRoute}/>
                <Route element={<Settings/>} path={SettingsRoute}/>

                <Route
                    path={SignInRoute}
                    element={
                        <AuthScreen onLogin={() => navigate(DashboardRoute)} />
                    }
                />
            </Routes>
        {!isAuthScreen && <Dock />}
    </>)
}