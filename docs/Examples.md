# ConsoleDocument Examples

## PowerShell Examples

### Basic Report
```powershell
Import-Module ./ConsoleDocument.PowerShell/bin/Debug/net9.0/ConsoleDocument.PowerShell.dll

New-ConsoleDocument -Width 80 -EnableVT |
    Add-ConsoleHeader -Text "Status Report" -PassThru |
    Add-ConsolePanel -Title "Info" -Text "Everything is working!" -PassThru |
    Add-ConsoleFooter -Text "Done" -PassThru |
    Write-ConsoleDocument
```

### System Monitoring Dashboard
```powershell
# Get real system data
$cpu = (Get-Counter '\Processor(_Total)\% Processor Time').CounterSamples.CookedValue
$mem = (Get-Counter '\Memory\% Committed Bytes In Use').CounterSamples.CookedValue
$processes = (Get-Process | Measure-Object).Count

New-ConsoleDocument -Width 100 -EnableVT |
    Add-ConsoleHeader -Text "SYSTEM MONITOR" -PassThru |
    Add-ConsoleTable -Title "Quick Stats" -Columns @("Metric", "Value") -Rows @(
        @("CPU Usage", "$([math]::Round($cpu,1))%"),
        @("Memory Usage", "$([math]::Round($mem,1))%"),
        @("Processes", "$processes")
    ) -PassThru |
    Add-ConsoleBarGraph -Title "Resource Visualization" -Segments @(
        @{ Text = "CPU"; Value = [int]$cpu; Color = $(if($cpu -gt 80){"Red"}elseif($cpu -gt 50){"Yellow"}else{"Green"}) },
        @{ Text = "Memory"; Value = [int]$mem; Color = $(if($mem -gt 80){"Red"}elseif($mem -gt 50){"Yellow"}else{"Green"}) }
    ) -PassThru |
    Add-ConsoleFooter -Text "Updated: $(Get-Date -Format 'HH:mm:ss')" -PassThru |
    Write-ConsoleDocument
```

### Folder Tree Visualization
```powershell
function Get-FolderTree {
    param([string]$Path, [int]$MaxDepth = 3, [int]$CurrentDepth = 0)
    
    $item = Get-Item $Path
    $node = @{ Text = $item.Name; Children = @() }
    
    if ($CurrentDepth -lt $MaxDepth -and (Test-Path $Path -PathType Container)) {
        $children = Get-ChildItem $Path -Directory -ErrorAction SilentlyContinue | Select-Object -First 5
        foreach ($child in $children) {
            $node.Children += Get-FolderTree -Path $child.FullName -MaxDepth $MaxDepth -CurrentDepth ($CurrentDepth + 1)
        }
    }
    
    return $node
}

$tree = Get-FolderTree -Path "C:\Users\$env:USERNAME\Documents" -MaxDepth 2

New-ConsoleDocument -Width 100 -EnableVT |
    Add-ConsoleTreeDiagram -Title "Documents Folder Structure" -RootNode $tree -PassThru |
    Write-ConsoleDocument
```

### Git Repository Status
```powershell
# Run from a git repository
$branch = git branch --show-current
$status = git status --porcelain
$modified = ($status | Where-Object { $_ -match '^ M' }).Count
$added = ($status | Where-Object { $_ -match '^A' }).Count
$deleted = ($status | Where-Object { $_ -match '^D' }).Count
$untracked = ($status | Where-Object { $_ -match '^\?\?' }).Count

New-ConsoleDocument -Width 80 -EnableVT |
    Add-ConsoleHeader -Text "GIT STATUS" -PassThru |
    Add-ConsolePanel -Title "Current Branch" -Text $branch -PassThru |
    Add-ConsoleTable -Title "Changes" -Columns @("Type", "Count") -Rows @(
        @("Modified", $modified.ToString()),
        @("Added", $added.ToString()),
        @("Deleted", $deleted.ToString()),
        @("Untracked", $untracked.ToString())
    ) -PassThru |
    Add-ConsoleDividedBarGraph -Title "Change Distribution" -Segments @(
        @{ Text = "Modified"; Value = [Math]::Max(1, $modified); Color = "Yellow" },
        @{ Text = "Added"; Value = [Math]::Max(1, $added); Color = "Green" },
        @{ Text = "Deleted"; Value = [Math]::Max(1, $deleted); Color = "Red" },
        @{ Text = "Untracked"; Value = [Math]::Max(1, $untracked); Color = "Gray" }
    ) -PassThru |
    Write-ConsoleDocument
```

### Service Health Check
```powershell
$services = @("wuauserv", "BITS", "Spooler", "W32Time") | ForEach-Object {
    $svc = Get-Service $_ -ErrorAction SilentlyContinue
    @($_,  $(if($svc.Status -eq 'Running'){"Running"}else{"Stopped"}))
}

New-ConsoleDocument -Width 90 -EnableVT |
    Add-ConsoleHeader -Text "SERVICE HEALTH" -PassThru |
    Add-ConsoleTable -Title "Windows Services" -Columns @("Service", "Status") -Rows $services -PassThru |
    Add-ConsoleFooter -Text "Checked at $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')" -PassThru |
    Write-ConsoleDocument
```

