import {Route, Routes, useLocation, useNavigate} from "react-router";
import {DashboardRoute, SettingsRoute, SignInRoute} from '../routeConstants.ts';
import {AlertPage, DashboardPage, HistoryPage, MyDevicePage, NotFoundPage} from "../pages"
import {useEffect} from "react";
import {useAtom} from "jotai";
import {JwtAtom} from "./import";
import toast from "react-hot-toast";
import AuthScreen from "../pages/Auth/AuthScreen.tsx";
import {Footer, NavBar} from "./index";
import UserSettings from "../pages/UserSettings/UserSettings.tsx";
import PlantsView from "../pages/PlantsView/PlantsView.tsx";

export default function ApplicationRoutes() {

    const navigate = useNavigate();
    const location = useLocation();
    const [jwt] = useAtom(JwtAtom);

    useEffect(() => {
        if (jwt == undefined || jwt.length < 1) {
            navigate(SignInRoute)
            toast("Please sign in to continue")
        }
    }, [])

    const isAuthScreen = location.pathname === SignInRoute

    return (
        <>
            {!isAuthScreen && <NavBar/>}
            <div className="min-h-screen">
                <Routes>
                    <Route element={<HistoryPage/>} path={"/history"}/>
                    <Route element={<DashboardPage/>} path={DashboardRoute}/>
                    <Route element={<UserSettings/>} path={SettingsRoute}/>
                    <Route element={<AlertPage/>} path={"/alerts"}></Route>
                    <Route element={<MyDevicePage/>} path={"/myDevice"}/>
                    <Route element={<PlantsView/>} path={"/plants"}></Route>
                    <Route path={SignInRoute} element={<AuthScreen onLogin={() => navigate(DashboardRoute)}/>}/>
                    <Route path="/*" element={<NotFoundPage/>}/>
                </Routes>
            </div>
            <Footer/>
        </>
    );
}