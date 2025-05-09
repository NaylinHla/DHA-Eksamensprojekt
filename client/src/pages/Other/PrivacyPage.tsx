import React, { useState } from "react";
import { ChevronDown, ChevronUp } from "lucide-react";

const Section = ({
                     title,
                     children,
                 }: {
    title: string;
    children: React.ReactNode;
}) => {
    const [open, setOpen] = useState(false);

    return (
        <div className="border-b border-gray-300 py-4">
            <button
                onClick={() => setOpen(!open)}
                className="w-full text-left flex justify-between items-center text-lg font-semibold"
            >
                {title}
                {open ? <ChevronUp size={20} /> : <ChevronDown size={20} />}
            </button>
            <div
                className={`mt-3 text-sm text-[--color-primary] overflow-hidden transition-all duration-300 ease-in-out ${
                    open ? "max-h-[1000px] opacity-100" : "max-h-0 opacity-0"
                }`}
                style={{
                    maxHeight: open ? "1000px" : "0", // Adjust maxHeight based on content
                    transition: "max-height 0.3s ease-in-out, opacity 0.3s ease-in-out",
                }}
            >
                {children}
            </div>
        </div>
    );
};

export default function PrivacyPolicyPage() {
    return (
        <div className="min-h-[calc(100vh-64px)] flex flex-col bg-[--color-background] text-[--color-primary] font-display overflow-hidden">
            <main className="flex-1 max-w-3xl mx-auto px-4 py-8 overflow-y-auto">
                <p className="mb-6 text-[--color-primary] text-sm">
                    Effective Date: May 1, 2025
                </p>

                <Section title="1. Introduction">
                    <p>
                        Welcome to Meet Your Plants. This Privacy Policy outlines how we collect,
                        use, and protect your information when you interact with our website,
                        services, and products. We value your privacy and are committed to
                        safeguarding your data.
                    </p>
                </Section>

                <Section title="2. Information We Collect">
                    <ul className="list-disc ml-6 space-y-2">
                        <li>Personal identifiers (e.g., name, email address)</li>
                        <li>Account login credentials</li>
                        <li>Usage data, such as pages visited and interaction history</li>
                        <li>Device and browser metadata</li>
                        <li>Location data (with your consent)</li>
                    </ul>
                </Section>

                <Section title="3. How We Use Your Information">
                    <p>
                        We use the information collected to:
                    </p>
                    <ul className="list-disc ml-6 space-y-2">
                        <li>Provide and improve our services</li>
                        <li>Personalize content and recommendations</li>
                        <li>Send service updates and promotional material</li>
                        <li>Analyze user behavior for product development</li>
                        <li>Ensure platform security and fraud prevention</li>
                    </ul>
                </Section>

                <Section title="4. Cookies and Tracking Technologies">
                    <p>
                        We use cookies and similar technologies to track user activity,
                        personalize experiences, and measure the effectiveness of our
                        communications. You can manage your cookie preferences through your
                        browser settings.
                    </p>
                </Section>

                <Section title="5. Sharing Your Data">
                    <p>
                        We do not sell your data. We may share your information with trusted
                        service providers and partners under strict confidentiality agreements to
                        help us operate and improve our services.
                    </p>
                </Section>

                <Section title="6. Your Rights and Choices">
                    <ul className="list-disc ml-6 space-y-2">
                        <li>Access and update your personal data</li>
                        <li>Delete your account or data (subject to legal requirements)</li>
                        <li>Opt-out of marketing emails at any time</li>
                        <li>Disable cookies and tracking (via browser settings)</li>
                    </ul>
                </Section>

                <Section title="7. Data Retention">
                    <p>
                        We retain your information only as long as necessary for legal,
                        operational, or business purposes. Inactive accounts may be removed after
                        extended periods of non-use.
                    </p>
                </Section>

                <Section title="8. Security Measures">
                    <p>
                        We implement industry-standard security practices including HTTPS,
                        encryption, secure storage, and access controls to protect your data.
                        However, no system is 100% secure, and we encourage strong password
                        practices.
                    </p>
                </Section>

                <Section title="9. International Users">
                    <p>
                        If you are accessing our service from outside the EU or US, be aware that
                        your data may be stored and processed in countries with different privacy
                        protections.
                    </p>
                </Section>

                <Section title="10. Changes to This Policy">
                    <p>
                        We may update this Privacy Policy from time to time. Significant changes
                        will be communicated through email or on our website.
                    </p>
                </Section>

                <Section title="11. Contact Us">
                    <p>
                        For any questions or requests regarding your privacy, you can reach us at:
                    </p>
                    <p className="mt-2 font-mono text-sm text-[--color-accent]">
                        privacy@meetyourplants.site
                    </p>
                </Section>
            </main>
        </div>
    );
}
