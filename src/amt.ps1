param (
    [Parameter(Position = 0)]
    [string]$Context,

    [Parameter(Position = 1)]
    [string]$Name
)

# Configuration: Project paths
$ProjectPath = ".\Host\Villsource.BuzMag.Migrations.PostgreSQL\"
$StartupProjectPath = ".\Host\Villsource.BuzMag.Api\"

# 1. Check if dotnet-ef is installed
$efCheck = dotnet tool list -g | Select-String "dotnet-ef"
if (-not $efCheck) {
    # Check in local tool manifest
    $efCheck = dotnet tool list | Select-String "dotnet-ef"
}

if ($efCheck) {
    # Extract and display the version number
    $efVersion = ($efCheck.ToString() -split '\s+')[1]
    Write-Host "[INFO] Found dotnet-ef Version: $efVersion" -ForegroundColor Green
} else {
    Write-Host "[WARNING] dotnet-ef is not installed." -ForegroundColor Yellow
    $confirmRestore = Read-Host "Do you want to run 'dotnet tool restore' now? (Y/n) :"
    
    if ($confirmRestore -eq '' -or $confirmRestore -match '^[Yy]') {
        dotnet tool restore
        if ($LASTEXITCODE -ne 0) {
            Write-Error "Failed to restore dotnet tools. Please check your manifest file (.config/dotnet-tools.json)."
            exit 1
        }
    } else {
        Write-Error "Execution canceled. 'dotnet-ef' is required to run EF Core migrations."
        exit 1
    }
}

# 2. Check Context; prompt if missing
if ([string]::IsNullOrWhiteSpace($Context)) {
    $Context = Read-Host "Enter Context Name (e.g., OrganizationDbContext) :"
}

# 3. Check Migration Name; prompt if missing
if ([string]::IsNullOrWhiteSpace($Name)) {
    $Name = Read-Host "Enter Migration Name (e.g., AddOrganizationModule) :"
}

# Ensure both inputs are provided
if ([string]::IsNullOrWhiteSpace($Context) -or [string]::IsNullOrWhiteSpace($Name)) {
    Write-Error "Error: Both Context and Migration Name are required."
    exit 1
}

# 4. Strip "DbContext" suffix from Context name for the Output Dir
$OutputDir = $Context -replace 'DbContext$', ''

Write-Host "----------------------------------------" -ForegroundColor Cyan
Write-Host "Context Name   : $Context"
Write-Host "Migration Name : $Name"
Write-Host "Output Dir     : $OutputDir"
Write-Host "Project        : $ProjectPath"
Write-Host "Startup Project: $StartupProjectPath"
Write-Host "----------------------------------------" -ForegroundColor Cyan

# 5. Execute dotnet ef migrations add
dotnet ef migrations add $Name `
    --project $ProjectPath `
    --startup-project $StartupProjectPath `
    --context $Context `
    --output-dir $OutputDir