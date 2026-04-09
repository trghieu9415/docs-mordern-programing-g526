$ErrorActionPreference = "Stop"

$baseUrl = "http://localhost:5273"
$containerName = "chapter06-postgres"
$dbName = "chapter06_identity"
$dbUser = "root"
$suffix = Get-Date -Format "yyyyMMddHHmmss"

function Get-ErrorResponse {
    param([System.Management.Automation.ErrorRecord]$ErrorRecord)

    $response = $ErrorRecord.Exception.Response
    if ($null -eq $response) {
        return [pscustomobject]@{
            StatusCode = 0
            Body = [pscustomobject]@{
                error = $ErrorRecord.Exception.Message
            }
        }
    }

    if ($response -is [System.Net.Http.HttpResponseMessage]) {
        $content = $ErrorRecord.ErrorDetails.Message
        $statusCode = [int]$response.StatusCode
    }
    else {
        $reader = New-Object System.IO.StreamReader($response.GetResponseStream())
        $content = $reader.ReadToEnd()
        $reader.Dispose()
        $statusCode = [int]$response.StatusCode
    }

    [pscustomobject]@{
        StatusCode = $statusCode
        Body = if ([string]::IsNullOrWhiteSpace($content)) { $null } else { $content | ConvertFrom-Json }
    }
}

function Invoke-Api {
    param(
        [string]$Method,
        [string]$Url,
        [object]$Body
    )

    try {
        $requestArgs = @{
            Uri = $Url
            Method = $Method
            Headers = @{ Accept = "application/json" }
        }

        if ($null -ne $Body) {
            $requestArgs["ContentType"] = "application/json"
            $requestArgs["Body"] = ($Body | ConvertTo-Json -Depth 10)
        }

        $response = Invoke-WebRequest @requestArgs

        [pscustomobject]@{
            StatusCode = [int]$response.StatusCode
            Body = if ([string]::IsNullOrWhiteSpace($response.Content)) { $null } else { $response.Content | ConvertFrom-Json }
        }
    }
    catch {
        Get-ErrorResponse -ErrorRecord $_
    }
}

function ConvertFrom-Base64Url {
    param([string]$Value)

    $padded = $Value.Replace('-', '+').Replace('_', '/')
    switch ($padded.Length % 4) {
        2 { $padded += '==' }
        3 { $padded += '=' }
    }

    [System.Text.Encoding]::UTF8.GetString([Convert]::FromBase64String($padded))
}

function Decode-JwtPayload {
    param([string]$Token)

    $parts = $Token.Split('.')
    if ($parts.Length -lt 2) {
        throw "JWT không hợp lệ"
    }

    (ConvertFrom-Base64Url -Value $parts[1]) | ConvertFrom-Json
}

function Get-ClaimValue {
    param(
        [object]$Payload,
        [string]$ClaimName
    )

    ($Payload.PSObject.Properties | Where-Object { $_.Name -eq $ClaimName } | Select-Object -First 1).Value
}

function Invoke-Psql {
    param([string]$Sql)

    docker exec $containerName psql -U $dbUser -d $dbName -t -A -c $Sql
}

$loginUserEmail = "login-$suffix@example.com"
$lockoutUserEmail = "lockout-$suffix@example.com"
$refreshUserEmail = "refresh-$suffix@example.com"
$password = "User@123"

$registerLoginUser = Invoke-Api -Method "POST" -Url "$baseUrl/api/user/auth/register" -Body @{
    userName = "login_$suffix"
    email = $loginUserEmail
    password = $password
}

$loginSuccess = Invoke-Api -Method "POST" -Url "$baseUrl/api/user/auth/login" -Body @{
    email = $loginUserEmail
    password = $password
}

$accessToken = $loginSuccess.Body.data.access.token
$refreshToken = $loginSuccess.Body.data.refresh.token
$decodedPayload = Decode-JwtPayload -Token $accessToken
$nameIdentifierClaim = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"
$roleClaim = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"

$refreshSuccess = Invoke-Api -Method "POST" -Url "$baseUrl/api/user/auth/refresh" -Body @{
    refreshToken = $refreshToken
}

$registerLockoutUser = Invoke-Api -Method "POST" -Url "$baseUrl/api/user/auth/register" -Body @{
    userName = "lockout_$suffix"
    email = $lockoutUserEmail
    password = $password
}

$lockoutAttempts = @()
foreach ($attempt in 1..5) {
    $response = Invoke-Api -Method "POST" -Url "$baseUrl/api/user/auth/login" -Body @{
        email = $lockoutUserEmail
        password = "Wrong@123"
    }

    $lockoutAttempts += [pscustomobject]@{
        Attempt = $attempt
        StatusCode = $response.StatusCode
        Error = $response.Body.error
    }
}

$registerRefreshUser = Invoke-Api -Method "POST" -Url "$baseUrl/api/user/auth/register" -Body @{
    userName = "refresh_$suffix"
    email = $refreshUserEmail
    password = $password
}

$refreshUserLogin = Invoke-Api -Method "POST" -Url "$baseUrl/api/user/auth/login" -Body @{
    email = $refreshUserEmail
    password = $password
}

$refreshUserToken = $refreshUserLogin.Body.data.refresh.token
$refreshUserPayload = Decode-JwtPayload -Token $refreshUserLogin.Body.data.access.token
$refreshUserId = Get-ClaimValue -Payload $refreshUserPayload -ClaimName $nameIdentifierClaim
$newSecurityStamp = "MANUAL-STAMP-$suffix"

$updateSql = "update ""Users"" set ""SecurityStamp"" = '$newSecurityStamp' where ""Id"" = '$refreshUserId';"
$updatedRows = Invoke-Psql -Sql $updateSql

$refreshRejected = Invoke-Api -Method "POST" -Url "$baseUrl/api/user/auth/refresh" -Body @{
    refreshToken = $refreshUserToken
}

[pscustomobject]@{
    Login = [pscustomobject]@{
        RegisterStatus = $registerLoginUser.StatusCode
        LoginStatus = $loginSuccess.StatusCode
        AccessTokenExpiresAt = $loginSuccess.Body.data.access.expiredAt
        RefreshTokenExpiresAt = $loginSuccess.Body.data.refresh.expiredAt
    }
    DecodedAccessToken = [pscustomobject]@{
        NameIdentifier = Get-ClaimValue -Payload $decodedPayload -ClaimName $nameIdentifierClaim
        Role = Get-ClaimValue -Payload $decodedPayload -ClaimName $roleClaim
        SecurityStamp = Get-ClaimValue -Payload $decodedPayload -ClaimName "SecurityStamp"
    }
    RefreshSuccess = [pscustomobject]@{
        StatusCode = $refreshSuccess.StatusCode
        HasAccessToken = [bool]$refreshSuccess.Body.data.access.token
        HasRefreshToken = [bool]$refreshSuccess.Body.data.refresh.token
    }
    Lockout = $lockoutAttempts
    SecurityStampRefreshRejection = [pscustomobject]@{
        UpdatedRows = ($updatedRows | Out-String).Trim()
        StatusCode = $refreshRejected.StatusCode
        Error = $refreshRejected.Body.error
    }
} | ConvertTo-Json -Depth 10