### Project Task Outline
```powershell
$tasks = @(
    @{ Text = "Phase 1: Planning"; Children = @(
        @{ Text = "Define requirements" },
        @{ Text = "Create timeline" },
        @{ Text = "Assign resources" }
    )},
    @{ Text = "Phase 2: Development"; Children = @(
        @{ Text = "Backend API"; Children = @(
            @{ Text = "Database schema" },
            @{ Text = "REST endpoints" }
        )},
        @{ Text = "Frontend UI" }
    )},
    @{ Text = "Phase 3: Testing" },
    @{ Text = "Phase 4: Deployment" }
)

New-ConsoleDocument -Width 100 -EnableVT |
    Add-ConsoleHeader -Text "PROJECT ROADMAP" -PassThru |
    Add-ConsoleOutline -Title "Task Breakdown" -Nodes $tasks -PassThru |
    Add-ConsoleFooter -Text "Q1 2025" -PassThru |
    Write-ConsoleDocument
```

### Progress Bar with Real Work
```powershell
$progress = [hashtable]::Synchronized(@{ Current = 0; Total = 100; Status = "Initializing..." })

$task = [System.Threading.Tasks.Task]::Run({
    param($p)
    $files = Get-ChildItem "C:\Windows\System32" -File | Select-Object -First 100
    $p.Total = $files.Count
    
    for ($i = 0; $i -lt $files.Count; $i++) {
        $p.Current = $i + 1
        $p.Status = "Processing: $($files[$i].Name)"
        Start-Sleep -Milliseconds 30
    }
    $p.Status = "Complete!"
}.GetNewClosure())

# Pass the hashtable to the task
$task = [System.Threading.Tasks.Task]::Run({
    $files = Get-ChildItem "C:\Windows\System32" -File -ErrorAction SilentlyContinue | Select-Object -First 50
    for ($i = 0; $i -lt $files.Count; $i++) {
        $script:progress.Current = $i + 1
        $script:progress.Status = "Scanning: $($files[$i].Name)"
        Start-Sleep -Milliseconds 50
    }
})

New-ConsoleDocument -Width 100 -EnableVT |
    Add-ConsoleHeader -Text "FILE SCANNER" -PassThru |
    Add-ConsoleProgressBar -Title "Scanning Files" -ProgressSource $progress -Task $task -PassThru |
    Write-ConsoleDocument -Wait
```

---

## C# Examples

### Basic Document
```csharp
using ConsoleDocumentSystem;
using ConsoleDocumentSystem.Models;

var doc = new ConsoleDocument(100, enableVT: true);

doc.Blocks.Add(new ConsoleHeader("Application Title"));
doc.Blocks.Add(new ConsolePanel("Welcome", "This is a demonstration of ConsoleDocument capabilities."));
doc.Blocks.Add(new ConsoleSeperator("Data Section"));
doc.Blocks.Add(new ConsoleFooter("Goodbye"));

doc.Render();
```

### Table with Hierarchical Data
```csharp
using ConsoleDocumentSystem;
using ConsoleDocumentSystem.Models;
using ConsoleDocumentSystem.Models.Parts;

var doc = new ConsoleDocument(120, enableVT: true);

var hierarchy = new ConsoleTableHierarchy
{
    Columns = new List<string> { "Department", "Team", "Employee" }
};

// Add root node
hierarchy.RootNodes["Engineering"] = new ConsoleTableNode 
{ 
    Key = "Engineering", 
    Depth = 0,
    Children = new Dictionary<string, ConsoleTableNode>
    {
        ["Backend"] = new ConsoleTableNode 
        { 
            Key = "Backend", 
            Depth = 1, 
            Values = new List<string?> { "Alice", "Bob" } 
        },
        ["Frontend"] = new ConsoleTableNode 
        { 
            Key = "Frontend", 
            Depth = 1, 
            Values = new List<string?> { "Carol" } 
        }
    }
};

doc.Blocks.Add(new ConsoleTable("Organization", hierarchy));
doc.Render();
```

### Bar Graph
```csharp
using ConsoleDocumentSystem;
using ConsoleDocumentSystem.Models;
using ConsoleDocumentSystem.Models.Parts;
using ConsoleDocumentSystem.Enums;

var doc = new ConsoleDocument(100, enableVT: true);

var segments = new List<ConsoleGraphSegment>
{
    new("January", PlushColor.Blue, 150),
    new("February", PlushColor.Green, 200),
    new("March", PlushColor.Yellow, 175),
    new("April", PlushColor.Red, 225)
};

doc.Blocks.Add(new ConsoleBarGraph("Monthly Sales", segments));
doc.Render();
```

### Tree Diagram
```csharp
using ConsoleDocumentSystem;
using ConsoleDocumentSystem.Models;
using ConsoleDocumentSystem.Models.Parts;

var doc = new ConsoleDocument(100, enableVT: true);

var root = new ConsoleNode("Project Root", new List<ConsoleNode>
{
    new("src", new List<ConsoleNode>
    {
        new("Models"),
        new("Controllers"),
        new("Views")
    }),
    new("tests"),
    new("docs")
});

doc.Blocks.Add(new ConsoleTreeDiagram("Project Structure", root));
doc.Render();
```

### Progress Bar with Async Task
```csharp
using ConsoleDocumentSystem;
using ConsoleDocumentSystem.Models;
using ConsoleDocumentSystem.Models.Structs;
using ConsoleDocumentSystem.Helpers;

var doc = new ConsoleDocument(100, enableVT: true);

long current = 0;
long total = 100;
string status = "Starting...";

var workTask = Task.Run(async () =>
{
    for (int i = 0; i <= 100; i++)
    {
        current = i;
        status = $"Processing item {i}...";
        await Task.Delay(50);
    }
    status = "Complete!";
});

var progressBar = new ConsoleProgressBar(
    "Processing Data",
    vtEnabled: true,
    progressProvider: () => new ProgressState(current, total, status),
    workTask: workTask
);

doc.Blocks.Add(progressBar);
doc.Render();

await LiveRegionRenderer.StopAsync();
```
