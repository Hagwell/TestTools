DHCW Test Tools – Product Specification (Future Version)

14. Planned Enhancements & Roadmap
Patient Generator API Integration: Integrate with an external or internal API to generate patient data. Store generated patients in MongoDB. Ensure NHS numbers are unique by checking the database before generation.
Master Patient HL7 Tab: Add a "Register" button for Admin users to submit each generated HL7 message to a web service. Each message is submitted individually, with status feedback.
Test Documents Library: Enable upload and display of PDFs from base64-encoded images stored in MongoDB. Support both file and base64 upload.
Service Checker: Store API endpoints in a MongoDB collection. Retrieve and display endpoints based on their "active" status. Allow Admins to add, edit, or deactivate endpoints.
XML Compare: Add a full diff view with highlighted differences between FirstXmlFile and SecondXmlFile, supporting side-by-side and inline views.
File Converter: Show a "Converting..." message with a loading GIF and progress bar during all conversions for better UX feedback.
Web Scraper: Support scraping across an entire website (multi-page crawl). Securely store logon credentials in MongoDB for authenticated scraping where required.
Admin Sign-In: Implement authentication for Admin users. Admins can upload/update TestDocs, manage endpoints, and submit patient registry requests.
MongoDB Integration: Use MongoDB for all persistent data (patients, endpoints, documents, credentials, logs).
Security: Encrypt sensitive data (credentials, patient info). Enforce role-based access. Audit all admin actions.
Extensibility: Modularize all tools for easy addition of new utilities (e.g., FHIR message generator, API mocking, test data anonymizer).
Accessibility & Internationalization: Ensure full WCAG 2.1 AA compliance and support for multiple languages.
Reporting & Analytics: Add dashboards for usage, error rates, and test coverage.
CI/CD & DevOps: Automate build, test, and deployment pipelines. Support containerization (Docker) for local and cloud deployment.
