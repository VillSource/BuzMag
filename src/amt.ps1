param (
    [Parameter(Position = 0)]
    [string]$Context,

    [Parameter(Position = 1)]
    [string]$Name
)

[Console]::OutputEncoding = [System.Text.Encoding]::UTF8

# Project configuration paths
$ProjectPath        = ".\Host\Villsource.BuzMag.Migrations.PostgreSQL\"
$StartupProjectPath = ".\Host\Villsource.BuzMag.Api\"

# Trap Ctrl+C globally to ensure clean script exit
[Console]::TreatControlCAsInput = $true

# ==========================================
# 1. PRE-PROCESS CHECK: EF CORE & GIT VERIFICATION
# ==========================================
Write-Host "--- Pre-process System Check ---" -ForegroundColor Cyan

# Check Git Version
$gitCmd = Get-Command git -ErrorAction SilentlyContinue
if ($gitCmd) {
    $gitVersion = (git --version)
    Write-Host "[OK] Git Found: $gitVersion" -ForegroundColor Green
} else {
    Write-Error "Git is required to discover DbContext files. Please install Git and try again."
    exit 1
}

# Check EF Core Version
$efCheck = dotnet tool list -g 2>$null | Select-String "dotnet-ef"
if (-not $efCheck) {
    $efCheck = dotnet tool list 2>$null | Select-String "dotnet-ef"
}

if ($efCheck) {
    $efVersion = ($efCheck.ToString() -split '\s+')[1]
    Write-Host "[OK] EF Core Found Version: $efVersion" -ForegroundColor Green
} else {
    Write-Host "[WARN] 'dotnet-ef' tool not found. Running 'dotnet tool restore'..." -ForegroundColor Yellow
    dotnet tool restore
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Failed to restore dotnet tools. Please check your .config/dotnet-tools.json"
        exit 1
    }
    Write-Host "[OK] Dotnet tools restored successfully." -ForegroundColor Green
}

# ==========================================
# 2. LIST ALL DBCONTEXTS FOUND (GIT ONLY)
# ==========================================
Write-Host "`n--- Searching for DbContexts ---" -ForegroundColor Cyan

$extractedContexts = [System.Collections.Generic.List[string]]::new()

$gitFiles = git ls-files *DbContext.cs --cached --others --exclude-standard 2>$null

foreach ($path in $gitFiles) {
    if (-not [string]::IsNullOrWhiteSpace($path)) {
        $extractedContexts.Add([System.IO.Path]::GetFileNameWithoutExtension($path))
    }
}

$allContexts = $extractedContexts | Select-Object -Unique | Sort-Object

if ($allContexts.Count -eq 0) {
    Write-Host "[WARN] No *DbContext.cs files found by Git." -ForegroundColor Yellow
} else {
    Write-Host "Found $($allContexts.Count) DbContext(s):" -ForegroundColor Yellow
    foreach ($ctx in $allContexts) {
        Write-Host " - $ctx" -ForegroundColor White
    }
}

# ==========================================
# 3. CONTEXT & MIGRATION RESOLUTION
# ==========================================
Write-Host "`n--- Context & Migration Selection ---" -ForegroundColor Cyan

