# SoPro - Onboarding WebApp

A web application designed to streamline and digitalize onboarding processes in a corporate environment. Built with the .NET Framework, Identity, and Entity Framework Core to provide a secure, customizable, and efficient onboarding experience.

---

## Features

- **Login System**  
  Secure user authentication and authorization using ASP.NET Identity.

- **Admin Control Panel**  
  Manage users, processes, and templates with an intuitive administrative interface.

- **Customizable Onboarding Processes**  
  Define and tailor onboarding processes to fit your company’s specific needs.

- **Definable Process Templates**  
  Create reusable process-templates consisting of multiple assignments.

- **Timed Assignments and Processes**  
  Assignments and entire processes can have configurable time limits, ensuring timely completion.

---

## Technologies Used

- [.NET Framework](https://dotnet.microsoft.com/)  
- [ASP.NET Identity](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/identity)  
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/)  

---

## Getting Started

### Prerequisites

- [.NET Framework SDK](https://dotnet.microsoft.com/download)  
- [SQL Server](https://www.microsoft.com/en-us/sql-server) or any compatible database  
- [Docker](https://www.docker.com/get-started) (for containerized setup)  

### Clone the repository

```bash
git clone https://github.com/wowtamz/web-onboarding.git
cd web-onboarding
```

### Running with .NET

**Navigate to the source project directory**

```bash
cd Projekt/SoPro24Team06
```

**Run the project with dotnet command**

```bash
dotnet run
```

### Running with Docker

**Navigate to the project directory containing the Dockerfile**

```bash
cd Projekt/SoPro24Team06
```

**Build the Docker image**
    
```
docker build -t onboarding-app .
```

**Run the Docker container**
    
```bash
docker run --name onboarding-cont --rm -d -p PORT:5000/tcp onboarding-app
```

**Access the application**
   
Open your browser and go to:

```bash
http://localhost:PORT
```

## License

This project is open source and available under the [MIT License](LICENSE).
