import {Route, Routes, useNavigate, useLocation} from "react-router";
import AdminDashboard from "../pages/History/HistoryPage.tsx";
import useInitializeData from "../hooks/useInitializeData.tsx";
import {DashboardRoute, SettingsRoute, SignInRoute} from '../routeConstants.ts';
import useSubscribeToTopics from "../hooks/useSubscribeToTopics.tsx";
import Settings from "./Settings.tsx";
import Dock from "./Dock.tsx";
import SignIn from "./SignIn.tsx";
import {useEffect} from "react";
import {useAtom} from "jotai";
import {JwtAtom} from "./import";
import toast from "react-hot-toast";
import WebsocketConnectionIndicator from "./WebsocketConnectionIndicator.tsx";
import AuthScreen from "../pages/Auth/AuthScreen.tsx";
import {HistoryPage} from "../pages";
import {NavBar} from "./index";

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
                <Route element={<HistoryPage/>} path={"/history"}/>

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