# Interactive Live Search + Fixed Height Window + Ctrl+C Trap
function Select-SearchableArrowMenu {
    param (
        [string[]]$Options,
        [string]$Title = "Select Target DbContext",
        [string]$InitialFilter = "",
        [int]$MaxVisibleItems = 8
    )

    $filterText = $InitialFilter
    $selectedIndex = 0
    [Console]::CursorVisible = $false
    $firstRun = $true

    $totalFixedLines = 3 + $MaxVisibleItems + 2

    try {
        while ($true) {
            # Filter options based on query
            $filteredOptions = @($Options | Where-Object { $_ -like "*$filterText*" })
            
            if ($filteredOptions.Count -eq 0) {
                $selectedIndex = 0
            } elseif ($selectedIndex -ge $filteredOptions.Count) {
                $selectedIndex = [Math]::Max(0, $filteredOptions.Count - 1)
            }

            # Calculate scrolling window
            $startIndex = [Math]::Max(0, [Math]::Min($selectedIndex - [int]($MaxVisibleItems / 2), $filteredOptions.Count - $MaxVisibleItems))
            if ($startIndex -lt 0) { $startIndex = 0 }
            $endIndex = [Math]::Min($startIndex + $MaxVisibleItems - 1, $filteredOptions.Count - 1)

            # Move cursor back strictly by fixed height
            if (-not $firstRun) {
                [Console]::SetCursorPosition(0, [Console]::CursorTop - $totalFixedLines)
            }
            $firstRun = $false

            # Render Header & Search Box
            Write-Host "┌── $Title ──────────────────────────────────────────────┐`e[K" -ForegroundColor DarkCyan
            Write-Host "│ " -NoNewline -ForegroundColor DarkCyan
            Write-Host "Search: " -NoNewline -ForegroundColor Yellow
            Write-Host "$filterText" -NoNewline -ForegroundColor White
            Write-Host "_`e[K" -ForegroundColor Gray
            Write-Host "├──────────────────────────────────────────────────────────────────┤`e[K" -ForegroundColor DarkCyan

            # Render Items
            $renderedCount = 0
            if ($filteredOptions.Count -gt 0) {
                for ($i = $startIndex; $i -le $endIndex; $i++) {
                    if ($i -eq $selectedIndex) {
                        Write-Host " > " -NoNewline -ForegroundColor Green
                        Write-Host "$($filteredOptions[$i])" -NoNewline -ForegroundColor Black -BackgroundColor Yellow
                        Write-Host "`e[K"
                    } else {
                        Write-Host "   $($filteredOptions[$i])`e[K" -ForegroundColor Gray
                    }
                    $renderedCount++
                }
            } else {
                Write-Host "   (No matching contexts found)`e[K" -ForegroundColor Red
                $renderedCount++
            }

            # Pad empty lines
            while ($renderedCount -lt $MaxVisibleItems) {
                Write-Host " `e[K"
                $renderedCount++
            }

            # Render Footer
            Write-Host "└──────────────────────────────────────────────────────────────────┘`e[K" -ForegroundColor DarkCyan
            Write-Host " (Type to filter | Up/Down Arrow | Enter to select | Ctrl+C to cancel)`e[K" -ForegroundColor DarkGray

            # Capture Key Press
            $key = [Console]::ReadKey($true)

            $isControlPressed = [bool]($key.Modifiers -band [ConsoleModifiers]::Control)
            
            if (($isControlPressed -and $key.Key -eq [ConsoleKey]::C) -or $key.Key -eq [ConsoleKey]::Escape) {
                Write-Host "`n`n[WARN] Operation canceled by user." -ForegroundColor Yellow
                exit 0
            }
            elseif ($key.Key -eq [ConsoleKey]::UpArrow) {
                if ($filteredOptions.Count -gt 0) {
                    $selectedIndex--
                    if ($selectedIndex -lt 0) { $selectedIndex = $filteredOptions.Count - 1 }
                }
            }
            elseif ($key.Key -eq [ConsoleKey]::DownArrow) {
                if ($filteredOptions.Count -gt 0) {
                    $selectedIndex++
                    if ($selectedIndex -ge $filteredOptions.Count) { $selectedIndex = 0 }
                }
            }
            elseif ($key.Key -eq [ConsoleKey]::Enter) {
                if ($filteredOptions.Count -gt 0) {
                    return $filteredOptions[$selectedIndex]
                }
            }
            elseif ($key.Key -eq [ConsoleKey]::Backspace) {
                if ($filterText.Length -gt 0) {
                    $filterText = $filterText.Substring(0, $filterText.Length - 1)
                }
            }
            elseif ([char]::IsLetterOrDigit($key.KeyChar) -or $key.KeyChar -eq '_') {
                $filterText += $key.KeyChar
            }
        }
    }
    finally {
        # Restore Console Cursor & Settings on Exit
        [Console]::CursorVisible = $true
        [Console]::TreatControlCAsInput = $false
    }
}

$selectedContext = $null

# Case 1: User did NOT pass Context
if ([string]::IsNullOrWhiteSpace($Context)) {
    Write-Host "[INFO] No Context specified. Opening selection menu..." -ForegroundColor Cyan
    $selectedContext = Select-SearchableArrowMenu -Options $allContexts -Title "Select Target DbContext"
}
# Case 2: Exact Match
elseif ($allContexts -contains $Context) {
    Write-Host "[OK] Context matched: $Context" -ForegroundColor Green
    $selectedContext = $Context
}
# Case 3: Fuzzy Match
else {
    Write-Host "[WARN] '$Context' was not matched directly." -ForegroundColor Yellow
    $searchKey = $Context -replace 'DbContext$', ''
    $selectedContext = Select-SearchableArrowMenu -Options $allContexts -Title "Search DbContext" -InitialFilter $searchKey
}

