<#
.SYNOPSIS
  Create Azure DevOps Epics, Features, Tasks, Test Plans and Test Cases from markdown docs.

USAGE
  - Run interactively and provide a PAT when prompted, or set environment variable AZDO_PAT.
  - Example: .\azure-devops-create.ps1 -OrgUrl 'https://dev.azure.com/DHCW-ADS' -Project 'MCP-AI-POC' -DocsPath '.\TestTools.Docs' -DryRun

NOTES
  - This script uses Azure DevOps REST API. Review and run in DryRun mode first.
  - Provide a PAT with Work Items (Read & Write) and Test Management scopes.
#>

param(
    [string]$OrgUrl = 'https://dev.azure.com/DHCW-ADS',
    [string]$Project = 'MCP-AI-POC',
    [string]$DocsPath = "$PSScriptRoot\..\TestTools.Docs",
    [switch]$DryRun
)

function Get-Pat {
    if ($env:AZDO_PAT) {
        return $env:AZDO_PAT
    }
    Write-Host "Enter a Personal Access Token (PAT) with Work Items & Test Management scopes:" -ForegroundColor Yellow
    $sec = Read-Host -AsSecureString
    return [System.Runtime.InteropServices.Marshal]::PtrToStringAuto([System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($sec))
}

function Build-AuthHeader($pat) {
    $pair = ':' + $pat
    $b = [System.Text.Encoding]::UTF8.GetBytes($pair)
    $base64 = [Convert]::ToBase64String($b)
    return "Basic $base64"
}

function Invoke-AdoApi {
    param(
        [string]$Method,
        [string]$Url,
        [hashtable]$Headers,
        $Body = $null,
        [string]$ContentType = 'application/json'
    )

    Write-Host "API: $Method $Url" -ForegroundColor Cyan
    if ($DryRun) {
        if ($Body) { Write-Host "Body:"; $Body | ConvertTo-Json -Depth 5 | Write-Host }
        return $null
    }

    try {
        if ($Body -ne $null) {
            $json = $Body
            $response = Invoke-RestMethod -Method $Method -Uri $Url -Headers $Headers -ContentType $ContentType -Body ($json)
        } else {
            $response = Invoke-RestMethod -Method $Method -Uri $Url -Headers $Headers -ContentType $ContentType
        }
        return $response
    } catch {
        Write-Error "Request failed: $($_.Exception.Message)"
        return $null
    }
}

function New-WorkItem {
    param(
        [string]$Type, # e.g., Epic, Feature, Task, 'Test Case'
        [string]$Title,
        [string]$Description,
        [string[]]$AdditionalFields
    )

    $url = "$OrgUrl/$Project/_apis/wit/workitems/`$$Type?api-version=7.1-preview.3"
    $ops = @()
    $ops += @{ op = 'add'; path = '/fields/System.Title'; value = $Title }
    if ($Description) { $ops += @{ op = 'add'; path = '/fields/System.Description'; value = $Description } }
    foreach ($f in $AdditionalFields) { $ops += $f }

    $headers = @{ Authorization = $global:AuthHeader }
    return Invoke-AdoApi -Method Patch -Url $url -Headers $headers -Body ($ops | ConvertTo-Json -Depth 10) -ContentType 'application/json-patch+json'
}

function New-TestPlan {
    param(
        [string]$Name
    )
    $url = "$OrgUrl/$Project/_apis/test/plans?api-version=7.0"
    $body = @{ name = $Name } | ConvertTo-Json
    $headers = @{ Authorization = $global:AuthHeader }
    return Invoke-AdoApi -Method Post -Url $url -Headers $headers -Body $body
}

function New-TestSuite {
    param(
        [int]$PlanId,
        [string]$Name
    )
    $url = "$OrgUrl/$Project/_apis/test/plans/$PlanId/suites?api-version=7.0"
    $body = @{ suiteType = 'Static'; name = $Name } | ConvertTo-Json
    $headers = @{ Authorization = $global:AuthHeader }
    return Invoke-AdoApi -Method Post -Url $url -Headers $headers -Body $body
}

function New-TestCaseWorkItem {
    param(
        [string]$Title,
        [string]$StepsHtml # Azure expects steps in field Microsoft.VSTS.TCM.Steps as HTML
    )
    $url = "$OrgUrl/$Project/_apis/wit/workitems/`$Test%20Case?api-version=7.1-preview.3"
    # Build patch ops
    $ops = @()
    $ops += @{ op = 'add'; path = '/fields/System.Title'; value = $Title }
    if ($StepsHtml) { $ops += @{ op = 'add'; path = '/fields/Microsoft.VSTS.TCM.Steps'; value = $StepsHtml } }
    $headers = @{ Authorization = $global:AuthHeader }
    return Invoke-AdoApi -Method Patch -Url $url -Headers $headers -Body ($ops | ConvertTo-Json -Depth 10) -ContentType 'application/json-patch+json'
}

function Add-TestCaseToSuite {
    param(
        [int]$PlanId,
        [int]$SuiteId,
        [int]$TestCaseId
    )
    $url = "$OrgUrl/$Project/_apis/test/plans/$PlanId/suites/$SuiteId/testcases/$TestCaseId?api-version=7.0"
    $headers = @{ Authorization = $global:AuthHeader }
    return Invoke-AdoApi -Method Post -Url $url -Headers $headers
}

function Convert-GherkinToHtmlSteps {
    param(
        [string[]]$Lines
    )
    # Simple converter: wrap lines as ordered steps grouped by Given/When/Then
    $html = "<steps xmlns=\"http://microsoft.com/schemas/VisualStudio/TeamTest/2010\"><step><description>" + [System.Web.HttpUtility]::HtmlEncode(($Lines -join "\n")) + "</description></step></steps>"
    return $html
}

### Main
Write-Host "Azure DevOps automation: will parse docs from $DocsPath and create work items/test artifacts in $OrgUrl/$Project" -ForegroundColor Green

$Pat = Get-Pat
if (-not $Pat) { Write-Error "PAT required"; exit 1 }
$global:AuthHeader = Build-AuthHeader $Pat

if (-not (Test-Path $DocsPath)) {
    Write-Error "Docs path not found: $DocsPath"
    exit 1
}

# Read markdown files
$mdFiles = Get-ChildItem -Path $DocsPath -Filter *.md -File

$currentEpic = $null
$currentFeature = $null

foreach ($f in $mdFiles) {
    Write-Host "Parsing $($f.Name)" -ForegroundColor Yellow
    $lines = Get-Content $f.FullName
    for ($i = 0; $i -lt $lines.Count; $i++) {
        $line = $lines[$i].Trim()
        if ($line -match '^# (.+)') {
            $epicTitle = $matches[1].Trim()
            Write-Host "Found Epic: $epicTitle"
            $currentEpic = @{ title = $epicTitle; id = $null }
            $res = New-WorkItem -Type Epic -Title $epicTitle -Description "Generated from $($f.Name)"
            if ($res -ne $null) { $currentEpic.id = $res.id }
        } elseif ($line -match '^## (.+)') {
            $featTitle = $matches[1].Trim()
            Write-Host " Found Feature: $featTitle"
            $currentFeature = @{ title = $featTitle; id = $null }
            $desc = "Child of Epic: $($currentEpic.title)" 
            $res = New-WorkItem -Type Feature -Title $featTitle -Description $desc -AdditionalFields @()
            if ($res -ne $null) { $currentFeature.id = $res.id
                # If we have an epic id, add relation to parent
                if ($currentEpic.id) {
                    Write-Host " Linking Feature $($currentFeature.id) to Epic $($currentEpic.id)" -ForegroundColor Gray
                    if (-not $DryRun) {
                        $linkOps = @(@{ op = 'add'; path = '/relations/-'; value = @{ rel = 'System.LinkTypes.Hierarchy-Reverse'; url = "$OrgUrl/_apis/wit/workItems/$($currentEpic.id)" } })
                        $url = "$OrgUrl/$Project/_apis/wit/workitems/`$Feature?api-version=7.1-preview.3"
                        Invoke-AdoApi -Method Patch -Url "$OrgUrl/$Project/_apis/wit/workitems/$($currentFeature.id)?api-version=7.1-preview.3" -Headers @{ Authorization = $global:AuthHeader } -Body ($linkOps | ConvertTo-Json -Depth 10) -ContentType 'application/json-patch+json'
                    }
                }
            }
        } elseif ($line -match '^### (.+)') {
            $taskTitle = $matches[1].Trim()
            Write-Host "  Found Task: $taskTitle"
            $desc = "Auto-created task under feature $($currentFeature.title)"
            $res = New-WorkItem -Type Task -Title $taskTitle -Description $desc
            if ($res -ne $null -and $currentFeature.id) {
                Write-Host "   Linking Task to Feature" -ForegroundColor Gray
                # link task to feature
                if (-not $DryRun) {
                    $linkOps = @(@{ op = 'add'; path = '/relations/-'; value = @{ rel = 'System.LinkTypes.Hierarchy-Forward'; url = "$OrgUrl/_apis/wit/workItems/$($currentFeature.id)" } })
                    Invoke-AdoApi -Method Patch -Url "$OrgUrl/$Project/_apis/wit/workitems/$($res.id)?api-version=7.1-preview.3" -Headers @{ Authorization = $global:AuthHeader } -Body ($linkOps | ConvertTo-Json -Depth 10) -ContentType 'application/json-patch+json'
                }
            }
        } elseif ($line -match '^(Scenario:|Scenario Outline:)(.+)$') {
            # collect the scenario block until blank
            $scenarioTitle = $matches[2].Trim()
            $j = $i + 1
            $scenarioLines = @()
            while ($j -lt $lines.Count -and $lines[$j].Trim() -ne '') {
                $scenarioLines += $lines[$j].Trim()
                $j++
            }
            $i = $j
            Write-Host " Found Scenario: $scenarioTitle"
            $stepsHtml = Convert-GherkinToHtmlSteps -Lines $scenarioLines
            $tc = New-TestCaseWorkItem -Title $scenarioTitle -StepsHtml $stepsHtml
            if ($tc -ne $null) { Write-Host " Created Test Case id: $($tc.id)" -ForegroundColor Green }
        }
    }
}

Write-Host "Done." -ForegroundColor Green
