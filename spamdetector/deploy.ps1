Write-Host -ForegroundColor Blue 'Simple deploy for dotnet boilerplate function'

$id = (docker images fn-dotnet -q)

if (-not $id) {
	Write-Host "Init image was not found. Building..."
	docker build -t fn-dotnet -f Dockerfile-init-image .
}

Write-Host -ForegroundColor Green "==> Initializing spam function"
fn --verbose init --init-image=fn-dotnet spam

Set-Location spam

Write-Host -ForegroundColor Green "==> Deploying function to local Fn server"
fn --verbose deploy --app dotnet --local

Write-Host -ForegroundColor Green "==> Function deployed!"