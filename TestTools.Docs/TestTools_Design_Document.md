DHCW Test Tools – Design Document
1. Introduction
This document details the modular design of the DHCW Test Tools project, supporting agile development and sprint planning in Azure DevOps. It provides technical guidance for maintainers and contributors.

2. Architecture Overview
ASP.NET Core MVC web application
Modular Razor views for each tool
Service layer for business logic
Configuration-driven (appsettings.json)
Responsive Bootstrap-based UI
No external database; all data is in-memory or on disk

2.1 High-Level Architecture Diagram
+-------------------+
|   User Browser    |
+--------+----------+
         |
         v
+--------+----------+
|   ASP.NET Core    |
|   Controllers     |
+--------+----------+
         |
         v
+--------+----------+
|   Services Layer  |
+--------+----------+
         |
         v
+--------+----------+
|   File System     |
|   (uploads, docs) |
+-------------------+

3. Modules & Components
3.1 Patient Generator
UI: Razor view with tabbed interface for patient data and HL7
Service: PatientService, NhsNumberGenerator, DOBGenerator, Name/Address generators
Features: Configurable patient count, export as CSV, HL7 message generation, clear/reset
Validation: Input validation for patient count, error handling for generation failures

3.2 Service Checker
UI: Table of endpoints, status indicators, metrics chart (Chart.js)
Service: API polling, response time measurement, error categorization
Features: Filter by active endpoints, export results, recheck failed endpoints
Validation: Timeout handling, error reporting, endpoint config validation

3.3 Document & Image Conversion
UI: Drag-and-drop file upload, format selection, preview/download
Service: DocumentConversionService, ImageConversionService, Base64Service
Features: PDF/DOCX/HTML/TXT/MD conversion, image format conversion, Base64 encode/decode
Validation: File type/size validation, error handling for unsupported formats

3.4 Web Scraper
UI: URL input, table of extracted elements, export for POM
Service: WebScraperService (HTML parsing, selector extraction)
Features: Add/remove URLs, scrape on demand, export selectors
Validation: URL validation, error handling for unreachable pages

3.5 Test Documents Library
UI: Filterable/searchable table, inline PDF viewer
Service: DocumentTabsViewModel, FrameworkGuidesViewModel, SupportGuidesViewModel
Features: Category filtering, search, inline preview, upload (future)
Validation: File existence, error handling for missing/corrupt files

3.6 XML Comparison
UI: Dual file/text input, diff/merge view, download merged XML
Service: XmlCompareService
Features: Highlight differences, merge, download
Validation: XML schema validation, error handling for malformed XML

3.7 Shared Components
UI: Navigation, layout, theming, modals
Service: Error handling, notifications, logging (future)

4. Data Flow & Error Handling
All file uploads are validated for type and size before processing
Conversion and scraping errors are caught and reported to the user
All file writes are atomic; temp files are cleaned up on error
API endpoint failures are logged and surfaced in the UI
All user actions are validated on both client and server

5. Extensibility
New tools can be added as Razor views and services
Endpoints and document sources are configured in appsettings.json
JS/CSS is modular and scoped to each tool
Future: plug-in architecture for custom tools

6. Security & Compliance
All file uploads are scanned for type and size; future: virus scanning
Data privacy: no PII is stored long-term; all test data is synthetic
Role-based access and authentication planned for future
Audit logging for key actions (future)
All endpoints are protected against CSRF and XSS

7. Traceability (Expanded)
Each module maps to specific user stories and requirements in the Product Specification.
All features are covered by Gherkin test cases (see Test Plan).
Traceability matrix is maintained in the Product Specification for cross-reference.

8. Deployment, Maintenance, and Support
Deployment: .NET publish to IIS, Azure App Service, or on-prem. Includes pre-deployment validation, SSL setup, and config checks.
Maintenance: Regular updates for .NET, NuGet packages, and JS/CSS dependencies. Automated tests and health checks recommended.
Support: User/admin guides, troubleshooting section, and escalation process for unresolved issues. Contact form for user feedback.
Backup/Restore: Procedures for file backup and disaster recovery are documented.

9. Glossary
Razor View: ASP.NET page template for dynamic HTML
Service Layer: Business logic abstraction in .NET
Bootstrap: Front-end CSS framework
Chart.js: JavaScript charting library
NuGet: .NET package manager
Gherkin: Syntax for writing test cases in BDD
App Service: Azure web app hosting platform
CSRF/XSS: Web security vulnerabilities

10. References
Microsoft Docs: ASP.NET Core MVC (https://docs.microsoft.com/aspnet/core/mvc)
NHS Digital HL7 (https://digital.nhs.uk/services/terminology-and-classifications/hl7)
Bootstrap (https://getbootstrap.com/)
Chart.js (https://www.chartjs.org/docs/)
DHCW Internal Documentation
README.md for usage and structure
Product Specification
Test Plan
