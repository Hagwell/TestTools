# DHCW Test Tools – Gherkin Test Plan (Future Scenarios)

```gherkin
Feature: Patient Data Generation with API and Uniqueness
  Scenario: Generate patient with unique NHS number
    Given I request to generate a patient
    When the system checks MongoDB for existing NHS numbers
    And the NHS number is unique
    Then the patient is generated and stored in MongoDB

  Scenario: Attempt to generate duplicate NHS number
    Given an NHS number already exists in MongoDB
    When I request to generate a patient
    Then the system generates a new unique NHS number

Feature: HL7 Message Registration
  Scenario: Admin registers HL7 message
    Given I am signed in as an Admin
    And I have generated an HL7 message
    When I click "Register"
    Then the message is submitted to the web service
    And the status is recorded in MongoDB

Feature: TestDocs Base64 PDF Upload
  Scenario: Upload PDF as base64
    Given I am signed in as an Admin
    When I upload a base64-encoded PDF
    Then the PDF is stored in MongoDB
    And it can be viewed inline

Feature: Service Checker with MongoDB Endpoints
  Scenario: Display only active endpoints
    Given endpoints are stored in MongoDB
    When I view the Service Checker
    Then only endpoints with Active = true are shown

Feature: XML Compare Full Diff
  Scenario: Show highlighted differences
    Given I upload two XML files
    When I click "Compare"
    Then a full diff view with highlights is displayed

Feature: File Converter Progress Feedback
  Scenario: Show progress during conversion
    Given I upload a file for conversion
    When I click "Convert"
    Then a loading GIF and progress bar are shown until conversion completes

Feature: Web Scraper with Authenticated Crawl
  Scenario: Scrape entire site with authentication
    Given I have stored credentials for a site
    When I start a site-wide scrape
    Then the scraper logs in and extracts elements from all pages

Feature: Admin Authentication and Permissions
  Scenario: Admin can manage endpoints and documents
    Given I am signed in as an Admin
    When I access the management UI
    Then I can add, edit, or delete endpoints and documents

Feature: Security and Audit Logging
  Scenario: Audit admin actions
    Given I perform an admin action
    Then the action is logged with user, timestamp, and details
```

---

## Additional Recommendations for Extensibility
- API Rate Limiting & Throttling
- Bulk Operations for documents and endpoints
- Notifications for long-running tasks
- Versioning for documents and endpoints
- User Preferences and Theming
- Mobile Responsiveness
- Comprehensive Logging and Log Viewer
- Disaster Recovery Drills
