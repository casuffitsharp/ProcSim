# ProcSim - Process Scheduling Simulator

## 📌 Project Overview
ProcSim is an educational simulator designed to demonstrate **process scheduling algorithms** in **operating systems**.  
The goal is to allow users to visualize and understand different scheduling strategies such as **First-Come, First-Served (FCFS)**, **Round Robin (RR)**, and **Shortest Job First (SJF)**.

This project follows a **modular architecture** where the **core scheduling logic** is independent of the **user interface**.  
The application will have two interfaces:  
1. **Console Application** - For quick testing and debugging of scheduling behavior.  
2. **WPF GUI** - A graphical interface following the **MVVM pattern**, allowing real-time visualization of processes.

---

## 🔹 Technical Requirements
- **Programming Language:** C# (.NET 9+)
- **Architecture:** **MVVM (Model-View-ViewModel)**
- **Project Structure:**
  - **Core Library (`ProcSim.Core`)** → Contains models, services, and logic for scheduling.
  - **Console UI (`ProcSim.Console`)** → CLI-based interface for running simulations.
  - **WPF UI (`ProcSim.WPF`)** → Desktop application with visual representation of process execution.
  - **Unit Tests (`ProcSim.Tests`)** → xUnit test cases for validating scheduling logic.

- **Key Features:**
  ✅ Process creation and scheduling simulation  
  ✅ Support for **preemptive and non-preemptive** scheduling algorithms  
  ✅ Metrics tracking (waiting time, turnaround time)  
  ✅ Visual representation of process states (ready, running, blocked)  

---

## 🔹 Folder Structure
```
ProcSim/
│── .github/workflows/         # GitHub Actions CI/CD automation
│   └── ci.yml                 # Workflow to build and test the project
│
│── docs/                      # Documentation files
│   ├── architecture.md        # MVVM and project architecture explanation
│   ├── installation.md        # How to install and run the simulator
│   ├── usage.md               # User guide
│
│── src/                       # Main source code
│   ├── ProcSim.Core/          # Core logic (Models, Services, ViewModels)
│   │   ├── Models/            # Process models (Process, Scheduler, etc.)
│   │   ├── Services/          # Scheduling logic
│   │   ├── ViewModels/        # MVVM ViewModels
│   │   └── ProcSim.Core.csproj
│   │
│   ├── ProcSim.Console/       # Console-based simulation
│   │   ├── Program.cs         # Entry point for console testing
│   │   ├── ConsoleUI.cs       # Handles input/output
│   │   └── ProcSim.Console.csproj
│   │
│   ├── ProcSim.WPF/           # WPF-based GUI application
│   │   ├── Views/             # XAML-based UI screens
│   │   ├── ViewModels/        # MVVM bindings
│   │   ├── App.xaml
│   │   ├── MainWindow.xaml
│   │   └── ProcSim.WPF.csproj
│
│── tests/                     # Unit tests
│   ├── ProcSim.Tests/         # xUnit tests for core logic
│   │   ├── ProcessTests.cs    # Test cases for process management
│   │   ├── SchedulerTests.cs  # Test cases for scheduling algorithms
│   │   └── ProcSim.Tests.csproj
│
│── .gitignore                 # Ignored files
│── LICENSE                    # Project license
│── README.md                  # Main project documentation
│── ProcSim.sln                # Solution file
```

---

## 🔹 Development Guidelines
### **1️⃣ Core Library (`ProcSim.Core`)**
- Defines **process models** (`Process.cs`) and **scheduling logic** (`Scheduler.cs`).
- Implements **scheduling strategies** using interfaces and modular classes.
- Should be **independent of any UI implementation**.

### **2️⃣ Console Application (`ProcSim.Console`)**
- A simple **CLI tool** to test process scheduling behavior.
- Displays output as **logs**, showing process execution order and performance metrics.

### **3️⃣ WPF GUI (`ProcSim.WPF`)**
- Implements **MVVM pattern**.
- Uses **data binding** for real-time process state visualization.
- Displays a **Gantt chart or timeline** of process execution.

### **4️⃣ Unit Testing (`ProcSim.Tests`)**
- Uses **xUnit** for validating:
  ✅ Process execution flow  
  ✅ Scheduling fairness and performance  
  ✅ Correctness of turnaround time and waiting time calculations  

---

## 🔹 CI/CD Automation
A **GitHub Actions** workflow (`.github/workflows/ci.yml`) should:
1. **Run on every push and pull request**.
2. **Build the solution** (`dotnet build`).
3. **Run unit tests** (`dotnet test`).
4. **Package artifacts** for easy testing.

---

## **🚀 Next Steps**
- Implement **FCFS and Round Robin** in `ProcSim.Core`.
- Develop an initial **WPF UI prototype** for process visualization.
- Write **unit tests** for scheduling correctness.

---

This file should be used by **GitHub Copilot** to understand the full project scope, requirements, and structure.
