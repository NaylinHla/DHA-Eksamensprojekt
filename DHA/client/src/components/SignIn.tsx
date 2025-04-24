import {authClient} from "../apiControllerClients.ts";
import toast from "react-hot-toast";
import {useAtom} from "jotai";
import {JwtAtom} from "../atoms.ts";

export default function SignIn() {

    const [jwt, setJwt] = useAtom(JwtAtom);


    return (<div className="flex flex-col items-center justify-center h-screen">

        {
            (jwt == null || jwt.length < 1) ?
                <button className="btn btn-primary" onClick={() =>
                    authClient.register({email: Math.random() * 123 + "@gmail.com", password: "123456"}).then(r => {
                        toast("welcome!")
                        localStorage.setItem('jwt', r.jwt);
                        setJwt(r.jwt)
                    })}>Click to register as a test user</button>
                :
                <>                <h1 className=" textarea-xl">you are already signed in.</h1>
                    <button className="btn btn-secondary" onClick={() => {
                        localStorage.setItem('jwt', '');
                        setJwt('');
                        toast("You have been signed out")
                    }}>Click here to sign out
                    </button>
                </>
        }

    </div>)
}