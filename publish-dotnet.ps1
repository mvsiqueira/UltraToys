# publish-dotnet.ps1 — publica como DLL framework-dependent (requer dotnet instalado)
# Uso: .\publish-dotnet.ps1 [-Version "1.0.0"]
# Iniciar com: dotnet "caminho\UltraToys.dll"

param(
    [string]$Version = "1.0.0"
)

$project = "UltraToys\UltraToys.csproj"
$outDir  = "dist\UltraToys-$Version-dotnet"

Write-Host "=== UltraToys publish (framework-dependent) v$Version ===" -ForegroundColor Cyan

if (Test-Path $outDir) {
    Remove-Item $outDir -Recurse -Force
}

dotnet publish $project `
    -c Release `
    --self-contained false `
    /p:PublishSingleFile=false `
    /p:Version=$Version `
    /p:FileVersion="$Version.0" `
    /p:AssemblyVersion="$Version.0" `
    -o $outDir

if ($LASTEXITCODE -ne 0) {
    Write-Host "ERRO: publish falhou." -ForegroundColor Red
    exit 1
}

$dll = Get-ChildItem $outDir -Filter "UltraToys.dll" | Select-Object -First 1
if ($dll) {
    $sizeMb = [math]::Round((Get-ChildItem $outDir | Measure-Object -Property Length -Sum).Sum / 1MB, 1)

    # Recria o launcher VBS
    $vbsPath = Join-Path $outDir "UltraToys.vbs"
    @'
Dim dll
dll = CreateObject("Scripting.FileSystemObject").GetParentFolderName(WScript.ScriptFullName) & "\UltraToys.dll"
CreateObject("WScript.Shell").Run "dotnet """ & dll & """", 0, False
'@ | Set-Content $vbsPath -Encoding UTF8

    Write-Host ""
    Write-Host "Publicado com sucesso:" -ForegroundColor Green
    Write-Host "  Pasta: $outDir  ($sizeMb MB total)" -ForegroundColor Green
    Write-Host "  VBS:   $vbsPath" -ForegroundColor Green
    Write-Host ""
    Write-Host "Para executar: clique duplo em UltraToys.vbs" -ForegroundColor Yellow
} else {
    Write-Host "Aviso: UltraToys.dll nao encontrada em $outDir" -ForegroundColor Yellow
}
