# ============================================================================
#  DEMO CATALOG-API — guion ejecutable para grabar el video de funcionamiento
#  Requisito: contenedor levantado (docker compose up -d)
#  Ejecutar en PowerShell 5.1 o 7 (Windows):
#    powershell -ExecutionPolicy Bypass -File .\scripts\demo.ps1
#  Para arrancar limpio (volumen vacio) ejecutar antes:
#    docker compose down -v  &&  docker compose up -d
#
#  Nota tecnica: usa curl.exe con -k (el certificado es autofirmado). No usa
#  Invoke-WebRequest porque el callback de certificado de PS5.1 en threads de
#  TLS de .NET falla con PSInvalidOperationException (sin runspace disponible).
# ============================================================================
param(
    [string]$BaseUrl = "https://localhost:5443"
)

# Consola en UTF-8 para ver correctamente los acentos de las respuestas.
[Console]::OutputEncoding = New-Object System.Text.UTF8Encoding($false)
$OutputEncoding = New-Object System.Text.UTF8Encoding($false)

function Wait-ForApi {
    param([int]$Seconds = 60)
    for ($i = 0; $i -lt $Seconds; $i++) {
        $code = curl.exe -s -k -o NUL -w "%{http_code}" "$BaseUrl/api/categories"
        if ($code -eq "200") {
            Write-Host "API disponible en $BaseUrl" -ForegroundColor DarkGray
            return
        }
        Start-Sleep -Seconds 1
    }
    Write-Host "La API no respondio a tiempo. Revisa: docker compose ps" -ForegroundColor Red
    exit 1
}

function Invoke-Demo {
    param(
        [Parameter(Mandatory)] [string]$Label,
        [Parameter(Mandatory)] [string]$Method,
        [Parameter(Mandatory)] [string]$Uri,
        [hashtable]$Body
    )

    $curlArgs = @("-s", "-k", "-X", $Method, $Uri, "-w", '\n__HTTP__%{http_code}')
    if ($Body) {
        $tmp = Join-Path $env:TEMP "catalog-body.json"
        [System.IO.File]::WriteAllText($tmp, ($Body | ConvertTo-Json -Compress))
        $curlArgs += @("-H", "Content-Type: application/json", "--data-binary", "@$tmp")
    }
    $raw = (& curl.exe @curlArgs) -join [char]10
    if ($Body) { Remove-Item $tmp -ErrorAction SilentlyContinue }

    if ($raw -match '(?s)(.*)[\r\n]+__HTTP__(\d+)\s*$') {
        $content = $Matches[1].TrimEnd()
        if ([string]::IsNullOrWhiteSpace($content)) { $content = "(sin cuerpo)" }
        $status = [int]$Matches[2]
    } else {
        $content = $raw
        $status = -1
    }

    $color = if ($status -eq 204 -or $status -eq 201 -or $status -eq 200) { "Green" } elseif ($status -lt 400 -and $status -gt 0) { "Yellow" } else { "Red" }
    Write-Host ""
    Write-Host ">>> $Label" -ForegroundColor Yellow
    Write-Host "  $Method  $Uri" -ForegroundColor Cyan
    Write-Host ("  HTTP {0}" -f $status) -ForegroundColor $color
    Write-Host "  $content"
}

function Section([string]$Title) {
    Write-Host ""
    Write-Host ("=" * 70) -ForegroundColor White
    Write-Host ("  " + $Title.ToUpper()) -ForegroundColor White
    Write-Host ("=" * 70) -ForegroundColor White
}

# ----------------------------------------------------------------------------
Start-Sleep -Seconds 6   # da margen al proxy de puertos tras recrear el contenedor
Wait-ForApi

# Guardarrail: el guion asume base limpia (debe listar [])
$initial = curl.exe -s -k "$BaseUrl/api/categories"
if ($initial -ne "[]") {
    Write-Host ""
    Write-Host "La base no esta vacia, pero el guion presume arrancar limpio." -ForegroundColor Red
    Write-Host "Para el video ejecuta ANTES:  docker compose down -v  &&  docker compose up -d" -ForegroundColor Yellow
    Write-Host "Datos actuales: $initial" -ForegroundColor Red
    exit 1
}

Section "A) Alta de categorias y productos"
# ----------------------------------------------------------------------------
Invoke-Demo "Lista vacia" GET "$BaseUrl/api/categories"

