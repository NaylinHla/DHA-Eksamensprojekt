import React from "react";
import { TitleTimeHeader } from "../import";

const CookiesPage = () => {
    return (
        <div className="min-h-[calc(100vh-64px)] flex flex-col bg-[--color-background] text-[--color-primary] font-display overflow-hidden">
            <TitleTimeHeader title="Cookie Policy" />

            <main className="flex-1 max-w-4xl mx-auto px-4 py-8 overflow-y-auto">
                <section>
                    <h1 className="text-3xl font-semibold text-[--color-primary] mb-6">
                        Cookie Policy
                    </h1>

                    <p className="text-lg text-[--color-primary] mb-6">
                        This Cookie Policy explains how we use cookies and similar technologies to recognize you when you visit our website. It explains what these technologies are, why we use them, and your rights to control our use of them.
                    </p>

                    <h2 className="text-2xl font-semibold text-[--color-primary] mb-4">
                        1. What Are Cookies?
                    </h2>
                    <p className="text-[--color-primary] mb-6">
                        Cookies are small data files that are placed on your device or computer when you visit a website. They are commonly used to make websites work more efficiently, as well as to provide information to the owners of the site.
                    </p>

                    <h2 className="text-2xl font-semibold text-[--color-primary] mb-4">
                        2. Why We Use Cookies
                    </h2>
                    <p className="text-[--color-primary] mb-6">
                        We use cookies for several reasons:
                    </p>
                    <ul className="list-disc ml-6 mb-6">
                        <li>To personalize your experience on our website</li>
                        <li>To remember your preferences and settings</li>
                        <li>To track website performance and analyze usage patterns</li>
                        <li>To enhance security and prevent fraudulent activity</li>
                    </ul>

                    <h2 className="text-2xl font-semibold text-[--color-primary] mb-4">
                        3. Types of Cookies We Use
                    </h2>
                    <p className="text-[--color-primary] mb-6">
                        There are different types of cookies we use on our website:
                    </p>
                    <ul className="list-disc ml-6 mb-6">
                        <li><strong>Essential Cookies:</strong> These cookies are necessary for the operation of the website and cannot be turned off.</li>
                        <li><strong>Performance and Analytics Cookies:</strong> These cookies allow us to analyze how visitors use the website to improve user experience.</li>
                        <li><strong>Advertising and Marketing Cookies:</strong> These cookies are used to track your browsing habits and deliver relevant ads.</li>
                        <li><strong>Functional Cookies:</strong> These cookies help with personalized features, such as remembering your login information or language preferences.</li>
                    </ul>

                    <h2 className="text-2xl font-semibold text-[--color-primary] mb-4">
                        4. How to Control Cookies
                    </h2>
                    <p className="text-[--color-primary] mb-6">
                        You have the right to control and manage your cookie preferences. You can:
                    </p>
                    <ul className="list-disc ml-6 mb-6">
                        <li>Change your browser settings to block or delete cookies</li>
                        <li>Use incognito or private browsing modes to limit cookie usage during your visit</li>
                        <li>Manage cookie preferences directly from our website if we provide an option</li>
                    </ul>

                    <h2 className="text-2xl font-semibold text-[--color-primary] mb-4">
                        5. Changes to This Cookie Policy
                    </h2>
                    <p className="text-[--color-primary] mb-6">
                        We may update this Cookie Policy from time to time. If we make significant changes, we will notify you via a prominent notice on our website.
                    </p>

                    <h2 className="text-2xl font-semibold text-[--color-primary] mb-4">
                        6. Contact Us
                    </h2>
                    <p className="text-[--color-primary] mb-6">
                        If you have any questions about our Cookie Policy or how we use cookies, feel free to contact us at:
                    </p>
                    <p className="mt-2 font-mono text-sm text-[--color-accent]">
                        privacy@meetyourplants.site
                    </p>
                </section>
            </main>
        </div>
    );
};

export default CookiesPage;
