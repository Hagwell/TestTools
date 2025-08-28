DHCW Test Tools – Design Document (Future Version)

11. Future Architecture & Module Design
API Integration Layer: Abstract service for patient data generation, supporting both internal logic and external API calls. Implements NHS number uniqueness check via MongoDB.
MongoDB Data Layer: All entities (patients, endpoints, documents, credentials, logs) are stored in MongoDB. Use Mongoose or MongoDB .NET Driver for access.
Authentication & Authorization: Use ASP.NET Core Identity or Auth0 for user management. Admin role required for sensitive actions.

Patient Generator:
UI: Add "Register" button in HL7 tab (visible to Admins).
Backend: On click, submit HL7 message to a configurable web service endpoint. Track submission status in MongoDB.

TestDocs Library:
UI: Add upload button for base64-encoded PDFs.
Backend: Store/retrieve PDFs as base64 in MongoDB. Render PDFs in-browser.

Service Checker:
UI: Endpoint management for Admins.
Backend: CRUD endpoints in MongoDB. Filter by "active" status.

XML Compare:
UI: Side-by-side and inline diff views with color-coded highlights.
Backend: Use XML diff libraries to compute and store differences.

File Converter:
UI: Show progress bar and loading GIF during conversion.
Backend: Stream conversion progress to UI (SignalR or polling).

Web Scraper:
UI: Option to scrape entire site or single page.
Backend: Crawl site map, handle authentication using stored credentials.

Security:
Encrypt all sensitive fields in MongoDB.
Use HTTPS everywhere.
Audit log for all admin actions.

Extensibility:
Plugin architecture for new tools.
API for external integrations (e.g., test automation frameworks).

Testing:
Full unit, integration, and E2E test coverage.
Mock external APIs for test isolation.

DevOps:
Dockerfile and Kubernetes manifests for deployment.
GitHub Actions or Azure DevOps for CI/CD.
