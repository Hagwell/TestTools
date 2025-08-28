# DHCW Test Tools – Test Plan (Gherkin Syntax)

## Patient Generator

```gherkin
Feature: Patient Data Generation
  Scenario: Generate a set of random patients
    Given I am on the Patient Generator page
    When I enter "10" as the number of patients
    And I click "Generate Patients"
    Then I should see a table with 10 patients

  Scenario: Export generated patients as CSV
    Given I have generated patients
    When I click "Export CSV"
    Then a CSV file should be downloaded

  Scenario: Generate HL7 message
    Given I am on the Master Patient (HL7) tab
    When I click "Generate Master Patient Index Message"
    Then I should see an HL7 message displayed

  Scenario: Clear HL7 message
    Given an HL7 message is displayed
    When I click "Clear Message"
    Then the HL7 message area should be empty
```

## Service Checker

```gherkin
Feature: API Endpoint Monitoring
  Scenario: Check all active endpoints
    Given I am on the Service Checker page
    When I click "Check All Endpoints"
    Then only endpoints with Active = true are checked
    And I see their status and response times

  Scenario: View metrics chart
    Given endpoints have been checked
    When I click "View Metrics"
    Then I see a chart of response times for active endpoints

  Scenario: Export results
    Given endpoints have been checked
    When I click "Export Results"
    Then a file with results is downloaded
```

## Document Conversion

```gherkin
Feature: Document Conversion
  Scenario: Convert a PDF to DOCX
    Given I am on the Document Conversion page
    When I upload a PDF file
    And I select DOCX as the output format
    And I click "Convert"
    Then a DOCX file is downloaded

  Scenario: Base64 encode a file
    Given I am on the Base64 Conversion page
    When I upload a file
    And I click "Encode"
    Then I see the Base64 string
```

## Web Scraper

```gherkin
Feature: Web Element Scraping
  Scenario: Scrape elements from a URL
    Given I am on the Web Scraper page
    When I add a valid URL
    And I click "Scrape Web Pages"
    Then I see a table of extracted elements

  Scenario: Export POM
    Given elements have been scraped
    When I click "Generate POM"
    Then a file is downloaded
```

## Test Documents Library

```gherkin
Feature: Test Document Browsing
  Scenario: Filter documents by name
    Given I am on the Test Documents page
    When I enter a filter term
    Then only matching documents are shown

  Scenario: View document inline
    Given I am on the Test Documents page
    When I click "View Document"
    Then the document is displayed inline
```

## XML Comparison

```gherkin
Feature: XML Comparison
  Scenario: Compare two XML files
    Given I am on the XML Compare page
    When I upload two XML files
    And I click "Compare"
    Then differences are highlighted

  Scenario: Merge XML files
    Given two XML files are loaded
    When I click "Merge"
    Then a merged XML is displayed
    And I can download the merged XML
```

# Additional Edge Cases, Negative Tests, Integration, and Cross-Tool Flows

## Patient Generator (Edge/Negative)
```gherkin
  Scenario: Generate zero patients
    Given I am on the Patient Generator page
    When I enter "0" as the number of patients
    And I click "Generate Patients"
    Then I should see a validation error

  Scenario: Enter invalid number of patients
    Given I am on the Patient Generator page
    When I enter "abc" as the number of patients
    And I click "Generate Patients"
    Then I should see a validation error

  Scenario: Export patients before generation
    Given I am on the Patient Generator page
    When I click "Export CSV"
    Then I should see a message indicating no data to export
```

## Service Checker (Edge/Negative)
```gherkin
  Scenario: Check endpoints when none are active
    Given all endpoints are inactive
    When I click "Check All Endpoints"
    Then I should see a message indicating no endpoints to check

  Scenario: Endpoint returns error
    Given an endpoint is active but returns an error
    When I check endpoints
    Then the error is displayed in the results
```

## Document Conversion (Edge/Negative)
```gherkin
  Scenario: Upload unsupported file type
    Given I am on the Document Conversion page
    When I upload a .exe file
    And I click "Convert"
    Then I should see an error message about unsupported file type

  Scenario: Convert with no file uploaded
    Given I am on the Document Conversion page
    When I click "Convert"
    Then I should see a validation error
```

## Web Scraper (Edge/Negative)
```gherkin
  Scenario: Scrape with invalid URL
    Given I am on the Web Scraper page
    When I add "not-a-url" as the URL
    And I click "Scrape Web Pages"
    Then I should see a validation error

  Scenario: Scrape with unreachable URL
    Given I am on the Web Scraper page
    When I add a URL that does not exist
    And I click "Scrape Web Pages"
    Then I should see an error message about network failure
```

## Integration & Cross-Tool Flows
```gherkin
  Scenario: Use generated patient data in document conversion
    Given I have generated patients
    When I export patients as CSV
    And I upload the CSV to the Document Conversion tool
    Then the file is accepted and processed

  Scenario: Use scraped data in Service Checker
    Given I have scraped URLs from the Web Scraper
    When I add these URLs as endpoints in Service Checker
    And I check endpoints
    Then I see their status and response times
```

## Test Documents Library (Edge/Negative)
```gherkin
  Scenario: Filter with no matching documents
    Given I am on the Test Documents page
    When I enter a filter term that matches nothing
    Then I see a message indicating no documents found

  Scenario: View document with unsupported format
    Given I am on the Test Documents page
    When I click "View Document" for an unsupported file type
    Then I see an error message
```

## XML Comparison (Edge/Negative)
```gherkin
  Scenario: Compare two identical XML files
    Given I upload two identical XML files
    When I click "Compare"
    Then I see a message indicating the files are identical

  Scenario: Compare XML files with different structures
    Given I upload two XML files with different structures
    When I click "Compare"
    Then I see a detailed difference report

  Scenario: Compare with one file missing
    Given I upload only one XML file
    When I click "Compare"
    Then I see a validation error
```
