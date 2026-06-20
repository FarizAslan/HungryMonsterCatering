# Hungry Monster – Catering Ledger

## Documentation

**Author:** Fariz Aslan
**Technology:** ASP.NET Core 8 + HTML / CSS / JavaScript frontend

---

## 1. Business Problem

Hungry Monster is a company that runs a cafeteria and a lunch delivery service in an
industrial park. Since 2015 it has cooperated with more than a dozen business
partners (contractors): for each partner, the company serves lunches to that
partner's employees.

For every year, the company knows **how many meals** were served to each
partner. If, in a given year, Hungry Monster did **not** cooperate with a
particular partner at all, the number of meals for that partner in that year
is **0**.

The CEO needs an application that:

1. Allows entering this data (year + number of meals served to each partner).
2. Analyzes the data to answer the question:
   **"In which year did the highest number of different partners use the
   catering service at least once?"**

A partner "used the service at least once" in a given year if the number of
meals served to them in that year is greater than 0.

---

## 2. Solution Overview

The problem is solved as an **ASP.NET Core 8** application made of two parts:

- **Backend** — a Minimal API (C#) that holds the data in memory and exposes
  a small set of JSON endpoints to add partners, save yearly data, read the
  full table, and run the analysis.
- **Frontend** — a plain **HTML + CSS + JavaScript** single page (no
  framework, no build step) served directly by the same application. It
  calls the backend endpoints with `fetch()` and displays the results.

This replaces the earlier idea of typing data into a terminal: the user now
fills in simple web forms in the browser, which is both easier to use and
easier to demonstrate/screenshot.

Everything still runs from a single `dotnet run` command — no database,
no external services, no internet connection required at runtime (aside from
loading two Google Fonts used purely for styling, which the browser fetches
automatically and which is **not** required for the app to function; it
still works fully offline, just with fallback system fonts).

### How to compile and run

1. Install the [.NET 8 SDK](https://dotnet.microsoft.com/) if not already installed.
2. Open a terminal in the `HungryMonsterCatering` folder (the one containing
   `HungryMonsterCatering.csproj`).
3. Run:
   ```
   dotnet build
   dotnet run
   ```
4. Open a web browser and go to:
   ```
   http://localhost:5000
   ```
5. The Catering Ledger page loads, with four steps: **Partners → Yearly
   entry → Ledger → Verdict**.

---

## 3. Data Model

| Concept     | Represents                                                                 |
|-------------|------------------------------------------------------------------------------|
| **Contractor** | The name of a business partner Hungry Monster cooperates with.            |
| **Year**       | A calendar year (e.g. 2023) for which data is being recorded.             |
| **Meals**      | Number of meals served to a given contractor's employees in a given year. A value of `0` means there was no cooperation with that contractor that year. |

This is implemented with two main classes (unchanged from the original
design — only the way data gets *into* them changed, from console prompts to
HTTP requests):

- **`YearlyRecord`** (`Models/YearlyRecord.cs`)
  Represents a single year. Internally it stores a dictionary that maps
  *contractor name → number of meals served* for that year. It also exposes
  two helper methods:
  - `CountActiveContractors()` – counts how many contractors have a value
    greater than 0 in that year (i.e., how many partners were "active").
  - `TotalMeals()` – sums all meals served in that year.

- **`CateringDataStore`** (`Services/CateringDataStore.cs`)
  Holds all the data entered during the application's run, registered as a
  **singleton service** in dependency injection so the same in-memory data
  is shared by every API request:
  - The list of known contractor names.
  - The list of `YearlyRecord` objects (one per year that has been entered).

---

## 4. Analysis Logic (the core of the assignment)

The actual business question is answered by the static class
**`AnalysisService`** (`Services/AnalysisService.cs`), specifically by the
method:

```csharp
public static AnalysisResult FindYearsWithMostActiveContractors(List<YearlyRecord> records)
```

### Algorithm (step by step)

1. For every year that has data entered, count how many contractors have a
   meal count greater than 0 in that year — this is done by
   `YearlyRecord.CountActiveContractors()`.
2. Find the **maximum** value of that count across all years
   (`records.Max(r => r.CountActiveContractors())`).
3. Collect **all** years whose active-contractor count equals that maximum
   (there could be a tie between two or more years).
4. Return the list of winning year(s) together with the count.

This directly answers the client's question: *"In which year did more
companies use the catering service at least once?"* If several years are
tied for the highest number of active partners, the application reports all
of them instead of arbitrarily picking one.

### Why this approach is correct

- A contractor counts as "having used the service" in a year purely based on
  whether their recorded value for that year is greater than zero, exactly as
  specified in the assignment ("If, in a given year, the catering provider
  did not work with a particular contractor, the value will be 0").
- The number of meals itself (5 vs. 500) does not matter for this particular
  question — only whether the partner was active (>0) or not (=0) — so the
  algorithm correctly ignores the magnitude of meals and only checks for
  "greater than zero".

---

## 5. API Endpoints (backend)

| Method | Route              | Purpose                                                       |
|--------|---------------------|----------------------------------------------------------------|
| GET    | `/api/contractors`  | Returns the list of registered business partner names.        |
| POST   | `/api/contractors`  | Registers a new partner. Body: `{ "name": "Nordic Logistics" }` |
| GET    | `/api/data`          | Returns the full table: contractors, and one row per year with meal counts + active-partner count. |
| POST   | `/api/data`          | Saves meal counts for one year. Body: `{ "year": 2023, "meals": { "Nordic Logistics": 140 } }` |
| GET    | `/api/analysis`      | Runs the analysis and returns the winning year(s) + active count. |

All endpoints return JSON and standard HTTP status codes (`200 OK`,
`400 Bad Request`, `409 Conflict`) so the frontend can show clear success or
error messages.

---

## 6. Frontend (HTML / CSS / JavaScript)

The frontend lives entirely in `wwwroot/` and is plain static files — no
React, no build tools, just files the browser reads directly:

- **`index.html`** — page structure: a header, a 4-step tab navigation
  (Partners / Yearly entry / Ledger / Verdict), and one panel per step.
- **`css/styles.css`** — visual styling (a "kitchen ledger" theme: dark
  green header, warm paper-colored content cards, a monospace font for the
  data table, and a stamped-looking badge for the final result).
- **`js/app.js`** — all the interactive behavior:
  - Switches between the four tabs.
  - Loads and submits data using `fetch()` calls to the API endpoints listed
    above.
  - Dynamically builds the "enter meals per partner" form based on whichever
    partners have been registered.
  - Renders the ledger table and the final analysis result.

### Step-by-step usage

1. **Partners tab** — type a partner name and click **Add partner**. It
   appears as a small tag below the form. Repeat for every business partner.
2. **Yearly entry tab** — enter a year, then a number input appears for
   *every* registered partner (defaulting to 0). Fill in meals served, click
   **Save year data**. Repeat for each year you want to record.
3. **Ledger tab** — shows a table with one row per year, one column per
   partner, and a final column with how many partners were active that year.
4. **Verdict tab** — click **Run analysis**; the busiest year (or years, in
   case of a tie) is displayed on a stamp-style badge along with the active
   partner count.

---

## 7. Project Structure

```
├── Documentation.md                          <- this file
├── Screenshots/                               <- application run screenshots
└── HungryMonsterCatering/
    ├── HungryMonsterCatering.csproj           <- project file (ASP.NET Core 8 Web SDK)
    ├── Program.cs                             <- app startup + API endpoints
    ├── Models/
    │   └── YearlyRecord.cs                    <- data for a single year
    ├── Services/
    │   ├── CateringDataStore.cs               <- holds/manages all entered data
    │   └── AnalysisService.cs                 <- the "which year" analysis logic
    ├── Dtos/
    │   └── ApiModels.cs                       <- request/response shapes for the API
    └── wwwroot/                                <- frontend (served as static files)
        ├── index.html
        ├── css/
        │   └── styles.css
        └── js/
            └── app.js
```

This structure separates concerns:

- **Models / Services** – business/data logic, identical in spirit to a
  console version; they know nothing about HTTP or HTML.
- **Dtos** – simple shapes used only to move data in and out over JSON.
- **Program.cs** – wires everything together and exposes it over HTTP.
- **wwwroot** – the presentation layer; could be swapped for a different UI
  without touching the backend logic at all.

---

## 8. Example Walkthrough

Suppose Hungry Monster works with 4 partners: **Nordic Logistics**,
**BuildRight Construction**, **TechHub Solutions**, and **Steelworks
Manufacturing**.

Data entered through the **Yearly entry** tab:

| Year | Nordic Logistics | BuildRight Construction | TechHub Solutions | Steelworks Manufacturing | Active partners |
|------|-------------------|---------------------------|----------------------|-----------------------------|------------------|
| 2021 | 140               | 0                          | 95                    | 60                           | 3                |
| 2022 | 160               | 110                        | 0                     | 70                           | 3                |
| 2023 | 0                 | 130                        | 120                   | 85                           | 3                |
| 2024 | 180               | 140                        | 100                   | 90                           | 4                |

Running the analysis (Verdict tab) would report:

```
Busiest year: 2024
4 active partners
```

If, hypothetically, two different years both reached 4 active partners, the
stamp would show both years (e.g. "2023 & 2024") with the tied count,
instead of arbitrarily picking one — this matches exactly what the
`AnalysisService` algorithm does (see Section 4).

---

## 9. Possible Future Extensions (not required by the assignment)

- Saving/loading data to/from a file or database so it persists between runs
  (currently all data lives only in memory and resets when the app stops).
- Editing or deleting previously entered partners/years.
- Exporting the ledger table and analysis result as a downloadable report.
- Basic authentication if multiple staff members were to use the tool.

These were intentionally left out to keep the project focused and fully
aligned with what was requested in the assignment description.
