import {createRoot} from 'react-dom/client'
import App from "./components/App.tsx";
import {Toaster} from "react-hot-toast";
import './style.css';
import {BrowserRouter} from "react-router";

document.documentElement.setAttribute("data-theme", "light");
createRoot(document.getElementById('root')!).render(
    <BrowserRouter>
        <App/>
        <Toaster/>
    </BrowserRouter>,
)
