import React, {useState} from "react";
import {TitleTimeHeader} from "../import";
import toast from "react-hot-toast";

const ContactUsPage = () => {
    const [form, setForm] = useState({
        name: "",
        email: "",
        subject: "",
        message: ""
    });

    const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
        const { name, value } = e.target;
        setForm((prev) => ({ ...prev, [name]: value }));
    };

    const handleSubmit = (e: React.FormEvent) => {
        e.preventDefault();
        toast.success("Message successfully sent!");
        setForm({ name: "", email: "", subject: "", message: "" });
    };

    return (
        <div className="min-h-[calc(100vh-64px)] flex flex-col bg-[--color-background] text-[--color-primary] font-display overflow-hidden">
            <TitleTimeHeader title="Contact Us" />

            <main className="flex-1 overflow-auto px-4 py-10 max-w-6xl mx-auto">
                <div className="grid grid-cols-1 md:grid-cols-2 gap-8 items-start">
                    {/* Contact Details */}
                    <div className="bg-[var(--color-surface)] p-8 rounded-lg shadow-lg space-y-4">
                        <h2 className="text-2xl font-semibold">Get in Touch</h2>
                        <p className="text-[--color-primary]">We’d love to hear from you. Reach us directly via email or phone:</p>

                        <div className="space-y-2 text-[--color-primary]">
                            <p><strong>Email:</strong> contact@greenhouseapp.com</p>
                            <p><strong>Phone:</strong> +1 (310) 555-1212</p>
                            <p><strong>Address:</strong> 123 Greenhouse Lane, Garden City, Earth</p>
                        </div>

                        <p className="text-[--color-primary] text-sm pt-2">We usually respond within 24 hours on weekdays.</p>
                    </div>

                    {/* Contact Form */}
                    <form onSubmit={handleSubmit} className="bg-[var(--color-surface)] p-8 rounded-lg shadow-lg space-y-6">
                        <h2 className="text-2xl font-semibold">Send an Email to Us</h2>

                        <div>
                            <label className="block mb-1 text-sm font-medium">Your Name</label>
                            <input
                                type="text"
                                name="name"
                                value={form.name}
                                onChange={handleChange}
                                required
                                className="w-full px-4 py-2 rounded border border-gray-300 focus:outline-none focus:ring-2 focus:ring-[--color-accent]"
                            />
                        </div>

                        <div>
                            <label className="block mb-1 text-sm font-medium">Email Address</label>
                            <input
                                type="email"
                                name="email"
                                value={form.email}
                                onChange={handleChange}
                                required
                                className="w-full px-4 py-2 rounded border border-gray-300 focus:outline-none focus:ring-2 focus:ring-[--color-accent]"
                            />
                        </div>

                        <div>
                            <label className="block mb-1 text-sm font-medium">Subject</label>
                            <input
                                type="text"
                                name="subject"
                                value={form.subject}
                                onChange={handleChange}
                                required
                                className="w-full px-4 py-2 rounded border border-gray-300 focus:outline-none focus:ring-2 focus:ring-[--color-accent]"
                            />
                        </div>

                        <div>
                            <label className="block mb-1 text-sm font-medium">Message</label>
                            <textarea
                                name="message"
                                value={form.message}
                                onChange={handleChange}
                                rows={5}
                                required
                                className="w-full px-4 py-2 rounded border border-gray-300 resize-none focus:outline-none focus:ring-2 focus:ring-[--color-accent]"
                            />
                        </div>

                        <button
                            type="submit"
                            className="btn btn-neutral bg-transparent btn-l"
                        >
                            Send
                        </button>
                    </form>
                </div>
            </main>
        </div>
    );
};

export default ContactUsPage;
