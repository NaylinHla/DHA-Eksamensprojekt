import {Navigate, Outlet, Route, Routes, useLocation, useNavigate} from "react-router";
import {DashboardRoute, SettingsRoute, SignInRoute} from '../routeConstants.ts';
import {AuthScreen, WeatherView, PlantsView, UserSettings} from "../pages"
import toast from "react-hot-toast";
import React from "react";
import {Footer, NavBar} from "./index";
import {
    AboutPage,
    AlertPage,
    DashboardPage,
    HistoryPage,
    MyDevicePage,
    NotFoundPage,
    AdvertisementPage,
    CareerPage,
    CookiesPage,
    MarketingPage,
    PrivacyPage,
    TermsPage,
    JwtAtom,
    useAtom,

} from "./import";

export default function ApplicationRoutes() {

    const navigate = useNavigate();
    const location = useLocation();
    const isAuthScreen = location.pathname === SignInRoute;

    function RequireAuth() {
        const [jwt] = useAtom(JwtAtom);

        if (!jwt || jwt.length < 1) {
            toast("Please sign in to continue");
            return <Navigate to={SignInRoute} replace/>;
        }

        return <Outlet/>;
    }

    return (
        <>
            {!isAuthScreen && <NavBar/>}
            <div className="min-h-screen">
                <Routes>
                    {/* Public Routes */}
                    <Route path={SignInRoute} element={<AuthScreen onLogin={() => navigate(DashboardRoute)}/>}/>
                    <Route path="/about" element={<AboutPage/>}/>
                    <Route path="/career" element={<CareerPage/>}/>
                    <Route path="/advertisement" element={<AdvertisementPage/>}/>
                    <Route path="/marketing" element={<MarketingPage/>}/>
                    <Route path="/terms" element={<TermsPage/>}/>
                    <Route path="/privacy" element={<PrivacyPage/>}/>
                    <Route path="/cookies" element={<CookiesPage/>}/>

                    {/* Grouped Protected Routes */}
                    <Route element={<RequireAuth/>}>
                        <Route path={DashboardRoute} element={<DashboardPage/>}/>
                        <Route path="/history" element={<HistoryPage/>}/>
                        <Route path={SettingsRoute} element={<UserSettings/>}/>
                        <Route path="/alerts" element={<AlertPage/>}/>
                        <Route path="/myDevice" element={<MyDevicePage/>}/>
                        <Route path="/plants" element={<PlantsView/>}/>
                        <Route path="/weather" element={<WeatherView />} />
                    </Route>

                    {/* Catch-all */}
                    <Route path="/*" element={<NotFoundPage/>}/>
                </Routes>
            </div>
            <Footer/>
        </>
    );
}
