#!/usr/bin/env powershell
# Cache Diagnostic Tool for Kido Teacher App
# Người dùng có thể chạy script này để kiểm tra cache database

param(
    [switch]$AutoFix = $false
)

# Get database path
$appDataPath = [Environment]::GetFolderPath("ApplicationData")
$dbPath = Join-Path $appDataPath "KidoTeacherApp" "app_cache.db"
$lecturesPath = Join-Path $appDataPath "KidoTeacherApp" "Lectures"

Write-Host "========== CACHE DIAGNOSTIC ==========" -ForegroundColor Cyan
Write-Host "Time: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')" -ForegroundColor Gray
Write-Host ""

# Check if database exists
if (-not (Test-Path $dbPath)) {
    Write-Host "✓ Database not found (first time setup)" -ForegroundColor Yellow
    exit 0
}

# Get database info
$dbSize = (Get-Item $dbPath).Length
Write-Host "Database path: $dbPath" -ForegroundColor Gray
Write-Host "Database size: $($dbSize / 1MB -as [int]) MB" -ForegroundColor Gray
Write-Host "Lectures path: $lecturesPath" -ForegroundColor Gray
Write-Host ""

# Check for SQLite
try {
    $sqliteModule = Get-Command sqlite3 -ErrorAction Stop
    Write-Host "SQLite found: $(($sqliteModule).Source)" -ForegroundColor Green
} catch {
    Write-Host "⚠ SQLite not found. Installing..." -ForegroundColor Yellow
    try {
        choco install sqlite -y | Out-Null
        Write-Host "✓ SQLite installed" -ForegroundColor Green
    } catch {
        Write-Host "✗ Cannot install SQLite. Please install manually." -ForegroundColor Red
        Write-Host "  Download: https://www.sqlite.org/download.html" -ForegroundColor Gray
        exit 1
    }
}

Write-Host ""
Write-Host "Scanning database..." -ForegroundColor Cyan

# Query database
$query = @"
SELECT lecture_id, 
       COALESCE(pdf_path, '') as pdf_path,
       COALESCE(video_path, '') as video_path,
       COALESCE(elearning_path, '') as elearning_path,
       COALESCE(powerpoint_path, '') as powerpoint_path
FROM offline_lecture_cache;
"@

$entries = @()
$corruptedCount = 0
$validCount = 0

try {
    $results = sqlite3 $dbPath $query
    
    foreach ($line in $results) {
        $fields = $line -split '\|'
        if ($fields.Count -ge 5) {
            $lectureId = $fields[0]
            $pdfPath = $fields[1]
            $videoPath = $fields[2]
            $elearningPath = $fields[3]
            $powerpointPath = $fields[4]
            
            $issues = @()
            
            # Check for encoding corruption
            if ($pdfPath -match '[À-ÿ]|Â|º|ß|ƒ|€|'|'|"|"') {
                $issues += "PDF has encoding issue"
            }
            if ($videoPath -match '[À-ÿ]|Â|º|ß|ƒ|€|'|'|"|"') {
                $issues += "Video has encoding issue"
            }
            if ($elearningPath -match '[À-ÿ]|Â|º|ß|ƒ|€|'|'|"|"') {
                $issues += "Elearning has encoding issue"
            }
            if ($powerpointPath -match '[À-ÿ]|Â|º|ß|ƒ|€|'|'|"|"') {
                $issues += "PowerPoint has encoding issue"
            }
            
            # Check if files exist
            if ($pdfPath -and -not (Test-Path $pdfPath)) {
                $issues += "PDF file missing"
            }
            if ($videoPath -and -not (Test-Path $videoPath)) {
                $issues += "Video file missing"
            }
            if ($elearningPath -and -not (Test-Path $elearningPath)) {
                $issues += "Elearning file missing"
            }
            if ($powerpointPath -and -not (Test-Path $powerpointPath)) {
                $issues += "PowerPoint file missing"
            }
            
            if ($issues.Count -gt 0) {
                $corruptedCount++
                Write-Host "✗ $lectureId" -ForegroundColor Red
                foreach ($issue in $issues) {
                    Write-Host "  - $issue" -ForegroundColor Red
                }
            } else {
                $validCount++
                Write-Host "✓ $lectureId" -ForegroundColor Green
            }
        }
    }
} catch {
    Write-Host "✗ Error querying database: $_" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "Summary:" -ForegroundColor Cyan
Write-Host "  Valid entries: $validCount" -ForegroundColor Green
Write-Host "  Corrupted entries: $corruptedCount" -ForegroundColor Red
Write-Host ""

if ($corruptedCount -gt 0) {
    Write-Host "⚠ Found corrupted cache entries!" -ForegroundColor Red
    Write-Host ""
    
    if ($AutoFix) {
        Write-Host "Auto-fixing database..." -ForegroundColor Yellow
    } else {
        $response = Read-Host "Do you want to backup and reset the database? (Y/N)"
        if ($response -eq "Y" -or $response -eq "y") {
            $AutoFix = $true
        }
    }
    
    if ($AutoFix) {
        # Backup database
        $backupPath = "$dbPath.backup.$(Get-Date -Format 'yyyyMMdd_HHmmss')"
        Copy-Item $dbPath $backupPath -ErrorAction Stop
        Write-Host "✓ Database backed up to: $backupPath" -ForegroundColor Green
        
        # Delete database
        Remove-Item $dbPath -Force -ErrorAction Stop
        Write-Host "✓ Corrupted database deleted" -ForegroundColor Green
        
        Write-Host ""
        Write-Host "Done! Please restart the Kido Teacher app." -ForegroundColor Green
    }
} else {
    Write-Host "✓ Database is healthy!" -ForegroundColor Green
}

Write-Host ""
Write-Host "========== END DIAGNOSTIC ==========" -ForegroundColor Cyan
