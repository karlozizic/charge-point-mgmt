# EV Charging Point Management System

🚗⚡ A cloud-native platform for managing Electric Vehicle (EV) charging points, built as part of my Master's Thesis at the Faculty of Electrical Engineering and Computing, University of Zagreb.  
Deployed on **Azure Container Apps**:  
👉 https://cpms-frontend.jollysky-76e311d7.northeurope.azurecontainerapps.io/

---

## 📖 Overview
The **Charging Point Management System (CPMS)** is a full-stack solution designed to manage electric vehicle charging infrastructure.  
It supports **OCPP 1.6 (Open Charge Point Protocol)** and provides a scalable, event-driven architecture suitable for real-world e-mobility applications.

**Key features:**
- 🔌 Communication with EV chargers via OCPP 1.6  
- 🗄️ Microservices architecture with event sourcing  
- 📊 Real-time monitoring of charging sessions  
- 💳 Payment and billing support  
- ☁️ Cloud deployment on Microsoft Azure  

---

## 🏗️ Architecture
The system is designed as a **distributed microservices architecture**:
- **Backend:** C#, .NET, Event Sourcing (CQRS)  
- **Frontend:** React.js + TypeScript
- **Database:** PostgreSQL (with event store)  
- **Deployment:** Azure Container Apps