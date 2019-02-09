# Remove the tar file if it is there
if (Test-Path 'init.tar') {
	Write-Host 'Deleting old tar file...'
	Remove-Item 'init.tar'
}

# Create the new tar
tar -cf init.tar Dockerfile spamdetector.csproj func.init.yaml Program.cs spamdata

Write-Host 'New Tar file created!'