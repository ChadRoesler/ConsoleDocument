# ConsoleDocument.PowerShell Module

A PowerShell module for creating rich, styled console output using the ConsoleDocumentSystem library.

## Installation

```powershell
# Build the module
dotnet build ConsoleDocument.PowerShell/ConsoleDocument.PowerShell.csproj

# Import the module
Import-Module ./ConsoleDocument.PowerShell/bin/Debug/net9.0/ConsoleDocument.PowerShell.dll
```

## Cmdlets

### Document Management

#### New-ConsoleDocument
Creates a new ConsoleDocument object.

```powershell
New-ConsoleDocument [-Width <Int32>] [-EnableVT] [-TextColor <PlushColor>] [-BorderColor <PlushColor>] 
                    [-TreeNodeColor <PlushColor>] [-BarGraphColor <PlushColor>] [-ProgressBarColor <PlushColor>]
```

**Parameters:**
- `-Width` - Document width in characters (50-200, default: 100)
- `-EnableVT` - Enable VT/ANSI color sequences
- `-TextColor` - Default text color
- `-BorderColor` - Border color for blocks
- `-TreeNodeColor` - Color for tree diagram nodes
- `-BarGraphColor` - Default bar graph color
- `-ProgressBarColor` - Progress bar fill color

**Example:**
```powershell
$doc = New-ConsoleDocument -Width 120 -EnableVT -BorderColor Cyan
```

#### Write-ConsoleDocument
Renders a ConsoleDocument to the console.

```powershell
Write-ConsoleDocument [-Document] <ConsoleDocument> [-Wait] [-PassThru]
```

**Parameters:**
- `-Document` - The ConsoleDocument to render (accepts pipeline input)
- `-Wait` - Wait for live regions (progress bars) to complete
- `-PassThru` - Return the document after rendering

**Example:**
```powershell
$doc | Write-ConsoleDocument -Wait
```

#### Stop-ConsoleLiveRegion
Stops the live region renderer for progress bars.

```powershell
Stop-ConsoleLiveRegion
```

---

### Block Cmdlets

All block cmdlets accept a `-Document` parameter from the pipeline and support `-PassThru` to continue the pipeline.

#### Add-ConsoleHeader
Adds a decorative header banner.

```powershell
Add-ConsoleHeader [-Document] <ConsoleDocument> [-Text] <String> [-PassThru]
```

**Example:**
```powershell
$doc | Add-ConsoleHeader -Text "MY APPLICATION" -PassThru
```

#### Add-ConsoleFooter
Adds a decorative footer banner.

```powershell
Add-ConsoleFooter [-Document] <ConsoleDocument> [-Text] <String> [-PassThru]
```

**Example:**
```powershell
$doc | Add-ConsoleFooter -Text "End of Report" -PassThru
```

#### Add-ConsolePanel
Adds a text panel with title and body text (auto word-wrapped).

```powershell
Add-ConsolePanel [-Document] <ConsoleDocument> [-Title] <String> [-Text] <String> [-PassThru]
```

**Example:**
```powershell
$doc | Add-ConsolePanel -Title "Description" -Text "This is a long description that will be automatically wrapped to fit within the document width." -PassThru
```

#### Add-ConsoleSeparator
Adds a section divider, optionally with centered text.

```powershell
Add-ConsoleSeparator [-Document] <ConsoleDocument> [-Text <String>] [-PassThru]
```

**Example:**
```powershell
$doc | Add-ConsoleSeparator -Text "Section 2" -PassThru
```

#### Add-ConsoleTable
Adds a hierarchical table.

```powershell
# From rows
Add-ConsoleTable [-Document] <ConsoleDocument> [-Title] <String> [-Columns] <String[]> [-Rows] <String[][]> [-PassThru]

# From DataTable
Add-ConsoleTable [-Document] <ConsoleDocument> [-Title] <String> [-Data] <DataTable> [-PassThru]

# From existing hierarchy
Add-ConsoleTable [-Document] <ConsoleDocument> [-Title] <String> [-Hierarchy] <ConsoleTableHierarchy> [-PassThru]
```

**Example:**
```powershell
$doc | Add-ConsoleTable -Title "Users" -Columns @("Department", "Name", "Role") -Rows @(
    @("Engineering", "Alice", "Developer"),
    @("Engineering", "Bob", "Developer"),
    @("Marketing", "Carol", "Manager")
) -PassThru
```

#### Add-ConsoleTreeDiagram
Adds a tree structure visualization.

```powershell
Add-ConsoleTreeDiagram [-Document] <ConsoleDocument> [-Title] <String> [-RootNode] <PSObject> [-PassThru]
```

**Node Format:**
```powershell
@{
    Text = "Node Name"
    Children = @(
        @{ Text = "Child 1" },
        @{ Text = "Child 2"; Children = @(...) }
    )
}
```

**Example:**
```powershell
$tree = @{
    Text = "Root"
    Children = @(
        @{ Text = "Branch A"; Children = @(@{ Text = "Leaf 1" }, @{ Text = "Leaf 2" }) },
        @{ Text = "Branch B" }
    )
}
$doc | Add-ConsoleTreeDiagram -Title "File Structure" -RootNode $tree -PassThru
```

#### Add-ConsoleOutline
Adds a hierarchical bullet list.

```powershell
Add-ConsoleOutline [-Document] <ConsoleDocument> [-Title] <String> [-Nodes] <PSObject[]> [-PassThru]
```

**Example:**
```powershell
$outline = @(
    @{ Text = "Introduction"; Children = @(@{ Text = "Background" }, @{ Text = "Goals" }) },
    @{ Text = "Methods" },
    @{ Text = "Results" },
    @{ Text = "Conclusion" }
)
$doc | Add-ConsoleOutline -Title "Report Structure" -Nodes $outline -PassThru
```

