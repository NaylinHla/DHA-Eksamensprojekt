import {Route, Routes, useLocation, useNavigate} from "react-router";
import {DashboardRoute, SettingsRoute, SignInRoute} from '../routeConstants.ts';
import {AuthScreen, AlertPage, WeatherView, PlantsView, DashboardPage, HistoryPage, MyDevicePage, NotFoundPage, UserSettings} from "../pages"
import {useEffect} from "react";
import {useAtom} from "jotai";
import {JwtAtom} from "./import";
import toast from "react-hot-toast";
import {Footer, NavBar} from "./index";

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
                    <Route path="/weather" element={<WeatherView />} />
                    <Route path={SignInRoute} element={<AuthScreen onLogin={() => navigate(DashboardRoute)}/>}/>
                    <Route path="/*" element={<NotFoundPage/>}/>
                </Routes>
            </div>
            <Footer/>
        </>
    );
}