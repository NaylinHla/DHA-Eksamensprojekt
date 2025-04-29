import {Route, Routes, useNavigate} from "react-router";
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
import {HistoryPage} from "../pages";
import {NavBar} from "./index";

export default function ApplicationRoutes() {
    
    const navigate = useNavigate();
    const [jwt] = useAtom(JwtAtom);
    useInitializeData();
    useSubscribeToTopics();

    useEffect(() => {
        if (jwt == undefined || jwt.length < 1) {
            navigate(SignInRoute)
            toast("Please sign in to continue")
        }
    }, [])
    
    return (<>
        <NavBar/>
            <Routes>
                <Route element={<HistoryPage/>} path={"/history"}/>

                <Route element={<AdminDashboard/>} path={DashboardRoute}/>
                <Route element={<Settings/>} path={SettingsRoute}/>
                <Route element={<SignIn/>} path={SignInRoute}/>
            </Routes>
        <Dock/>
    </>)
}