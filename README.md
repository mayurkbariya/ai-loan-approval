An enterprise-grade loan risk assessment platform built with **ASP.NET Core 10.0** and **ML.NET**. This project demonstrates a production-ready implementation of **Clean Architecture**, focusing on scalability, maintainability, and clear separation of concerns.

---

##  Key Features

*   **AI-Powered Risk Prediction**: Utilizes machine learning to analyze applicant profiles and predict loan repayment probability.
*   **Domain-Driven Safety Rules**: Implements strict business rule overrides (e.g., credit score floors) within the core domain.
*   **Scalable Architecture**: Decoupled layers ensuring that infrastructure and presentation concerns never leak into business logic.
*   **High-Performance ML Integration**: Uses `PredictionEnginePool` for thread-safe, optimized model execution in a web environment.
*   **Responsive Enterprise UI**: A polished, data-dense interface designed for professional financial applications.
*   **ML Training Pipeline**: Includes a dedicated trainer console app for model retraining and evaluation.

---

##  Application Screenshots

### Home Page
![Home Page](docs/screenshots/home-page.png)

### Loan Assessment Form
![Loan Assessment Form](docs/screenshots/loan-assessment-form.png)

### Assessment Results
| Low Risk (Approved) | Medium Risk (Review) | High Risk (Rejected) |

| ![Low Risk](docs/screenshots/low-risk-result.png) | ![Medium Risk](docs/screenshots/medium-risk-result.png) | ![High Risk](docs/screenshots/high-risk-result.png) |

---

##  Architecture Overview

The solution follows the **Clean Architecture** (Onion Architecture) pattern, ensuring the dependency flow always points inward toward the Domain.

###  Separation of Concerns

 Layer  Responsibility 

 **Domain**  The "Core". Contains entities, enums, exceptions, and pure business rules. No external dependencies. 
 **Application**  Orchestrates use cases. Contains DTOs, interfaces, and logic to coordinate business operations. 
 **Infrastructure**  Handles external concerns. Implements ML.NET model loading, prediction engine pooling, and file I/O. 
 **Web**  The "Shell". A thin MVC layer for presentation, DI registration, and request/response handling. 
 **Trainer**  A standalone console app for dataset processing and ML model generation. 

###  Solution Structure

```text
CreditRiskPlatform/
- CreditRiskPlatform.Domain/          # Core Business Entities & Rules
- CreditRiskPlatform.Application/     # Use Cases & Service Abstractions
- CreditRiskPlatform.Infrastructure/  # ML.NET Implementation & External Services
- CreditRiskPlatform.Web/             # ASP.NET Core MVC Presentation
- CreditRiskPlatform.Trainer/         # ML Model Training Pipeline
```

###  Dependency Flow
`Web`  `Application`  `Domain`  
`Infrastructure`  `Application` & `Domain`

---

##  ML.NET Integration

The application uses a **Binary Classification** model (Logistic Regression) trained on financial datasets.

*   **Isolation**: ML.NET attributes and schemas are isolated within the Infrastructure layer.
*   **Performance**: Instead of creating a new `PredictionEngine` for every request (which is not thread-safe), we use the `Microsoft.Extensions.ML` integration to manage an optimized pool of engines.
*   **Safety Layer**: While the AI provides a probability, the **Domain Rules** layer can override results based on regulatory or enterprise safety constraints (e.g., automatic rejection for credit scores below 400).

---

##  Technologies Used

*   **Backend**: .NET 10.0, ASP.NET Core MVC
*   **AI/ML**: ML.NET (Microsoft.ML)
*   **Frontend**: Bootstrap 5, Bootstrap Icons, jQuery
*   **Patterns**: Clean Architecture, Repository Pattern, Dependency Injection, DTOs

---

##  Setup & Installation

