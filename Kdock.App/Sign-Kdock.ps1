param(
    [Parameter(Mandatory = $true)]
    [string] $Exe
)

$ErrorActionPreference = "Stop"
try {
    if (-not (Test-Path $Exe)) {
        return
    }

    $subject = "CN=Decimal Tech, O=Decimal Tech"
    $cert = Get-ChildItem Cert:\CurrentUser\My -CodeSigningCert -ErrorAction SilentlyContinue |
        Where-Object { $_.Subject -like "*Decimal Tech*" -and $_.HasPrivateKey -and $_.NotAfter -gt (Get-Date) } |
        Select-Object -First 1

    if ($null -eq $cert) {
        $cert = New-SelfSignedCertificate `
            -Type CodeSigningCert `
            -Subject $subject `
            -FriendlyName "Decimal Tech Kdock" `
            -KeyExportPolicy Exportable `
            -KeySpec Signature `
            -HashAlgorithm SHA256 `
            -NotAfter (Get-Date).AddYears(10) `
            -CertStoreLocation "Cert:\CurrentUser\My"
    }

    Set-AuthenticodeSignature -FilePath $Exe -Certificate $cert -HashAlgorithm SHA256 | Out-Null

    $publisher = "Cert:\CurrentUser\TrustedPublisher"
    if (-not (Get-ChildItem $publisher -ErrorAction SilentlyContinue | Where-Object { $_.Thumbprint -eq $cert.Thumbprint })) {
        $export = Join-Path $env:TEMP "DecimalTech-Kdock.cer"
        Export-Certificate -Cert $cert -FilePath $export | Out-Null
        Import-Certificate -FilePath $export -CertStoreLocation $publisher | Out-Null
        Remove-Item $export -Force -ErrorAction SilentlyContinue
    }
}
catch {
    Write-Host "Signing skipped: $($_.Exception.Message)"
    exit 0
}
