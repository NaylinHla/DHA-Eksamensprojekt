import { JwtAtom } from "../../../atoms/atoms";
import { Setter } from "jotai";
import { SignInRoute } from "../../../routeConstants";

export const Logout = (
    setJwt: Setter,
    navigate: (path: string) => void,
) => {
    setJwt('');
    localStorage.removeItem("jwt");
    navigate(SignInRoute);
};
