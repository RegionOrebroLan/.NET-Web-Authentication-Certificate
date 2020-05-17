Import-Module "$($PSScriptRoot)\Certificate-Module";

$_maximumDate = [DateTime]::MaxValue;
$_minimumDate = [DateTime]::MinValue.AddYears(1899);

# RegionOrebroLan.Web.Authentication.Certificate.IntegrationTests.Claims.CertificatePrincipalFactoryTest

New-RootCertificate `
	-Name "Integration-test Root CA" `
	-NotAfter $_maximumDate `
	-NotBefore $_minimumDate;

New-ClientCertificate `
	-Name "Integration-test client-certificate" `
	-NotAfter $_maximumDate `
	-NotBefore $_minimumDate `
	-SignerName "Integration-test Root CA";

New-ClientCertificateWithEmailAndUpn `
	-Name "Integration-test client-certificate with email and UPN" `
	-NotAfter $_maximumDate `
	-NotBefore $_minimumDate `
	-SignerName "Integration-test Root CA";

# Samples: Application.Data

New-RootCertificate `
	-Name "Root" `
	-NotAfter $_maximumDate `
	-NotBefore $_minimumDate;

New-IntermediateCertificate `
	-Name "Intermediate-1" `
	-NotAfter $_maximumDate `
	-NotBefore $_minimumDate `
	-PathLength 2 `
	-SignerName "Root";

New-IntermediateCertificate `
	-Name "Intermediate-2" `
	-NotAfter $_maximumDate `
	-NotBefore $_minimumDate `
	-PathLength 1 `
	-SignerName "Intermediate-1";

New-IntermediateCertificate `
	-Name "Intermediate-3" `
	-NotAfter $_maximumDate `
	-NotBefore $_minimumDate `
	-SignerName "Intermediate-2";

New-ClientCertificate `
	-Name "Chained" `
	-NotAfter $_maximumDate `
	-NotBefore $_minimumDate `
	-SignerName "Root";

New-ClientCertificate `
	-Name "Chained-1" `
	-NotAfter $_maximumDate `
	-NotBefore $_minimumDate `
	-SignerName "Intermediate-1";

New-ClientCertificate `
	-Name "Chained-2" `
	-NotAfter $_maximumDate `
	-NotBefore $_minimumDate `
	-SignerName "Intermediate-2";

New-ClientCertificate `
	-Name "Chained-3" `
	-NotAfter $_maximumDate `
	-NotBefore $_minimumDate `
	-SignerName "Intermediate-3";

New-ClientCertificate `
	-Name "Self-signed" `
	-NotAfter $_maximumDate `
	-NotBefore $_minimumDate;

<#
New-RootCertificate `
	-Name "Test-root-certificate" `
	-NotAfter $_maximumDate `
	-NotBefore $_minimumDate;

New-RootCertificate `
	-Name "To old test-root-certificate" `
	-NotAfter $_minimumDate.AddYears(1) `
	-NotBefore $_minimumDate;

New-RootCertificate `
	-Name "To young test-root-certificate" `
	-NotAfter $_maximumDate `
	-NotBefore $_maximumDate.AddYears(-1);

New-ClientCertificate `
	-Name "Test-client-certificate" `
	-NotAfter $_maximumDate `
	-NotBefore $_minimumDate `
	-SignerName "Test-root-certificate";

New-ClientCertificate `
	-Name "Test-client-certificate with to old root-certificate" `
	-NotAfter $_maximumDate `
	-NotBefore $_minimumDate `
	-SignerName "To old test-root-certificate";

New-ClientCertificate `
	-Name "Test-client-certificate with to young root-certificate" `
	-NotAfter $_maximumDate `
	-NotBefore $_minimumDate `
	-SignerName "To young test-root-certificate";

New-ClientCertificate `
	-Name "To old test-client-certificate" `
	-NotAfter $_minimumDate.AddYears(1) `
	-NotBefore $_minimumDate `
	-SignerName "Test-root-certificate";

New-ClientCertificate `
	-Name "To young test-client-certificate" `
	-NotAfter $_maximumDate `
	-NotBefore $_maximumDate.AddYears(-1) `
	-SignerName "Test-root-certificate";

New-SslCertificate `
	-DnsName "*.company.com", "localhost", "127.0.0.1" `
	-Name "Test-SSL-certificate" `
	-NotAfter $_maximumDate `
	-NotBefore $_minimumDate `
	-SignerName "Test-root-certificate";

New-SslCertificate `
	-DnsName "*.company.com", "localhost", "127.0.0.1" `
	-Name "Test-SSL-certificate with to old root-certificate" `
	-NotAfter $_maximumDate `
	-NotBefore $_minimumDate `
	-SignerName "To old test-root-certificate";

New-SslCertificate `
	-DnsName "*.company.com", "localhost", "127.0.0.1" `
	-Name "Test-SSL-certificate with to young root-certificate" `
	-NotAfter $_maximumDate `
	-NotBefore $_minimumDate `
	-SignerName "To young test-root-certificate";

New-SslCertificate `
	-DnsName "*.company.com", "localhost", "127.0.0.1" `
	-Name "To old test-SSL-certificate" `
	-NotAfter $_minimumDate.AddYears(1) `
	-NotBefore $_minimumDate `
	-SignerName "Test-root-certificate";

New-SslCertificate `
	-DnsName "*.company.com", "localhost", "127.0.0.1" `
	-Name "To young test-SSL-certificate" `
	-NotAfter $_maximumDate `
	-NotBefore $_maximumDate.AddYears(-1) `
	-SignerName "Test-root-certificate";
#>