import React from "react";
import { TitleTimeHeader } from "../import";

const CareerPage = () => {
    return (
        <div className="min-h-[calc(100vh-64px)] flex flex-col bg-[--color-background] text-[--color-primary] font-display overflow-hidden">
            <TitleTimeHeader title="Careers" />

            <main className="flex-1 overflow-auto px-4 py-8 max-w-4xl mx-auto">
                <section className="space-y-6">
                    <h2 className="text-2xl font-semibold">Join the Greenhouse Team</h2>
                    <p className="text-base leading-relaxed text-[--color-text]">
                        We're building tools that make plant care smarter, simpler, and more sustainable. Want to be part of that mission? We're always on the lookout for curious, driven, and creative people.
                    </p>

                    <h2 className="text-2xl font-semibold">Open Roles</h2>
                    <ul className="list-disc pl-6 space-y-2 text-[--color-text]">
                        <li>🌿 Frontend Developer (React, Tailwind)</li>
                        <li>🌱 Backend Engineer (Node, GraphQL)</li>
                        <li>🌾 Plant Data Analyst</li>
                        <li>🌼 Marketing & Growth Strategist</li>
                    </ul>

                    <h2 className="text-2xl font-semibold">Don’t see your role?</h2>
                    <p className="text-base leading-relaxed text-[--color-text]">
                        We’d still love to hear from you. Drop us an email at{" "}
                        <a
                            href="mailto:join@meetyourplants.site"
                            className="text-[--color-accent] underline hover:text-[--color-accent-hover]"
                        >
                            join@meetyourplants.site
                        </a>{" "}
                        and tell us what you’d bring to the team.
                    </p>
                </section>
            </main>
        </div>
    );
};

export default CareerPage;
