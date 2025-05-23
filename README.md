# 🌱 MeetYourPlants - Greenhouse Management Application

**MeetYourPlants** is a full-stack application developed for the final exam project. It enables users to monitor, manage, and optimize their greenhouse environment in real time using modern software technologies.

## Features

-  **Real-time monitoring** of temperature, humidity, and other environmental metrics
-  **Plant management** with personalized settings and preferences
-  **Weather integration** for localized environmental context
-  **User settings** including temperature unit preferences and UI customization
-  **JWT-based authentication** for secure user access
-  **Email notifications** and contact form support
-  **Dashboard overview** of greenhouse metrics and trends

## Tech Stack

### Frontend
- **React** + **TypeScript**
- **Tailwind CSS** / **DaisyUI**
- **Jotai** for global state management
- **React Router** for navigation
- **Vite** for fast development build

### Backend
- **ASP.NET Core Web API**
- **Entity Framework Core** with **PostgreSQL**
- **Fleck** for WebSocket functionality
- **NSwag** for API client generation
- **JWT** for authentication
- **FeatureHub SDK** (optional feature toggles)

### DevOps / Testing
- **NUnit** for backend testing
- **GitHub Actions** for CI
- **Self-hosted runners** for when you run out of free runners
