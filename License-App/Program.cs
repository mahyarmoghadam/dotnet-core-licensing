using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

class Program
{
    static void Main()
    {
        GenerateLicense();
        LoadLicense();
    }

    private static void LoadLicense()
    {
        var licenseFileContent = File.ReadAllText("license.inc");
        var licenseData = JsonSerializer.Deserialize<License>(licenseFileContent) ?? throw new ArgumentNullException("JsonSerializer.Deserialize<License>(licenseFileContent)");

        if (VerifyLicenseSignature(licenseData))
        {
            if (licenseData.ExpirationDate >= DateTime.UtcNow)
            {
                Console.WriteLine(licenseData.LicenseType == "Standard"
                    ? "License is valid."
                    : "Invalid license type.");
            }
            else
            {
                Console.WriteLine("License has expired.");
            }
        }
        else
        {
            Console.WriteLine("Invalid license signature.");
        }
    }

    private static void GenerateLicense()
    {
        var licenseData = new License
        {
            LicenseType = "Standard",
            ExpirationDate = DateTime.UtcNow.AddYears(1), // Example: License valid for 1 year
            CustomerName = "John Doe",
        };

        var signature = SignLicenseData(licenseData);

        licenseData.Signature = Convert.ToBase64String(signature);
        var licenseJson = JsonSerializer.Serialize(licenseData);
        File.WriteAllText("license.inc", licenseJson);

        Console.WriteLine("License file 'license.inc' created successfully.");
    }

    static byte[] SignLicenseData(License licenseData)
    {
        using var rsa = new RSACryptoServiceProvider();
        rsa.ImportFromPem(GetPrivateKey()); 

        var licenseBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(licenseData));

        return rsa.SignData(licenseBytes, HashAlgorithmName.SHA1, RSASignaturePadding.Pkcs1);
    }
    
    static bool VerifyLicenseSignature(License licenseData)
    {
        using var rsa = new RSACryptoServiceProvider();
        rsa.ImportFromPem(GetPublicKey()); 
        var signature = licenseData.Signature;
        
        licenseData.Signature = null;
        
        var licenseBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(licenseData));

        var signatureBytes = Convert.FromBase64String(signature);

        return rsa.VerifyData(licenseBytes, signatureBytes, HashAlgorithmName.SHA1, RSASignaturePadding.Pkcs1);
    }

    static string GetPrivateKey()
    {
        // Replace with your private key or load it from a secure location
        return @"-----BEGIN RSA PRIVATE KEY-----
MIICWwIBAAKBgHmu06DaeA+N0Qm/ofPcdzQYyTqlAP+s/0s7+X7ElwixP7N8WYyW
Q896fQIb1Z3x0oqqEHAPcv8Yl3r+vkWSWQX1UqjNkrGm830tkBvmfIUp7blmXrCJ
htZG/bolFXCf0Gpi1g8sgxnBeErFhPW0uIcL5PklS6PpIon2nKIFrMH9AgMBAAEC
gYA5+jzHdZCjCJVDKdWGldMONYkbsibpq4nwVOEpr42vDJUndeZNAAPLRbduW8jK
esAwZZtzaUkHlrYGWn5aM8LKVHlVY07Z5GP/Q4OyaLOU49b8XOW+Kmcdic132W4g
+t22hIWbjxWyco3bA1D2graw1bAesaZ6zkCYepzX+u7dmQJBAMgC94a2OScI7f/u
JlTyctEe+h01ey2jHxmcPZOYtx3k2xP8HSfKzSqkZHXLGT/erSqhhkLIqohh0Dc0
79TdE88CQQCbvr9r+yvzQDOXRJ00OfYVe/WLBnSRDZv79AaalmX5oEFbT82ti5sc
Qr6B3fOUCzmVcBPA8Cv0luRR2TzTYGRzAkA67czaDuRF4PamWhdHHevAO107r98r
8gyesg7eZrdFAoGdoMFCURkjwC2tGvrEe6oPjmmNUawU5KTBL0KeN8i3AkBsAc9s
SgaDrg1ZJQtEMcH3yjxRSovCIEcBZozB3fUgNUO92E0RwlQyOBM3qr2F+HbZrJz2
W1iQSahTHq0xBZMDAkEAlOSQ7wtSUmn2atvNfxcX/H2vysI8LfQlmIOrHeYWKgaU
EbmvZfmUH3jjgilM9XyR1AwU14aXCgLHKov+gx7OtQ==
-----END RSA PRIVATE KEY-----";
    }

    static string GetPublicKey()
    {
        return @"-----BEGIN PUBLIC KEY-----
MIGeMA0GCSqGSIb3DQEBAQUAA4GMADCBiAKBgHmu06DaeA+N0Qm/ofPcdzQYyTql
AP+s/0s7+X7ElwixP7N8WYyWQ896fQIb1Z3x0oqqEHAPcv8Yl3r+vkWSWQX1UqjN
krGm830tkBvmfIUp7blmXrCJhtZG/bolFXCf0Gpi1g8sgxnBeErFhPW0uIcL5Pkl
S6PpIon2nKIFrMH9AgMBAAE=
-----END PUBLIC KEY-----";
    }
}
