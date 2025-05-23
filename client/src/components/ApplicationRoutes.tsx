import {Outlet, Route, Routes, useLocation, useNavigate,} from "react-router";
import {
    AboutRoute,
    AdvertisementRoute,
    AlertsRoute,
    CareerRoute,
    ContactUsRoute,
    CookiesRoute,
    DashboardRoute,
    HistoryRoute,
    MarketingRoute,
    MyAlertConditionRoute,
    MyDeviceOverviewInRoute,
    NotFoundRoute,
    PlantRoute,
    PrivacyRoute,
    ProfileRoute,
    SignInRoute,
    TermsRoute,
    WeatherRoute,
} from "../routeConstants.ts";
import {AlertConditionsPage, AuthScreen, ContactUsPage, PlantsView, UserSettingsPage, WeatherView,} from "../pages";
import React, {useEffect, useState} from "react";
import {Footer, NavBar} from "./index";
import {
    AboutPage,
    AdvertisementPage,
    AlertPage,
    CareerPage,
    CookiesPage,
    DashboardPage,
    HistoryPage,
    JwtAtom,
    MarketingPage,
    MyDevicePage,
    NotFoundPage,
    PrivacyPage,
    TermsPage,
    useAtom,
} from "./import";
import {useGlobalAlertToasts} from "../hooks/useGlobalAlertToasts";

export default function ApplicationRoutes() {
    const navigate = useNavigate();
    const location = useLocation();
    const isAuthScreen = location.pathname === SignInRoute;

    function RequireAuth() {
        const [jwt] = useAtom(JwtAtom);
        const navigate = useNavigate();

        const [checked, setChecked] = useState(false);

        useEffect(() => {
            setChecked(true);

            if (!jwt || jwt.length < 10) {
                localStorage.removeItem('jwt');
                navigate(SignInRoute, {replace: true});
            }
        }, [jwt, navigate]);

        if (!checked) return null;

        return <Outlet/>;
    }

    useGlobalAlertToasts();

    return (
        <>
            {!isAuthScreen && <NavBar/>}
            <div className="min-h-screen">
                <Routes>
                    {/* Public Routes */}
                    <Route
                        path={SignInRoute}
                        element={<AuthScreen onLogin={() => navigate(DashboardRoute)}/>}
                    />
                    <Route path={AboutRoute} element={<AboutPage/>}/>
                    <Route path={ContactUsRoute} element={<ContactUsPage/>}/>
                    <Route path={CareerRoute} element={<CareerPage/>}/>
                    <Route path={AdvertisementRoute} element={<AdvertisementPage/>}/>
                    <Route path={MarketingRoute} element={<MarketingPage/>}/>
                    <Route path={TermsRoute} element={<TermsPage/>}/>
                    <Route path={PrivacyRoute} element={<PrivacyPage/>}/>
                    <Route path={CookiesRoute} element={<CookiesPage/>}/>

                    {/* Protected Routes */}
                    <Route element={<RequireAuth/>}>
                        <Route path={DashboardRoute} element={<DashboardPage/>}/>
                        <Route path={HistoryRoute} element={<HistoryPage/>}/>
                        <Route path={ProfileRoute} element={<UserSettingsPage/>}/>
                        <Route path={AlertsRoute} element={<AlertPage/>}/>
                        <Route path={MyDeviceOverviewInRoute} element={<MyDevicePage/>}/>
                        <Route path={MyAlertConditionRoute} element={<AlertConditionsPage/>}/>
                        <Route path={PlantRoute} element={<PlantsView/>}/>
                        <Route path={WeatherRoute} element={<WeatherView/>}/>
                    </Route>

                    {/* Catch-all */}
                    <Route path={NotFoundRoute} element={<NotFoundPage/>}/>
                </Routes>
            </div>
            <Footer/>
        </>
    );
}
