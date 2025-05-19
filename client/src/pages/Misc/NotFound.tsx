import {useNavigate} from "react-router";

function NotFound() {
    const navigate = useNavigate();

    const navigateToHome = () => {
        navigate("/");
    };

    return (
        <section className="flex items-center h-full p-16 sm:mt-10 text-primary-text">
            <div className="container flex flex-col items-center justify-center px-5 mx-auto my-8">
                <div className="max-w-md text-center">

                    <h2 className="mb-8 font-extrabold text-9xl">
                        <span className="sr-only">Error</span>404
                    </h2>

                    <p className="text-2xl font-semibold md:text-3xl">Sorry, we couldn’t find this page.</p>
                    <p className="mt-4 mb-8">But don’t worry, you can explore many other plants in our web
                        application.</p>

                    <button
                        rel="noopener noreferrer"
                        onClick={navigateToHome}
                        className="px-8 py-3 text-sm sm:text-base font-semibold btn btn-neutral bg-transparent btn-sm"
                    >
                        Return back
                    </button>
                </div>
            </div>
        </section>
    );
}

export default NotFound;