if ([string]::IsNullOrWhiteSpace($selectedContext)) {
    Write-Error "Context selection was canceled."
    exit 1
}

# Prompt for Migration Name if not provided
if ([string]::IsNullOrWhiteSpace($Name)) {
    Write-Host ""
    $Name = Read-Host "Enter Migration Name (e.g., AddOrganizationModule) :"
}

if ([string]::IsNullOrWhiteSpace($Name)) {
    Write-Error "Migration Name is required."
    exit 1
}

$OutputDir = $selectedContext -replace 'DbContext$', ''

# ==========================================
# 4. SUMMARY & EXECUTION (DYNAMIC TABLE WIDTH)
# ==========================================

# 1. คำนวณความยาวสูงสุดของข้อมูลทั้งหมด
$values = @($selectedContext, $Name, $OutputDir, $ProjectPath, $StartupProjectPath)
$maxValLen = 0
foreach ($v in $values) {
    if ($v.Length -gt $maxValLen) { $maxValLen = $v.Length }
}

# ความกว้างของ Prefix ("│ Context Name   : ") = 19 ตัวอักษร
$prefixLen = 19
$contentWidth = [Math]::Max($maxValLen, 30)

# ความกว้างรวมของตาราง
$boxWidth = $prefixLen + $contentWidth + 1
$lineBorder = "─" * ($boxWidth - 2)

# คำนวณเพื่อจัดข้อความ Title ให้อยู่กึ่งกลางแบบใช้มาตรฐาน .NET Strings
$headerText = "EF CORE MIGRATION SUMMARY"
$headerAvailableWidth = $boxWidth - 2
$padTotal = $headerAvailableWidth - $headerText.Length
$padLeft = [Math]::Max(0, [int]($padTotal / 2))
$centeredHeader = (" " * $padLeft) + $headerText
$centeredHeader = $centeredHeader.PadRight($headerAvailableWidth)

Write-Host "`n┌$lineBorder┐" -ForegroundColor Cyan
Write-Host "│" -NoNewline -ForegroundColor Cyan
Write-Host $centeredHeader -ForegroundColor Cyan -NoNewline
Write-Host "│" -ForegroundColor Cyan
Write-Host "├$lineBorder┤" -ForegroundColor Cyan

Write-Host "│ Context Name   : " -NoNewline -ForegroundColor White
Write-Host ("{0,-$contentWidth}" -f $selectedContext) -ForegroundColor Yellow -NoNewline
Write-Host "│" -ForegroundColor Cyan

Write-Host "│ Migration Name : " -NoNewline -ForegroundColor White
Write-Host ("{0,-$contentWidth}" -f $Name) -ForegroundColor Yellow -NoNewline
Write-Host "│" -ForegroundColor Cyan

Write-Host "│ Output Dir     : " -NoNewline -ForegroundColor White
Write-Host ("{0,-$contentWidth}" -f $OutputDir) -ForegroundColor Green -NoNewline
Write-Host "│" -ForegroundColor Cyan

Write-Host "│ Project        : " -NoNewline -ForegroundColor White
Write-Host ("{0,-$contentWidth}" -f $ProjectPath) -ForegroundColor Gray -NoNewline
Write-Host "│" -ForegroundColor Cyan

Write-Host "│ Startup Proj   : " -NoNewline -ForegroundColor White
Write-Host ("{0,-$contentWidth}" -f $StartupProjectPath) -ForegroundColor Gray -NoNewline
Write-Host "│" -ForegroundColor Cyan

Write-Host "└$lineBorder┘" -ForegroundColor Cyan

$confirmRun = Read-Host "`nExecute 'dotnet ef migrations add'? (Y/n) :"

if ($confirmRun -eq '' -or $confirmRun -match '^[Yy]') {
    Write-Host "[INFO] Adding EF Core migration..." -ForegroundColor Cyan
    dotnet ef migrations add $Name `
        --project $ProjectPath `
        --startup-project $StartupProjectPath `
        --context $selectedContext `
        --output-dir $OutputDir

    if ($LASTEXITCODE -eq 0) {
        Write-Host "[OK] Migration '$Name' added successfully!" -ForegroundColor Green
    } else {
        Write-Host "[FAIL] Failed to add migration. Check error log above." -ForegroundColor Red
    }
} else {
    Write-Host "[WARN] Operation canceled by user." -ForegroundColor Yellow
}