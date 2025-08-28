DHCW Test Tools – Product Specification

1. Business Context & Overview
DHCW Test Tools is a web-based suite of utilities designed to support software testing, automation, and documentation for Digital Health and Care Wales (DHCW) teams. The application provides a unified interface for generating test data, monitoring API endpoints, converting documents, scraping web elements, and accessing essential test documentation. It is intended to standardize and accelerate the testing process across multiple DHCW projects, reduce manual effort, and improve quality and traceability.

2. Personas & Stakeholders
Test Analyst: Needs to generate test data, validate APIs, and document results efficiently.
Developer: Uses the tools to debug, verify endpoints, and understand test coverage.
Business Analyst: Reviews test documentation and ensures requirements traceability.
Project Manager: Monitors test progress and ensures compliance with standards.
Support Engineer: Uses logs and tools to reproduce and diagnose issues.

3. User Stories
As a Test Analyst, I want to generate realistic patient data and HL7 messages so I can test healthcare integrations.
As a Developer, I want to check the status and response times of all API endpoints so I can quickly identify issues.
As a Test Analyst, I want to convert documents and images between formats so I can prepare test artifacts and evidence.
As a Test Analyst, I want to scrape web elements and export selectors so I can automate UI tests.
As a Business Analyst, I want to browse and filter test documentation so I can find relevant guides and evidence.
As a Support Engineer, I want to compare XML files and merge differences so I can troubleshoot data issues.
As a Project Manager, I want all tools to be accessible in a single, secure web interface so I can ensure team efficiency and compliance.

4. Scope
Patient data generation and HL7 message creation
API endpoint monitoring and metrics visualization
Document and image conversion (PDF, DOCX, Base64, etc.)
Web element scraping for automation
Test document library and inline PDF viewing
XML comparison and merging
(Optional) Contact form for support/feedback

5. Functional Requirements
Generate random patient data and HL7 messages (configurable fields, export options)
Monitor and visualize API endpoint status and metrics (filter, export, charting)
Convert documents and images between supported formats (drag-and-drop, preview, download)
Scrape web elements and export for automation (URL management, selector export)
Browse, filter, and view test documents (category, search, inline PDF viewer)
Compare and merge XML files (highlight differences, merge, download)
Secure authentication and role-based access (future)

6. Non-Functional Requirements
Responsive and accessible UI (WCAG 2.1 AA)
Secure file handling and data privacy (GDPR, NHS standards)
Configurable endpoints and document sources (via appsettings)
Extensible architecture for new tools/modules
Audit logging for key actions (future)
High availability and disaster recovery (future)

7. Technical Constraints
Internal DHCW use only
.NET 8.0 and modern browser support (Chrome, Edge, Firefox)
Hosted on DHCW infrastructure (IIS, Azure, or on-prem)
All data stored in-memory or on disk; no external DB required
All uploads and conversions are sandboxed and virus-scanned (future)

8. Acceptance Criteria
All features are accessible via a unified web interface
Only active endpoints are included in metrics and status checks
All document and data operations are secure and auditable
All tools are tested and documented
All user stories are covered by test cases

9. Deployment, Maintenance, and Support (Expanded)
Deployment: Follows standard .NET publish workflow. Supports deployment to IIS, Azure App Service, or on-premises Windows Server. Includes pre-deployment checklist (config, SSL, permissions).
Configuration: Managed via appsettings.json, environment variables, and secure secrets for sensitive data.
Maintenance: Includes regular .NET and dependency updates, security patching, and periodic review of endpoint/document configs. Automated health checks and monitoring recommended.
Support: User and admin guides provided. Support requests via contact form or internal ticketing. Troubleshooting guide and FAQ included. Escalation path for critical issues.
Backup/Restore: Procedures for backing up uploaded/converted files and restoring from backup. Disaster recovery plan documented.

10. Glossary (Expanded)
HL7: Health Level 7, a standard for healthcare data exchange
API Endpoint: A URL that exposes a service for integration
POM: Page Object Model, a pattern for UI test automation
CSV: Comma-Separated Values, a common data export format
Base64: A method for encoding binary data as text
IIS: Internet Information Services, a web server for hosting .NET apps
Azure App Service: Microsoft cloud platform for hosting web apps
AppSettings: Configuration file for .NET applications
Disaster Recovery: Procedures for restoring service after major failure
Traceability Matrix: Table mapping requirements to design and test cases

11. Traceability Matrix (Expanded)
Requirement/User Story	Design Section	Test Case(s)	Status
Generate patient data	3.1	Patient Generator: all	Complete
API monitoring	3.2	Service Checker: all	Complete
Document conversion	3.3	Document Conversion: all	Complete
Web scraping	3.4	Web Scraper: all	Complete
Test docs library	3.5	Test Documents: all	Complete
XML comparison	3.6	XML Comparison: all	Complete
Security & privacy	6	All: negative/edge	In Progress
Extensibility	5	N/A	In Progress

12. Deliverables
Source code and documentation
Deployment instructions
User and admin guides
Test cases and test plan
Support and maintenance procedures

13. References
NHS Digital: HL7 Standards (https://digital.nhs.uk/services/terminology-and-classifications/hl7)
Microsoft Docs: ASP.NET Core (https://docs.microsoft.com/aspnet/core)
DHCW Internal Testing Standards
Bootstrap Documentation (https://getbootstrap.com/)
Chart.js Documentation (https://www.chartjs.org/docs/)
