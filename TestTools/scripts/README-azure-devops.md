Azure DevOps automation helper
=================================

What this does
---------------
- A PowerShell script `azure-devops-create.ps1` reads markdown files from `TestTools.Docs` and creates Epics, Features, Tasks and Test Cases in the Azure DevOps project.

Security: create a PAT
----------------------
1. Sign in to Azure DevOps (https://dev.azure.com/DHCW-ADS).
2. Click your profile (top right) -> 'Personal access tokens'.
3. Click 'New Token'.
   - Name: e.g., TestTools-Import
   - Organization: DHCW-ADS
   - Expiration: pick appropriate expiry
   - Scopes: set these minimums:
     - Work Items (Read & write)
     - Test Management (Read & write)
     - (Optionally) Code Read & Write if you want repository operations
4. Create token and copy it. Keep it secret.

How to run the script
---------------------
Open PowerShell (Windows) and run:

```powershell
cd <repo_root>\TestTools\scripts
# Option A: provide PAT via environment variable (preferred for automation)
$env:AZDO_PAT = '<yourPAT>'
.\azure-devops-create.ps1 -OrgUrl 'https://dev.azure.com/DHCW-ADS' -Project 'MCP-AI-POC' -DocsPath '..\TestTools.Docs' -DryRun

# Option B: run interactively and paste PAT when prompted
.\azure-devops-create.ps1 -OrgUrl 'https://dev.azure.com/DHCW-ADS' -Project 'MCP-AI-POC' -DocsPath '..\TestTools.Docs'
```

Run with `-DryRun` to preview API calls and payloads.

About the `mcp` azure-devops helper
----------------------------------
This workspace has an `mcp` set of helper functions available via the agent environment. There are Azure DevOps related functions available (for example: repo and work item helpers). Using the agent's mcp Azure DevOps wrappers can simplify authentication and batching, but they require the agent to have network access and credentials configured. If you prefer that route, you can:

- Inspect the available mcp azure-devops functions in the environment (the agent may provide functions named like `mcp_azure-devops_repo_*`, `mcp_azure-devops_wit_*`, `mcp_azure-devops_testplan_*`).
- They map closely to the REST APIs used in this script. If you'd like, I can produce an alternative script using those helpers — tell me and I will convert the calls.

Limitations & next steps
------------------------
- The markdown parsing is simple: H1 => Epic, H2 => Feature, H3 => Task, Gherkin 'Scenario' => Test Case. If your docs use different headings, update the script.
- Test Case steps are stored as simple HTML in the `Microsoft.VSTS.TCM.Steps` field; complex rich formatting or parameterized Scenario Outlines may need additional handling.
- I recommend running with `-DryRun` first and verifying created items in the Azure DevOps web UI.

If you want, I can:
- Convert this script to use mcp azure-devops helper functions instead of raw REST calls.
- Extend the parser to handle more heading levels, tags, acceptance criteria, and automatic area/iteration mapping.
