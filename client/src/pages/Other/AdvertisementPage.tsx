import React from "react";
import { TitleTimeHeader } from "../import";
import {ContactUsRoute} from "../../routeConstants";

const AdvertisementPage = () => {
    return (
        <div className="min-h-[calc(100vh-64px)] flex flex-col bg-[--color-background] text-[--color-primary] font-display overflow-hidden">
            <TitleTimeHeader title="Advertisement" />

            <main className="flex-1 flex items-center justify-center overflow-hidden px-4 py-8">
                {/* Advertisement Content */}
                <section className="max-w-4xl mx-auto text-center">
                    <h1 className="text-3xl font-semibold text-[--color-primary] mb-4">
                        Premium is on Sale – Don't Miss Out!
                    </h1>
                    <p className="text-lg mb-6">
                        Our Premium plan is now available at a discounted price for a limited time! Upgrade today and enjoy all the exclusive features we offer.
                    </p>

                    {/* Premium Offer Section */}
                    <div className="bg-[var(--color-surface)] p-6 rounded-lg shadow-lg mb-6">
                        <h2 className="text-2xl font-semibold text-[--color-primary] mb-2">
                            Get Premium Now at 25% Off!
                        </h2>
                        <p className="mb-4">
                            This is your chance to upgrade to Premium for a reduced price! Gain access to exclusive content, advanced features, and personalized services.
                        </p>
                        <button className="bg-[--color-accent] text-white py-2 px-6 rounded-lg text-lg hover:bg-[--color-accent-dark] transition-all">
                            Upgrade Now
                        </button>
                    </div>

                    {/* Contact CTA */}
                    <p className="text-lg mb-4">
                        Have any questions or need more details? We're here to help!
                    </p>
                    <a
                        href={ContactUsRoute}
                        className="bg-[--color-accent] text-[--color-primary] py-2 px-6 rounded-lg text-lg hover:bg-[--color-accent-dark] transition-all"
                    >
                        Contact Us
                    </a>
                </section>
            </main>
        </div>
    );
};

export default AdvertisementPage;