Invoke-Demo "Crear categoria: Entradas"            POST "$BaseUrl/api/categories" @{ name = "Entradas";          description = "Para compartir" }
Invoke-Demo "Crear categoria: Platos Principales"  POST "$BaseUrl/api/categories" @{ name = "Platos Principales"; description = "Platos fuertes" }
Invoke-Demo "Crear categoria: BEBIDAS (mayusculas)" POST "$BaseUrl/api/categories" @{ name = "BEBIDAS";            description = "Con hielo" }
Invoke-Demo "Crear categoria: Postres"             POST "$BaseUrl/api/categories" @{ name = "Postres";            description = "Dulces" }

Invoke-Demo "Listar categorias (4)" GET "$BaseUrl/api/categories"

Invoke-Demo "Producto: Ensalada Cesar"          POST "$BaseUrl/api/products" @{ name = "Ensalada Cesar";           description = "Lechuga, pollo, queso parmesano"; category = "Entradas" }
Invoke-Demo "Producto: Lomo Saltado"            POST "$BaseUrl/api/products" @{ name = "Lomo Saltado";             description = "Lomo, papas, arroz";                category = "Platos Principales" }
Invoke-Demo "Producto: Ceviche"                 POST "$BaseUrl/api/products" @{ name = "Ceviche";                  description = "Pescado fresco, limon";              category = "Platos Principales" }
Invoke-Demo "Producto: Coca-Cola"               POST "$BaseUrl/api/products" @{ name = "Coca-Cola";                description = "Gaseosa clasica";                    category = "BEBIDAS" }
Invoke-Demo "Producto: Inca Kola"               POST "$BaseUrl/api/products" @{ name = "Inca Kola";                description = "Gaseosa amarilla";                    category = "BEBIDAS" }
Invoke-Demo "Producto: Agua Mineral"            POST "$BaseUrl/api/products" @{ name = "Agua Mineral";             description = "Sin gas";                             category = "BEBIDAS" }
Invoke-Demo "Producto: Cheesecake de Maracuya"  POST "$BaseUrl/api/products" @{ name = "Cheesecake de Maracuya"; description = "Con salsa de maracuya";               category = "Postres" }

Invoke-Demo "Listar productos (7, con categoryName)" GET "$BaseUrl/api/products"

# ----------------------------------------------------------------------------
Section "B) Reglas de negocio (errores)"
# ----------------------------------------------------------------------------
Invoke-Demo "Categoria duplicada en minusculas 'bebidas' (NOCASE -> 409)" POST "$BaseUrl/api/categories" @{ name = "bebidas" }
Invoke-Demo "Borrar BEBIDAS que tiene productos (mensaje literal)"       DELETE "$BaseUrl/api/categories/3"
Invoke-Demo "Producto con categoria inexistente"                         POST "$BaseUrl/api/products" @{ name = "Fantasma"; category = "NoExiste" }
Invoke-Demo "Categoria inexistente (id 999)"                             GET "$BaseUrl/api/categories/999"
Invoke-Demo "Categoria sin nombre (400)"                                POST "$BaseUrl/api/categories" @{ name = "" }

# ----------------------------------------------------------------------------
Section "C) Actualizacion y eliminacion"
# ----------------------------------------------------------------------------
Invoke-Demo "Actualizar producto id 2 (PUT)"  PUT "$BaseUrl/api/products/2" @{ name = "Lomo Saltado Clasico"; description = "Con papas doradas y arroz"; category = "Platos Principales" }
Invoke-Demo "Eliminar producto id 7 (Cheesecake)" DELETE "$BaseUrl/api/products/7"
Invoke-Demo "Eliminar categoria id 4 (Postres, ya vacia)" DELETE "$BaseUrl/api/categories/4"

Invoke-Demo "Listar productos (6)"  GET "$BaseUrl/api/products"
Invoke-Demo "Listar categorias (3)" GET "$BaseUrl/api/categories"

# ----------------------------------------------------------------------------
Section "D) Persistencia: la base sobrevive al reinicio del contenedor"
# ----------------------------------------------------------------------------
Write-Host "Reiniciando el contenedor (docker compose restart)..."
Push-Location (Join-Path $PSScriptRoot "..")
$null = docker compose restart catalog-api 2>&1
Pop-Location
Wait-ForApi

Invoke-Demo "Categorias tras reiniciar (deben seguir las 3)" GET "$BaseUrl/api/categories"
Invoke-Demo "Productos tras reiniciar (deben seguir los 6)" GET "$BaseUrl/api/products"

Write-Host ""
Write-Host "FIN DE LA DEMO" -ForegroundColor Green