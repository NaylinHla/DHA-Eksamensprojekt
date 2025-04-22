import {useLocation, useNavigate} from "react-router";
import {DashboardRoute, SettingsRoute, SignInRoute} from "../routeConstants.ts";

export default function Dock() {

    const navigate = useNavigate();
    const location = useLocation()

    return (<>
        <div className="dock">
            <button className={location.pathname == DashboardRoute ? 'dock-active' : 'dock'}
                    onClick={() => navigate(DashboardRoute)}>
                <h1>🏠</h1>

                <span className="dock-label">Home</span>
            </button>


            <button className={location.pathname == SettingsRoute ? 'dock-active' : 'dock'}
                    onClick={() => navigate(SettingsRoute)}>
                <h1>⚙️</h1>
                <span className="dock-label">Settings</span>
            </button>
            <button className={location.pathname == SignInRoute ? 'dock-active' : 'dock'}
                    onClick={() => navigate(SignInRoute)}>
                <h1>🔐</h1>

                <span className="dock-label">Sign In</span>
            </button>

        </div>


    </>);
}