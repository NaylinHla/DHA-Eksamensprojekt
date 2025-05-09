import React, { useState } from "react";
import { TitleTimeHeader } from "../import";

const MarketingPage = () => {
    const [formData, setFormData] = useState({
        feedback: "",
    });

    // Handle form input change
    const handleInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        const { name, value } = e.target;
        setFormData((prevData) => ({
            ...prevData,
            [name]: value,
        }));
    };

    const handleSubmit = (e: React.FormEvent) => {
        e.preventDefault();
        // Handle survey submission logic (e.g., send data to API, show confirmation)
        console.log("Survey Submitted:", formData);
    };

    return (
        <div className="min-h-[calc(100vh-64px)] flex flex-col bg-[--color-background] text-[--color-primary] font-display overflow-hidden">
            <TitleTimeHeader title="Marketing" />

            <main className="flex-1 flex items-center justify-center overflow-hidden px-4 py-8">
                {/* Marketing Content */}
                <section className="max-w-4xl mx-auto text-center">
                    <h1 className="text-3xl font-semibold text-[--color-primary] mb-4">
                        Welcome to Our Marketing Hub!
                    </h1>
                    <p className="text-lg text-gray-700 mb-6">
                        Discover the latest promotions, campaigns, and offers we have for you. Stay tuned for
                        exciting updates and exclusive deals!
                    </p>

                    {/* Call to Action for Newsletter */}
                    <button className="bg-[--color-accent] text-[--color-primary] py-2 px-6 rounded-lg text-lg hover:bg-[--color-accent-dark] transition-all mb-8">
                        Join Our Newsletter
                    </button>

                    {/* New Box: Survey Section */}
                    <div className="bg-[var(--color-surface)] p-6 rounded-lg shadow-lg mb-6">
                        <h2 className="text-2xl font-semibold text-[--color-primary] mb-4">
                            We Need Your Feedback!
                        </h2>
                        <p className="text-gray-700 mb-4">
                            We're considering changing the water icon to a watering can. Help us make this decision by sharing your opinion in our short survey!
                        </p>

                        <form onSubmit={handleSubmit}>
                            <div className="mb-4">
                                <label htmlFor="feedback" className="block text-gray-700 text-lg font-semibold mb-2">
                                    How do you feel about changing water to a watering can?
                                </label>
                                <input
                                    type="radio"
                                    name="feedback"
                                    value="Yes, I like the idea"
                                    onChange={handleInputChange}
                                    className="mr-2"
                                />
                                <label className="mr-4">Yes, I like the idea</label>

                                <input
                                    type="radio"
                                    name="feedback"
                                    value="No, I don't like the idea"
                                    onChange={handleInputChange}
                                    className="mr-2"
                                />
                                <label>No, I don't like the idea</label>
                            </div>

                            <button
                                type="submit"
                                className="btn btn-neutral bg-transparent btn-sm"
                            >
                                Submit Feedback
                            </button>
                        </form>
                    </div>

                    {/* New Box: Other Promotion */}
                    <div className="bg-[var(--color-surface)] p-6 rounded-lg shadow-lg">
                        <h2 className="text-2xl font-semibold text-[--color-primary] mb-4">
                            Special Promotion 2
                        </h2>
                        <p className="text-gray-700 mb-4">
                            Don't miss out on our second limited-time offer! Unlock exclusive benefits with our premium plan and experience the best features.
                        </p>
                        <button className="btn btn-neutral bg-transparent btn-sm">
                            Upgrade Now
                        </button>
                    </div>
                </section>
            </main>
        </div>
    );
};

export default MarketingPage;
