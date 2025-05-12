import React from "react";
import { TitleTimeHeader } from "../import";

const TermsPage = () => {
    return (
        <div className="min-h-[calc(100vh-64px)] flex flex-col bg-[--color-background] text-[--color-primary] font-display overflow-hidden">
            <TitleTimeHeader title="Terms & Conditions" />

            <main className="flex-1 overflow-auto px-4 py-8 max-w-4xl mx-auto space-y-6">
                <section>
                    <h2 className="text-xl font-semibold mb-2">1. Introduction</h2>
                    <p className="text-[--color-text] leading-relaxed">
                        Welcome to Greenhouse Application. By using this app, you agree to comply with these terms and any applicable laws. If you disagree with any part, please do not use our service.
                    </p>
                </section>

                <section>
                    <h2 className="text-xl font-semibold mb-2">2. Usage</h2>
                    <p className="text-[--color-text] leading-relaxed">
                        You may not misuse the application or interfere with its normal operation. Accounts found engaging in harmful behavior may be suspended or terminated.
                    </p>
                </section>

                <section>
                    <h2 className="text-xl font-semibold mb-2">3. Intellectual Property</h2>
                    <p className="text-[--color-text] leading-relaxed">
                        All content, branding, and assets remain the property of Greenhouse Application. You agree not to reproduce, duplicate, or exploit our material without permission.
                    </p>
                </section>

                <section>
                    <h2 className="text-xl font-semibold mb-2">4. Data</h2>
                    <p className="text-[--color-text] leading-relaxed">
                        We respect your privacy. By using the app, you agree to our data practices as described in our Privacy Policy.
                    </p>
                </section>

                <section>
                    <h2 className="text-xl font-semibold mb-2">5. Changes</h2>
                    <p className="text-[--color-text] leading-relaxed">
                        We may update these terms occasionally. Continued use of the app after changes constitutes acceptance of those changes.
                    </p>
                </section>

                <section>
                    <h2 className="text-xl font-semibold mb-2">6. Contact</h2>
                    <p className="text-[--color-text] leading-relaxed">
                        For any questions, reach out to us at{" "}
                        <a
                            href="mailto:support@meetyourplants.site"
                            className="text-[--color-accent] underline hover:text-[--color-accent-hover]"
                        >
                            support@meetyourplants.site
                        </a>.
                    </p>
                </section>
            </main>
        </div>
    );
};

export default TermsPage;
