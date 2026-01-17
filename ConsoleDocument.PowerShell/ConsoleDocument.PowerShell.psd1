@{
    # Module manifest for ConsoleDocument.PowerShell
    
    # Script module or binary module file associated with this manifest.
    RootModule = 'ConsoleDocument.PowerShell.dll'
    
    # Version number of this module.
    ModuleVersion = '1.0.0'
    
    # Supported PSEditions
    CompatiblePSEditions = @('Core')
    
    # ID used to uniquely identify this module
    GUID = 'a1b2c3d4-e5f6-7890-abcd-ef1234567890'
    
    # Author of this module
    Author = 'Your Name'
    
    # Company or vendor of this module
    CompanyName = 'Your Company'
    
    # Copyright statement for this module
    Copyright = '(c) 2024. All rights reserved.'
    
    # Description of the functionality provided by this module
    Description = 'A PowerShell module for creating rich, styled console output including tables, progress bars, trees, and more using the ConsoleDocumentSystem library.'
    
    # Minimum version of the PowerShell engine required by this module
    PowerShellVersion = '7.0'
    
    # Minimum version of the .NET Framework required by this module
    DotNetFrameworkVersion = '9.0'
    
    # Processor architecture (None, X86, Amd64) required by this module
    # ProcessorArchitecture = ''
    
    # Modules that must be imported into the global environment prior to importing this module
    # RequiredModules = @()
    
    # Assemblies that must be loaded prior to importing this module
    RequiredAssemblies = @('ConsoleDocumentSystem.dll')
    
    # Type files (.ps1xml) to be loaded when importing this module
    # TypesToProcess = @()
    
    
    # Format files (.ps1xml) to be loaded when importing this module
    FormatsToProcess = @('ConsoleDocument.PowerShell.Format.ps1xml')
    
    # Functions to export from this module
    FunctionsToExport = @()
    
    # Cmdlets to export from this module
    CmdletsToExport = @(
        'New-ConsoleDocument',
        'Add-ConsoleHeader',
        'Add-ConsoleFooter',
        'Add-ConsolePanel',
        'Add-ConsoleSeparator',
        'Add-ConsoleTable',
        'Add-ConsoleTreeDiagram',
        'Add-ConsoleOutline',
        'Add-ConsoleBarGraph',
        'Add-ConsoleDividedBarGraph',
        'Add-ConsoleProgressBar',
        'Write-ConsoleDocument',
        'Stop-ConsoleLiveRegion'
    )
    
    # Variables to export from this module
    VariablesToExport = @()
    
    # Aliases to export from this module
    AliasesToExport = @()
    
    # List of all modules packaged with this module
    # ModuleList = @()
    
    # List of all files packaged with this module
    # FileList = @()
    
    # Private data to pass to the module specified in RootModule/ModuleToProcess
    PrivateData = @{
        PSData = @{
            # Tags applied to this module for discovery
            Tags = @('Console', 'Output', 'Formatting', 'Table', 'ProgressBar', 'ANSI', 'VT')
            
            # A URL to the license for this module.
            # LicenseUri = ''
            
            # A URL to the main website for this project.
            # ProjectUri = ''
            
            # A URL to an icon representing this module.
            # IconUri = ''
            
            # Release notes for this module
            ReleaseNotes = 'Initial release of ConsoleDocument.PowerShell module.'
            
            # Prerelease string of this module
            # Prerelease = ''
            
            # Flag to indicate whether the module requires explicit user acceptance for install/update/save
            # RequireLicenseAcceptance = $false
            
            # External dependent modules of this module
            # ExternalModuleDependencies = @()
        }
    }
    
    # HelpInfo URI of this module
    # HelpInfoURI = ''
    
    # Default prefix for commands exported from this module
    # DefaultCommandPrefix = 'Plush'
}
