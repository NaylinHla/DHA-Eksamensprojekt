import React from "react";
import { Navigate } from "react-router-dom";
import { isJwtValid } from "../utils/jwtValidation/jwt.ts";

type Props = { children: React.ReactNode };

const ProtectedRoute: React.FC<Props> = ({ children }) => {
    const token = localStorage.getItem("jwt");

    if (!isJwtValid(token)) {
        return <Navigate to="/signin" replace />;
    }

    return <>{children}</>;
};

export default ProtectedRoute;
