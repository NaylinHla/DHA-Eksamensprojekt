import React from "react";
import { TitleTimeHeader } from "../import";

const AboutPage = () => {
    return (
        <div className="min-h-[calc(100vh-64px)] flex flex-col bg-[--color-background] text-[--color-primary] font-display overflow-hidden">
            <TitleTimeHeader title="About Us" />

            <main className="flex-1 overflow-auto px-4 py-8 max-w-4xl mx-auto">
                <section className="space-y-6">
                    <h2 className="text-2xl font-semibold">Our Mission</h2>
                    <p className="text-base leading-relaxed text-[--color-text]">
                        Greenhouse Application helps individuals and organizations monitor, manage, and optimize their
                        plant environments with real-time data, intuitive dashboards, and proactive alerts.
                    </p>

                    <h2 className="text-2xl font-semibold">Why We Built This</h2>
                    <p className="text-base leading-relaxed text-[--color-text]">
                        Too many growers rely on guesswork. We wanted to provide a data-driven way to ensure healthy plant growth, reduce waste, and empower users with actionable insights.
                    </p>

                    <h2 className="text-2xl font-semibold">Who We Are</h2>
                    <p className="text-base leading-relaxed text-[--color-text]">
                        We're a passionate team of developers, horticulturists, and designers who care about sustainability and smart agriculture.
                    </p>
                </section>
            </main>
        </div>
    );
};

export default AboutPage;
