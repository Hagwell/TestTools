# DHCW Test Tools

DHCW Test Tools is a comprehensive web-based suite of utilities designed to streamline and support software testing, automation, and documentation processes for DHCW (Digital Health and Care Wales) teams. The application provides a unified interface for generating test data, monitoring API endpoints, converting documents, scraping web elements, and accessing essential test documentation.

---

## Features

### 1. **Patient Generator**
- Generate random patient data for testing.
- Export generated patients as CSV.
- Generate HL7 messages for Master Patient Index and "All Wales" scenarios.
- Copy, clear, and save HL7 messages.

### 2. **Service Checker**
- Monitor the status and response times of API endpoints/services.
- View real-time metrics and export results.
- Filter and check only active endpoints (as defined in `appsettings.json`).
- Visualize endpoint health with charts and status indicators.

### 3. **Document Conversion**
- Convert documents between formats (PDF, DOCX, etc.).
- Base64 encode/decode files and images.
- Download converted files directly from the browser.

### 4. **Web Scraper**
- Scrape web elements from specified URLs for automation testing.
- Review and export extracted elements for use in Page Object Models (POM).
- Mobile-friendly and responsive interface.

### 5. **Test Documents Library**
- Browse, filter, and view a range of test documents and guides.
- Inline PDF viewer for quick document previews.
- Filter by document name or category (Education, Framework, Support, Instructions).

### 6. **XML Comparison**
- Compare two XML files or text blocks.
- Highlight differences and merge XML documents.
- Download merged XML results.

### 7. **Contact Form**
- (Optional) Contact form for feedback and support (can be enabled/disabled in the UI).

---

## Installation

### Prerequisites
- [.NET 8.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
- Node.js (for front-end asset management, if you wish to modify JS/CSS)
- Visual Studio 2022+ or VS Code (recommended for development)

### Clone the Repository
```sh
git clone <your-repo-url>
cd TestTools
```

### Configuration
- Update `appsettings.json` and `appsettings.Development.json` to configure API endpoints and other settings.
- Place your test documents in the `Documents/` subfolders (`Education`, `Framework`, `Support`, `Instructions`).

### Build and Run
```sh
dotnet build
# For development
cd TestTools
# Launch the app (default: http://localhost:5000 or as configured)
dotnet run
```

Or use Visual Studio to run/debug the project directly.

---

## Usage

### Accessing the Application
- Open your browser and navigate to the local address (e.g., `http://localhost:5000`).
- Use the navigation tabs to access each tool/module.

### Patient Generator
- Select the number of patients to generate and click "Generate Patients".
- Export results as CSV or clear the table.
- Switch to the HL7 tab to generate and copy/save HL7 messages.

### Service Checker
- View all configured API endpoints and their status.
- Click "Check All Endpoints" to test all active endpoints.
- Use the metrics button to visualize response times and status.
- Export results for reporting.

### Document Conversion
- Convert files between supported formats.
- Use the Base64 tool for encoding/decoding files and images.

### Web Scraper
- Add URLs to scrape for web elements.
- Review and export the extracted elements for automation scripts.

### Test Documents
- Filter and view test documents by name or category.
- Click "View Document" to preview PDFs inline.

### XML Comparison
- Upload or paste two XML files/blocks.
- Compare, highlight differences, and merge as needed.
- Download merged XML.

---

## Project Structure

```
TestTools/
├── Controllers/           # ASP.NET Core MVC controllers
├── Documents/             # Test documents (PDFs, guides, etc.)
├── Models/                # Data models and view models
├── Services/              # Business logic and utility services
├── Views/                 # Razor views (UI)
│   ├── Home/              # Main tool pages
│   └── Shared/            # Shared partials and layouts
├── wwwroot/               # Static files (JS, CSS, images)
├── appsettings.json       # Main configuration
├── Program.cs             # App entry point
└── README.md              # This file
```

---

## Customization & Extensibility
- Add new API endpoints in `appsettings.json` under `ApiEndpoints`.
- Add new documents to the appropriate `Documents/` subfolder.
- Extend or modify tools by editing the corresponding controller, service, and view files.
- Front-end assets (JS/CSS) are in `wwwroot/`.

---

## Support & Feedback
For issues, feature requests, or support, contact Tom Connor:
- Email: [tom.connor@wales.nhs.uk](mailto:tom.connor@wales.nhs.uk)
- Microsoft Teams: [Open Chat](https://teams.microsoft.com/l/chat/0/0?users=tom.connor@wales.nhs.uk)

---

## License
This project is for internal DHCW use. Please contact the maintainer for licensing or external use inquiries.
