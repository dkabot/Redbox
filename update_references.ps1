# Define the new base folder for Redbox projects
$newBaseFolder = "Redbox"

# Prompt for the project name to migrate
$projectToMove = Read-Host "Enter the project name to move (e.g., Redbox.Command.Tokenizer)"

# Get all .csproj files in the repository
$csprojFiles = Get-ChildItem -Recurse -Filter *.csproj

Write-Host "Found $($csprojFiles.Count) .csproj files in the repository."

# Loop through each .csproj file
foreach ($csprojFile in $csprojFiles) {
    Write-Host "Processing file: $($csprojFile.FullName)" -ForegroundColor Cyan

    $csprojDir = $csprojFile.DirectoryName
    $csprojContent = Get-Content $csprojFile.FullName

    Write-Host "  Directory: $csprojDir"

    # Track whether any changes were made to this file
    $fileUpdated = $false

    # Find and update ProjectReference paths
    $updatedContent = $csprojContent | ForEach-Object {
        if ($_ -match '<ProjectReference Include="([^"]+)"') {
            $oldPath = $matches[1]
            $referencedProjectName = [System.IO.Path]::GetFileNameWithoutExtension($oldPath)

            Write-Host "  Found ProjectReference: $oldPath"
            Write-Host "    Referenced Project Name: $referencedProjectName"

            # Check if the referenced project matches the project to move
            if ($referencedProjectName -eq $projectToMove) {
                Write-Host "    Matches project to move: $projectToMove" -ForegroundColor Green

                # Calculate the number of "..\" levels in the old path
                $oldPathLevels = ($oldPath -split '\\' | Where-Object { $_ -eq '..' }).Count
                Write-Host "    Old Path Levels: $oldPathLevels"

                # Construct the new relative path
                $newRelativePath = ("..\" * $oldPathLevels) + "$newBaseFolder\$projectToMove\$projectToMove.csproj"
                Write-Host "    New Relative Path: $newRelativePath" -ForegroundColor Yellow

                $fileUpdated = $true
                $_ -replace [regex]::Escape($oldPath), $newRelativePath
            } else {
                Write-Host "    Does not match project to move (skipping)" -ForegroundColor Gray
                $_
            }
        } else {
            $_
        }
    }

    # Save the updated content if changes were made
    if ($fileUpdated) {
        Write-Host "  Updating file: $($csprojFile.FullName)" -ForegroundColor Green
        $updatedContent | Set-Content $csprojFile.FullName
    } else {
        Write-Host "  No changes made to file: $($csprojFile.FullName)" -ForegroundColor Gray
    }
}

Write-Host "Script completed." -ForegroundColor Cyan