#### Add-ConsoleBarGraph
Adds a horizontal bar chart.

```powershell
Add-ConsoleBarGraph [-Document] <ConsoleDocument> [-Title] <String> [-Segments] <PSObject[]> [-PassThru]
```

**Segment Format:**
```powershell
@{ Text = "Label"; Value = 100; Color = "Green" }  # Color is optional
```

**Example:**
```powershell
$data = @(
    @{ Text = "Completed"; Value = 75; Color = "Green" },
    @{ Text = "In Progress"; Value = 20; Color = "Yellow" },
    @{ Text = "Failed"; Value = 5; Color = "Red" }
)
$doc | Add-ConsoleBarGraph -Title "Task Status" -Segments $data -PassThru
```

#### Add-ConsoleDividedBarGraph
Adds a stacked/proportional bar showing segments in one horizontal bar.

```powershell
Add-ConsoleDividedBarGraph [-Document] <ConsoleDocument> [-Title] <String> [-Segments] <PSObject[]> [-PassThru]
```

**Example:**
```powershell
$data = @(
    @{ Text = "Used"; Value = 65; Color = "Red" },
    @{ Text = "Free"; Value = 35; Color = "Green" }
)
$doc | Add-ConsoleDividedBarGraph -Title "Disk Space" -Segments $data -PassThru
```

#### Add-ConsoleProgressBar
Adds a progress bar (static or dynamic).

```powershell
# Static progress
Add-ConsoleProgressBar [-Document] <ConsoleDocument> [-Title] <String> [-Current <Int64>] [-Total <Int64>] 
                       [-Status <String>] [-BarColor <PlushColor>] [-EmptyColor <PlushColor>] [-PassThru]

# Dynamic progress
Add-ConsoleProgressBar [-Document] <ConsoleDocument> [-Title] <String> [-ProgressSource] <PSObject> 
                       [-Task] <Task> [-BarColor <PlushColor>] [-EmptyColor <PlushColor>] [-PassThru]
```

**Static Example:**
```powershell
$doc | Add-ConsoleProgressBar -Title "Download" -Current 50 -Total 100 -Status "Downloading..." -PassThru
```

**Dynamic Example:**
```powershell
$progress = @{ Current = 0; Total = 100; Status = "Starting..." }
$task = [System.Threading.Tasks.Task]::Run({
    1..100 | ForEach-Object {
        $progress.Current = $_
        $progress.Status = "Processing item $_"
        Start-Sleep -Milliseconds 50
    }
})
$doc | Add-ConsoleProgressBar -Title "Processing" -ProgressSource $progress -Task $task -PassThru
$doc | Write-ConsoleDocument -Wait
```

---

## Complete Example

```powershell
Import-Module ./ConsoleDocument.PowerShell/bin/Debug/net9.0/ConsoleDocument.PowerShell.dll

New-ConsoleDocument -Width 100 -EnableVT -BorderColor Cyan |
    Add-ConsoleHeader -Text "DAILY REPORT" -PassThru |
    Add-ConsolePanel -Title "Summary" -Text "All systems operational. No critical issues detected in the last 24 hours." -PassThru |
    Add-ConsoleSeparator -Text "Metrics" -PassThru |
    Add-ConsoleTable -Title "Server Status" -Columns @("Server", "Status", "Uptime") -Rows @(
        @("web-01", "Online", "99.9%"),
        @("web-02", "Online", "99.8%"),
        @("db-01", "Online", "100%")
    ) -PassThru |
    Add-ConsoleBarGraph -Title "Resource Usage" -Segments @(
        @{ Text = "CPU"; Value = 45; Color = "Green" },
        @{ Text = "Memory"; Value = 72; Color = "Yellow" },
        @{ Text = "Disk"; Value = 58; Color = "Green" }
    ) -PassThru |
    Add-ConsoleDividedBarGraph -Title "Request Distribution" -Segments @(
        @{ Text = "Success"; Value = 950; Color = "Green" },
        @{ Text = "Redirect"; Value = 30; Color = "Yellow" },
        @{ Text = "Error"; Value = 20; Color = "Red" }
    ) -PassThru |
    Add-ConsoleFooter -Text "Generated $(Get-Date -Format 'yyyy-MM-dd HH:mm')" -PassThru |
    Write-ConsoleDocument
```

---

## Color Reference

The module supports 140+ colors via tab completion. Common colors include:

| Category | Colors |
|----------|--------|
| Basic | Red, Green, Blue, Yellow, Cyan, Magenta, White, Black |
| Dark | DarkRed, DarkGreen, DarkBlue, DarkCyan, DarkGray, DarkOrange |
| Light | LightBlue, LightGreen, LightPink, LightGray, LightCyan |
| Vivid | Coral, Crimson, Gold, Teal, Navy, Purple, Orange, Lime |
| Custom | SerenValeBlue, ChadsCopper, VelvetRot |
| Terminal | DefaultForeground, DefaultBackground |

Use tab completion on any color parameter to see all available options:
```powershell
New-ConsoleDocument -BorderColor <TAB>
```

---

## Tips

1. **Pipeline Everything** - Use `-PassThru` to chain cmdlets
2. **Enable VT** - Always use `-EnableVT` for color support in modern terminals
3. **Width Matters** - Set width to match your terminal for best results
4. **Tab Completion** - Colors have full IntelliSense/tab completion support
5. **Live Progress** - Use `-Wait` on `Write-ConsoleDocument` when using progress bars
