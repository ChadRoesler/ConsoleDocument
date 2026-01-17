# ConsoleDocument

[![License: GPL v3](https://img.shields.io/badge/License-GPLv3-blue.svg)](https://www.gnu.org/licenses/gpl-3.0)
[![.NET 9](https://img.shields.io/badge/.NET-9.0-purple.svg)](https://dotnet.microsoft.com/)
[![PowerShell 7+](https://img.shields.io/badge/PowerShell-7.0+-blue.svg)](https://github.com/PowerShell/PowerShell)

A powerful .NET library for creating rich, styled console output with tables, progress bars, tree diagrams, bar graphs, and more. Includes a **full PowerShell module** for scripting scenarios.

```
??????????????????????????????????????????????????????????????????????????????????
?                              ?????????????????                                 ?
???????????????????????????????? SYSTEM REPORT ???????????????????????????????????
?                              ?????????????????                                 ?
??????????????????????????????????????????????????????????????????????????????????
??????????????????????????????????????????????????????????????????????????????????
? [+] Overview                                                                   ?
??????????????????????????????????????????????????????????????????????????????????
? Daily system health check completed successfully.                              ?
??????????????????????????????????????????????????????????????????????????????????
```

## ? Features

- ?? **Tables** - Hierarchical tables with automatic column grouping
- ?? **Bar Graphs** - Horizontal bar charts with customizable colors
- ?? **Divided Bar Graphs** - Stacked/proportional bar visualization
- ?? **Tree Diagrams** - Visual tree structures with branch connectors
- ?? **Outlines** - Hierarchical bullet lists with depth coloring
- ?? **Panels** - Text panels with automatic word wrapping
- ?? **Progress Bars** - Live-updating progress with async support
- ?? **140+ Colors** - Full RGB color palette with VT/ANSI support
- ? **PowerShell Module** - Complete cmdlet library (13 cmdlets!)

## ?? Installation

### .NET Library

```bash
# Clone the repository
git clone https://github.com/ChadRoesler/ConsoleDocument.git

# Build the solution
dotnet build
```

### PowerShell Module

```powershell
# Build the module
dotnet build ConsoleDocument.PowerShell/ConsoleDocument.PowerShell.csproj

# Import the module
Import-Module ./ConsoleDocument.PowerShell/bin/Debug/net9.0/ConsoleDocument.PowerShell.dll
```

## ?? Quick Start

### C# Example

```csharp
using ConsoleDocumentSystem;
using ConsoleDocumentSystem.Models;
using ConsoleDocumentSystem.Models.Parts;

// Create a new document
var doc = new ConsoleDocument(100, enableVT: true);

// Add blocks
doc.Blocks.Add(new ConsoleHeader("My Application"));
doc.Blocks.Add(new ConsolePanel("Welcome", "Rich console formatting made easy!"));
doc.Blocks.Add(new ConsoleFooter("End of Report"));

// Render to console
doc.Render();
```

### PowerShell Example

```powershell
Import-Module ./ConsoleDocument.PowerShell/bin/Debug/net9.0/ConsoleDocument.PowerShell.dll

# Create a beautiful report with pipeline syntax
New-ConsoleDocument -Width 100 -EnableVT |
    Add-ConsoleHeader -Text "SYSTEM REPORT" -PassThru |
    Add-ConsolePanel -Title "Overview" -Text "Daily system health check completed." -PassThru |
    Add-ConsoleSeparator -Text "Metrics" -PassThru |
    Add-ConsoleTable -Title "Resource Usage" -Columns @("Resource", "Usage") -Rows @(
        @("CPU", "45%"),
        @("Memory", "62%"),
        @("Disk", "78%")
    ) -PassThru |
    Add-ConsoleBarGraph -Title "Visual Breakdown" -Segments @(
        @{Text="CPU"; Value=45; Color="Green"},
        @{Text="Memory"; Value=62; Color="Yellow"},
        @{Text="Disk"; Value=78; Color="Red"}
    ) -PassThru |
    Add-ConsoleFooter -Text "Report Complete" -PassThru |
    Write-ConsoleDocument
```

## ?? Documentation

| Document | Description |
|----------|-------------|
| [PowerShell Module Guide](docs/PowerShell-Module.md) | Complete cmdlet reference with examples |
| [Examples](docs/Examples.md) | Real-world C# and PowerShell examples |
| [Design Document](DesignDoc.txt) | Design philosophy and conventions |

## ?? Block Types

| Block Type | Description | C# Class | PowerShell Cmdlet |
|------------|-------------|----------|-------------------|
| Header | Decorative banner at top | `ConsoleHeader` | `Add-ConsoleHeader` |
| Footer | Decorative banner at bottom | `ConsoleFooter` | `Add-ConsoleFooter` |
| Panel | Text box with title | `ConsolePanel` | `Add-ConsolePanel` |
| Separator | Section divider | `ConsoleSeperator` | `Add-ConsoleSeparator` |
| Table | Hierarchical data table | `ConsoleTable` | `Add-ConsoleTable` |
| Tree Diagram | Tree structure | `ConsoleTreeDiagram` | `Add-ConsoleTreeDiagram` |
| Outline | Bullet point list | `ConsoleOutline` | `Add-ConsoleOutline` |
| Bar Graph | Horizontal bars | `ConsoleBarGraph` | `Add-ConsoleBarGraph` |
| Divided Bar | Stacked bar | `ConsoleDividedBarGraph` | `Add-ConsoleDividedBarGraph` |
| Progress Bar | Live progress | `ConsoleProgressBar` | `Add-ConsoleProgressBar` |

## ?? PowerShell Cmdlets

| Cmdlet | Description |
|--------|-------------|
| `New-ConsoleDocument` | Creates a new document with width and color settings |
| `Add-ConsoleHeader` | Adds a decorative header banner |
| `Add-ConsoleFooter` | Adds a decorative footer banner |
| `Add-ConsolePanel` | Adds a text panel with title |
| `Add-ConsoleSeparator` | Adds a section divider |
| `Add-ConsoleTable` | Adds a hierarchical table |
| `Add-ConsoleTreeDiagram` | Adds a tree structure visualization |
| `Add-ConsoleOutline` | Adds a hierarchical bullet list |
| `Add-ConsoleBarGraph` | Adds a horizontal bar chart |
| `Add-ConsoleDividedBarGraph` | Adds a stacked/proportional bar |
| `Add-ConsoleProgressBar` | Adds a progress bar (static or dynamic) |
| `Write-ConsoleDocument` | Renders the document to console |
| `Stop-ConsoleLiveRegion` | Stops live progress bar updates |

## ?? Color Palette

ConsoleDocument supports **140+ colors** via the `PlushColor` enum with full tab completion in PowerShell!

| Category | Colors |
|----------|--------|
| **Basic** | `Red`, `Green`, `Blue`, `Yellow`, `Cyan`, `Magenta`, `White`, `Black` |
| **Dark** | `DarkRed`, `DarkGreen`, `DarkBlue`, `DarkCyan`, `DarkGray`, `DarkOrange` |
| **Light** | `LightBlue`, `LightGreen`, `LightPink`, `LightGray`, `LightCyan` |
| **Vivid** | `Coral`, `Crimson`, `Gold`, `Teal`, `Navy`, `Purple`, `Orange`, `Lime` |
| **Custom** | `SerenValeBlue`, `ChadsCopper`, `VelvetRot` |
| **Terminal** | `DefaultForeground`, `DefaultBackground` |

## ??? Project Structure

```
ConsoleDocument/
??? ConsoleDocument/                    # Core library
?   ??? Models/                         # Block types (Table, Panel, etc.)
?   ??? Helpers/                        # Rendering and VT helpers
?   ??? Enums/                          # PlushColor, PlushTextStyle
?   ??? Interfaces/                     # IConsoleBlock, ILiveRenderable
??? ConsoleDocument.PowerShell/         # PowerShell module
?   ??? Cmdlets/                        # All 13 cmdlet implementations
?   ??? Completers/                     # Tab completion providers
??? TestingApp/                         # Demo/test application
??? docs/                               # Documentation
```

## ?? Design Philosophy

ConsoleDocument follows specific design conventions (see [DesignDoc.txt](DesignDoc.txt)):

- **`[+]` glyph** denotes titles, roots, and section starts
- **Separator boxes** (`??? ???`) mark major breaks
- **Tables** use double-line box characters (`???????????`)
- **Text centering** with excess space added to the right
- **Column grouping** follows parent?child hierarchy
- **Bar graphs** scale dynamically with configurable ratios

## ?? Requirements

- **.NET 9.0** or later
- **PowerShell 7.0+** (for the PowerShell module)
- **VT/ANSI-compatible terminal** for color support (Windows Terminal, iTerm2, etc.)

## ?? License

This project is licensed under the **GNU General Public License v3.0** - see the [LICENSE](LICENSE) file for details.

## ?? Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## ?? Author

**Chad Roesler** - [GitHub](https://github.com/ChadRoesler)

---

<p align="center">Made with ?? and lots of box-drawing characters</p>
