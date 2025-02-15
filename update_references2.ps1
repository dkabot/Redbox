# Define the new base folder for Redbox projects
$newBaseFolder = "Redbox"

# Prompt for the project name to move
$projectToMove = Read-Host "Enter the project name to move (e.g., Redbox.Command.Tokenizer)"

# Get all .sln files in the repository
$slnFiles = Get-ChildItem -Recurse -Filter *.sln

Write-Host "Found $($slnFiles.Count) .sln files in the repository."

# Loop through each .sln file
foreach ($slnFile in $slnFiles) {
    Write-Host "Processing .sln file: $($slnFile.FullName)" -ForegroundColor Cyan

    $slnDir = $slnFile.DirectoryName
    $slnContent = Get-Content $slnFile.FullName

    # Track whether any changes were made to this file
    $fileUpdated = $false

    # Find and update project locations
    $updatedContent = $slnContent | ForEach-Object {
        if ($_ -match 'Project\("{.*}"\) = "([^"]+)", "([^"]+)", "{.*}"') {
            $projectName = $matches[1]
            $oldPath = $matches[2]

            Write-Host "  Found Project: $projectName"
            Write-Host "    Old Path: $oldPath"

            # Check if the project name matches the project to move
            if ($projectName -eq $projectToMove) {
                Write-Host "    Matches project to move: $projectToMove" -ForegroundColor Green

                # Calculate the number of "..\" levels in the old path
                $oldPathLevels = ($oldPath -split '\\' | Where-Object { $_ -eq '..' }).Count
                Write-Host "    Old Path Levels: $oldPathLevels"

                # Construct the new relative path
                $newRelativePath = ("..\" * $oldPathLevels) + "$newBaseFolder\$projectToMove\$projectToMove.csproj"
                Write-Host "    New Relative Path: $newRelativePath" -ForegroundColor Yellow

                $fileUpdated = $true

                # Update the line with the new path
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
        Write-Host "  Updating file: $($slnFile.FullName)" -ForegroundColor Green
        $updatedContent | Set-Content $slnFile.FullName
    } else {
        Write-Host "  No changes made to file: $($slnFile.FullName)" -ForegroundColor Gray
    }
}

Write-Host "Script completed." -ForegroundColor Cyan