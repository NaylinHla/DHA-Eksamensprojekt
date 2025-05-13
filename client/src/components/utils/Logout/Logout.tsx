import {
    GreenhouseSensorDataAtom,
    JwtAtom,
    RandomUidAtom,
    SelectedDeviceIdAtom,
    SubscribedTopicsAtom,
    UserSettingsAtom, // ⬅ Add this
    useAtom
} from "../../import";
import { SignInRoute } from "../../../routeConstants";
import { useNavigate } from "react-router";
import { useUser } from "../../../UserContext.tsx";

export const useLogout = () => {
    const [, setJwt] = useAtom(JwtAtom);
    const [, setSubscribedTopics] = useAtom(SubscribedTopicsAtom);
    const [, setGreenhouseSensorDataAtom] = useAtom(GreenhouseSensorDataAtom);
    const [, setRandomUidAtom] = useAtom(RandomUidAtom);
    const [, setSelectedDeviceIdAtom] = useAtom(SelectedDeviceIdAtom);
    const [, setUserSettings] = useAtom(UserSettingsAtom); // ✅ Add this
    const { reset } = useUser();
    const navigate = useNavigate();

    const logout = () => {

        setJwt(null);
        localStorage.removeItem("jwt");

        setSubscribedTopics([]);
        setGreenhouseSensorDataAtom([]);
        setRandomUidAtom("");
        setSelectedDeviceIdAtom("");
        setUserSettings(null); // ✅ Reset settings atom

        // Clear localStorage
        localStorage.removeItem("jwt");
        localStorage.removeItem("randomUid");
        localStorage.removeItem("theme"); // ✅ Reset theme override

        // Reset HTML theme attribute
        document.documentElement.setAttribute("data-theme", "light");

        reset();
        navigate(SignInRoute);
    };

    return { logout };
};
