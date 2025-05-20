import {
    GreenhouseSensorDataAtom,
    JwtAtom,
    RandomUidAtom,
    SelectedDeviceIdAtom,
    SubscribedTopicsAtom,
    UserSettingsAtom,
    useAtom, UserIdAtom
} from "../../import";
import { SignInRoute } from "../../../routeConstants";
import { useNavigate } from "react-router";
import { useUser } from "../../../LoggedInUserData/UserContext.tsx";
import {useTopicManager} from "../../../hooks";

export const useLogout = () => {
    const [, setJwt] = useAtom(JwtAtom);
    const [userId, setUserId] = useAtom(UserIdAtom);
    const [, setSubscribedTopics] = useAtom(SubscribedTopicsAtom);
    const [, setGreenhouseSensorDataAtom] = useAtom(GreenhouseSensorDataAtom);
    const [, setRandomUidAtom] = useAtom(RandomUidAtom);
    const [, setSelectedDeviceIdAtom] = useAtom(SelectedDeviceIdAtom);
    const [, setUserSettings] = useAtom(UserSettingsAtom);
    const { reset } = useUser();
    const navigate = useNavigate();
    const { unsubscribe } = useTopicManager();

    const logout = () => {

        if (userId) {
            unsubscribe(`alerts-${userId}`).then();
        }

        setUserId("")
        setJwt("");
        localStorage.removeItem("jwt");

        setSubscribedTopics([]);
        setGreenhouseSensorDataAtom([]);
        setRandomUidAtom("");
        setSelectedDeviceIdAtom("");
        setUserSettings(null);

        // Clear localStorage
        localStorage.removeItem("jwt");
        localStorage.removeItem("randomUid");
        localStorage.removeItem("theme");

        // Reset HTML theme attribute
        document.documentElement.setAttribute("data-theme", "light");

        reset();
        navigate(SignInRoute);
    };

    return { logout };
};
