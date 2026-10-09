# ObeTools

C# .NET library (netstandard2.1 and net8.0) to help any developer in their project.
It collects what I have needed in every project over 10+ years of work.

### It has many static classes:
  - 1 => **Logger**: simple thread-safe daily text-file logger (no lost lines across threads or processes)
  - 2 => **SecretProtector**: authenticated encryption (AES-256-GCM) for tokens and links, with purpose binding and key rotation
  - 3 => **EncryptHasher**: SHA/HMAC hashing, PBKDF2 password hashing, constant-time compare, random numeric codes, safe base64 (and the legacy `Encrypt`/`Decrypt` obfuscation)
  - 4 => **ExcelHelper**: create/read Excel files (.xlsx) with a modern design, multiple sheets and typed cells
  - 5 => **TLVEncoding**: tag-length-value encoding (UTF-8 text and binary), as used by ZATCA QR codes
  - 6 => **VATHelper**: calculate VAT
  - 7 => **FileHelper**: detect a file's type from its content (base64 or bytes)
  - 8 => **NumberHelper**: amounts to words in English and Arabic, and rounding
  - 9 => **Base1024Encoding**: encode numbers and strings with a 1,024-character alphabet (compact in characters)

## Logger

```csharp
Logger.LogInfo("Orders", @"D:\Logs", "Order created", order, nameof(Create));
string text = Logger.LogException("Orders", @"D:\Logs", ex, "Checkout failed");

// Need to know whether the entry reached the disk?
bool written = Logger.TryLogInfo("Orders", logDir, "Order created", order, nameof(Create));
```

Each entry goes to `"{logName} yyyy-MM-dd - Info.txt"` (or `- Exception.txt`) in this exact shape, which log
scanners may rely on:

```
<blank line>
StringTime: HH:mm:ss:ff |  Caption: … |  Message: … |  Method: … |  FileName: … |  LineNumber: …
```

Settings (all static, all optional):

| Setting | Default | Meaning |
|---|---|---|
| `WriteTimeoutMs` | 2000 | How long one entry waits for another writer or a reader holding the file |
| `UseCrossProcessMutex` | false on Windows, true elsewhere | Serialise processes sharing a file with a named mutex |
| `IncludeStackTrace` | false | Append `\|  StackTrace: …` (full exception text) after LineNumber |
| `IncludeDatabaseDetail` | true | Include PostgreSQL `Detail` (may contain row values / customer data) |
| `RetentionDays` | null | Delete this log's daily files older than N days |
| `WriteFailed` event | | Raised with (path, exception) when an entry could not be written |

## SecretProtector

```csharp
byte[] key = SecretProtector.ParseKey(Environment.GetEnvironmentVariable("APP_PROTECTOR_KEY")); // 32 bytes, base64
string token = SecretProtector.Protect("order:42", key, purpose: "waybill-link");               // "v2.…", URL-safe

if (SecretProtector.TryUnprotect(token, key, "waybill-link", out var value)) { /* trusted */ }

// Key rotation: newest first
SecretProtector.TryUnprotect(token, new[] { newKey, oldKey }, "waybill-link", out value);
```

A token only opens with the same key **and** purpose, and any change to it makes `TryUnprotect` return false.
Create a key once with `Convert.ToBase64String(SecretProtector.GenerateKey())` and keep it in your secret store.

## Security notes

- `EncryptHasher.Encrypt` / `Decrypt` / `ToURLToken` / `FromURLToken` are **obfuscation, not encryption**. The key is
  a constant in this public repository and package, so anyone can decrypt and forge these tokens, and there is no
  integrity check. They are kept (output unchanged) so existing links keep working, and are marked `[Obsolete]`.
  Use `SecretProtector` for anything secret or anything a client must not be able to forge.
- `HashSha512` / `HashSha256` are fine for high-entropy values (API keys). For human passwords use
  `HashPassword` / `VerifyPassword` (PBKDF2-SHA256, 600,000 iterations, random salt).
- Compare hashes and HMACs with `EncryptHasher.FixedTimeEquals`, not `==`.
- `StripTags` (formerly `RemoveJavaScript`) removes tags; it is not an XSS defence. HTML-encode output instead.
- Use `GenerateNumericCode` for OTPs; it is cryptographically random.

## Numbers and VAT

```csharp
NumberHelper.NumberToWords(1234.56, NumberWordType.English, "ريال", "SAR", "هللة", "Halala");
// "One Thousand Two Hundred Thirty Four SAR and Fifty Six Halala"
NumberHelper.NumberToWords(1234.56, NumberWordType.Arabic, "ريال", "SAR", "هللة", "Halala");
// "ألف ومئتين وأربعة وثلاثون ريال وستة وخمسون هللة"

decimal vat = NumberHelper.RoundMoney(1.90m * 0.15m);                    // 0.29 (half away from zero)
var (net, vatAmount, gross) = VATHelper.SplitVat(115m, 15, amountIncludesVat: true); // 100.00, 15.00, 115.00
```

`NumberHelper.Round(double, digits)` keeps its historical behaviour (binary double, banker's rounding:
`Round(0.285, 2) = 0.28`). Use `RoundMoney`/`SplitVat` (decimal, half away from zero) for new money code.

## Excel

```csharp
ExcelHelper.WriteExcel(stream, "Invoices", headers, rows /* List<object[]>: numbers and dates keep their type */,
    new ExcelWriteOptions { RightToLeft = true, Properties = new ExcelDocumentProperties { Company = "My Co" } });
List<string[]> cells = ExcelHelper.GetCells(stream, skipFirstRow: true);
```

**Licence:** ExcelHelper uses EPPlus, which is free for non-commercial use only (EPPlus 5+). The host application
should declare its licence before calling it, e.g. `ExcelPackage.LicenseContext = LicenseContext.Commercial;`.
ObeTools never overwrites a licence you set; if none is set it falls back to NonCommercial (turn that off with
`ExcelHelper.UseNonCommercialLicenseWhenUnset = false`).

## Build

```
dotnet test ObeTools.sln -c Release
dotnet pack ObeTools.csproj -c Release -o artifacts
```

See [CHANGELOG.md](CHANGELOG.md) for behaviour changes between versions.