### Prerequisites
*   [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
*   Visual Studio  Insiders 2026 

###  Running the Application
1.  Clone the repository.
2.  Navigate to the solution root.
3.  Restore dependencies:
    ```bash
    dotnet restore
    ```
4.  Run the Web project:
    ```bash
    dotnet run --project CreditRiskPlatform.Web
    ```
5.  Open your browser at `http://localhost:5124`.

---
##  Running the Project Locally

### Solution File

Open the following solution file in Visual Studio:

```text
CreditRiskPlatform.sln
```

### Recommended Visual Studio Version

* Visual Studio 2022+
* .NET 10 SDK installed

---

## ▶ Set Startup Project

In **Solution Explorer**:

1. Right-click:

   ```text
   CreditRiskPlatform.Web
   ```

2. Select:

   ```text
   Set as Startup Project
   ```

The startup project name should appear in **Bold**.

---

##  Restore NuGet Packages

Before running the application:

1. Open the solution.
2. Go to:

```text
Build -> Build Solution
```

If package restore is required:

```text
Right Click Solution -> Restore NuGet Packages
```

---

##  Build the Solution

Build the entire solution:

```text
Build -> Build Solution
```

Expected result:

```text
Build: 5 succeeded, 0 failed
```

---

### Layer Responsibilities

| Layer          | Responsibility                         |
| -------------- | -------------------------------------- |
| Web            | UI, Controllers, Dependency Injection  |
| Application    | Use Cases, DTOs, Interfaces            |
| Domain         | Business Rules, Entities               |
| Infrastructure | ML.NET, External Services, Persistence |
| Trainer        | ML Model Training Pipeline             |

---

##  Run the Web Application

Start the application using:

```text
F5
```

or click the green **Start** button.

The browser will automatically open the application.

### Verification Steps

1. Open the Risk Assessment page.
2. Fill the loan application form.
3. Click:

   ```text
   Analyze Risk
   ```

If the result card appears successfully, the Application and Infrastructure layers are communicating correctly.

---

##  Run the ML Trainer

To retrain the ML model:

1. Right-click:

   ```text
   CreditRiskPlatform.Trainer
   ```

2. Select:

   ```text
   Debug -> Start New Instance
   ```

The console application will:

* train the ML model
* generate a new `model.zip`
* export the updated prediction model

---

##  Common Issues

### model.zip Not Found

Ensure the model file exists in:

```text
CreditRiskPlatform.Web/MLModels/
```

---

### Missing Namespace or Project Reference

If Visual Studio shows errors like:

```text
The type or namespace could not be found
```

Add the missing Project Reference:

```text
Right Click Project
-> Add
-> Project Reference
```

---

### Blank White Page on Startup

Usually caused by:

* invalid model path
* startup configuration issue
* missing static assets

Verify:

* `Program.cs`
* model path configuration
* static file configuration

---

##  Functional Verification

The refactored solution preserves the original project behavior while improving the internal architecture.

### Validation Checks

| Scenario                            | Expected Result   |
| ----------------------------------- | ----------------- |
| Age < 18                            | Validation error  |
| Credit Score < 400                  | High Risk         |
| High Income + High Score + Low Loan | Low Risk Override |

---

##  ML.NET + Business Rules

The system combines:

```text
ML.NET Prediction
+
Enterprise Business Validation Rules
```

This ensures:

* intelligent prediction
* enterprise-grade safety validation
* predictable business behavior
* maintainable domain logic


###  Training the ML Model
To retrain the model with updated data:
1.  Navigate to the Trainer project.
2.  Place your updated CSV in the data folder.
3.  Run the trainer:
    ```bash
    dotnet run --project CreditRiskPlatform.Trainer
    ```
4.  The new `model.zip` will be generated in the output directory. Copy this to the `Web/MLModels` folder.

---

##  Business Validation Rules

*   **Age**: Minimum 18 years.
*   **Credit Score**: 300 - 900 (Enterprise standard).
*   **Hard Overrides**: 
    *   Score < 400 -> **High Risk** (regardless of AI prediction).
    *   High income + High score + Low loan -> **Low Risk** (safety floor).

---

##  Roadmap & Future Improvements

*    Integration with SQL Server for applicant history persistence.
*    Implementation of a REST API layer for mobile integration.
*    Unit testing suite for Domain and Application layers.
*    Advanced Feature Engineering in the ML pipeline.

---

*Developed with  using .NET & ML.NET*
