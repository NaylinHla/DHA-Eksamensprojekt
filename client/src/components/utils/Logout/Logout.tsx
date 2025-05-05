import {
    GreenhouseSensorDataAtom,
    JwtAtom,
    RandomUidAtom,
    SelectedDeviceIdAtom,
    SubscribedTopicsAtom,
    useAtom
} from "../../import";
import {SignInRoute} from "../../../routeConstants";
import {useNavigate} from "react-router";

export const useLogout = () => {
    const [, setJwt] = useAtom(JwtAtom);
    const [, setSubscribedTopics] = useAtom(SubscribedTopicsAtom);
    const [, setGreenhouseSensorDataAtom] = useAtom(GreenhouseSensorDataAtom);
    const [, setRandomUidAtom] = useAtom(RandomUidAtom);
    const [, setSelectedDeviceIdAtom] = useAtom(SelectedDeviceIdAtom);
    const navigate = useNavigate();

    const logout = () => {
        // Reset JWT and subscribed topics
        setJwt("");
        setSubscribedTopics([]);
        setGreenhouseSensorDataAtom([]);
        setRandomUidAtom("");
        setSelectedDeviceIdAtom("");

        // Remove the JWT and randomUid from localStorage
        localStorage.removeItem("jwt");
        localStorage.removeItem("randomUid");

        navigate(SignInRoute);
    };
    return {logout};
};
