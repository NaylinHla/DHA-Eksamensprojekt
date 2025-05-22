import React, { useState } from "react";
import {EmailClient} from "../../generated-client.ts"; // Adjust path
import {Link} from "react-router";

const emailClient = new EmailClient("http://localhost:5000"); // Adjust base URL

export default function Footer() {
    const [email, setEmail] = useState("");

    const showToast = (message: string, isError = false) => {
        const toastContainer = document.getElementById("toast-container");
        if (!toastContainer) return;

        const toast = document.createElement("div");
        toast.className = `alert ${isError ? "alert-error" : "alert-success"} text-white`;
        toast.innerHTML = `<span>${message}</span>`;
        toastContainer.appendChild(toast);

        setTimeout(() => {
            toast.remove();
        }, 3000); // Toast disappears after 3 seconds
    };

    const handleSubscribe = async (e: React.FormEvent<HTMLFormElement>) => {
        e.preventDefault();
        try {
            await emailClient.subscribeToEmailList({email});
            setEmail("");
            showToast("Subscribed successfully!");
        } catch (err) {
            console.error("Subscription failed", err);
            showToast("Subscription failed. Please try again.", true);
        }
    };

    return (
        <footer className="footer sm:footer-horizontal bg-base-200 text-base-content p-10">
            <nav>
                <h6 className="footer-title">Company</h6>
                <Link className="link link-hover" to="/about">About us</Link>
                <Link className="link link-hover" to="/contact-us">Contact</Link>
                <Link className="link link-hover" to="/career">Career</Link>
            </nav>
            <nav>
                <h6 className="footer-title">Services</h6>
                <Link className="link link-hover" to="/advertisement">Advertisement</Link>
                <Link className="link link-hover" to="/marketing">Marketing</Link>
            </nav>
            <nav>
                <h6 className="footer-title">Legal</h6>
                <Link className="link link-hover" to="/terms">Terms of use</Link>
                <Link className="link link-hover" to="/privacy">Privacy policy</Link>
                <Link className="link link-hover" to="/cookies">Cookie policy</Link>
            </nav>

            <form onSubmit={handleSubscribe}>
                <h6 className="footer-title">Newsletter</h6>
                <fieldset className="w-80">
                    <label className="flex ml-2">Enter your email address</label>
                    <div className="flex join mt-2">
                        <input
                            type="email"
                            name="email"
                            value={email}
                            onChange={(e) => setEmail(e.target.value)}
                            placeholder="username@site.com"
                            className="input input-bordered join-item"
                            required
                        />
                        <button type="submit" className="btn btn-primary join-item">
                            Subscribe
                        </button>
                    </div>
                </fieldset>
            </form>
        </footer>
    );
}
