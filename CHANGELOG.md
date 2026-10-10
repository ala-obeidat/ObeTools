# Changelog

## 1.7.1

- **Behaviour change (Arabic NumberToWords):** when a count of thousands, millions or billions is over 100 and ends
  in 1 or 2, the last one or two are now written as their own word:
  501,000 is "خمسمائة ألف وألف" (was "خمسمائة وواحد ألف"), 602,000 is "ستمائة ألف وألفين",
  201,000,000 is "مئتين مليون ومليون". Checked over 1,999,999 amounts: only these cases change, and English is
  unchanged.

## 1.7.0

Safe, additive release: existing tokens, hashes, TLV bytes and log line format are unchanged.
Items marked **Behaviour change** are bug fixes that change output; read them before upgrading.

### Logger
- **Fixed: log lines were silently lost** when two threads (or processes) wrote at the same moment, or when a
  viewer held the file open. 1.6.7 kept 8,702 of 16,000 lines with 8 threads; 1.7.0 keeps all of them.
  Entries are written with one write under a per-file lock, and retried with back-off (up to `WriteTimeoutMs`)
  while another process holds the file.
- Cross-process: optional named mutex per file (`UseCrossProcessMutex`; on by default on Linux/macOS, where file
  sharing is advisory, off on Windows).
- New: `TryLogInfo` / `TryLogException` return whether the entry was written; `WriteFailed` event.
- New opt-ins: `IncludeStackTrace` (appended after LineNumber), `IncludeDatabaseDetail` (default true),
  `RetentionDays`.
- A caption or exception message containing a line break followed by `StringTime:` can no longer forge a log entry
  (it is written as `(StringTime):`).
- The file date and `StringTime` now come from the same moment (no more entries at 23:59 in the next day's file).
- `StringTime` is formatted with the invariant culture (always `HH:mm:ss:ff`).
- FileName is no longer empty for Linux paths and deterministic builds (`/_/src/...`); Windows paths give the same
  text as before.
- Database details are also found inside `AggregateException` branches.
- `JsonSerializerOptions` and created directories are cached.

### Security
- New `SecretProtector`: AES-256-GCM tokens (`v2.` prefix) with purpose binding and key rotation.
- `Encrypt`, `Decrypt`, `ToURLToken`, `FromURLToken` are now `[Obsolete]`: they are obfuscation with a public key and
  no integrity check. Their output is unchanged; tokens created by 1.6.x still decrypt.
- **Behaviour change:** `Encrypt` throws `ArgumentException` for input longer than 2,502 characters (was
  `ArgumentOutOfRangeException`) and for a salt shorter than 4 characters (used to produce a token that could
  never be decrypted).
- New: `HashPassword` / `VerifyPassword` (PBKDF2-SHA256, 600,000 iterations), `FixedTimeEquals`, `TryHashSha512`,
  `GenerateNumericCode` (cryptographically random, exact digit count).
- `GenerateRandomNumber` (returns digit+1 digits) and `CustomEncrypt` (drops a character on odd lengths) are
  `[Obsolete]`, behaviour unchanged.
- `RemoveJavaScript` is renamed `StripTags` (old name obsolete). It now also removes unterminated and multi-line tags.
- `HashHmacSha512/256` with a null secret throw `ArgumentNullException(secret)`.
- Hash output (uppercase hex, no dashes) is unchanged; no longer uses the obsolete `SHA*Managed` types.
- `FromSafeBase64` computes padding exactly (the `tries` parameter is ignored); new `TryFromSafeBase64`.

### NumberHelper / VATHelper
- **Behaviour change (NumberToWords):** rewritten on `decimal` arithmetic, independent of the current culture. Fixed:
  - 9.999 returned an empty string (now "Ten SAR"); `NumberWordType.All` threw for it.
  - Under `ar-SA`/`fr-FR` CurrentCulture, 1.5 became "One Hundred SAR".
  - 1,000,001 gave "One Million Thousand One".
  - Amounts below 1 had no "Zero" ("SAR and Fifty Halala").
  - "Fourty" is now "Forty".
  - Arabic: 3-10 thousands/millions/billions take the plural (ثمانية آلاف, ثلاثة ملايين); 11-99 take the accusative
    singular (أحد عشر ألفاً, عشرون ألفاً); 11 is أحد عشر.
  - Leading and doubled spaces are gone ("One SAR and Fifty Halala").
  - Rounding to halalas is half away from zero (1.005 => "One SAR and One Halala").
  - Amounts of 10^12 or more, NaN and infinity throw `ArgumentOutOfRangeException` (errors are no longer swallowed
    into blank text).
  
  All other amounts produce the same words as 1.6.7 (checked over 80 amounts). The Arabic table in
  `tests/ObeTools.Tests/NumberHelperTests.cs` should be reviewed by a native speaker.
- New: `AmountToWords(decimal, …)`, `RoundMoney(decimal, digits)` (half away from zero),
  `VATHelper.SplitVat(amount, ratePercent, amountIncludesVat)`, `RoundObject(input, digits, MidpointRounding)`
  (double, double?, decimal, decimal?).
- `Round(double, digits)`, `GetItemRate` and `GetRate` are unchanged (stored invoices were computed with them).
  `GetRate`'s parameter meaning with `rateIncludeVAT` is now documented.
- `RoundObject(input, digits)` skips read-only properties instead of throwing.

### TLVEncoding
- **Behaviour change:** a tag above 255 or a value longer than 255 bytes now throws `ArgumentOutOfRangeException`
  (it used to silently corrupt the QR code, e.g. for an Arabic seller name over 127 characters).
- New binary overload `Encode(IEnumerable<(int Tag, byte[] Value)>)` for ZATCA phase-2 tags 6-9.
- Encoded bytes for valid input are unchanged.

### FileHelper
- Detection by magic bytes; no exception for short input.
- **Behaviour change:** ZIP files are inspected: xlsx, pptx, apk and plain zip are no longer reported as "docx".
  Real docx files still return "docx". Any RIFF file is no longer "wav" (WEBP and AVI are recognised).
- New: GIF, WEBP, HEIC, AVIF, M4A, 3GP, AVI; `MimeType`, `SizeInBytes` (long).
- New correctly spelled `GetExtension`, `FileInformation.Extension`, `FileInfoType.Unknown`; the misspelled names
  remain as `[Obsolete]` aliases. New `FileInfoType` values: Spreadsheet, Presentation, Archive.
- New `GetAllInfo(byte[])`.

### ExcelHelper
- Fixed: `dataDesign` styled only the first data row; zero rows with a `dataDesign` threw.
- Fixed: `GetCells` threw `NullReferenceException` on an empty sheet (now returns an empty list).
- Cells are no longer rich text (faster, smaller files). Packages are disposed (your stream stays open).
- **Behaviour change:** files no longer carry Author "Ala Obeidat", Company "JITBS" or custom properties; set your
  own with `ExcelWriteOptions.Properties`.
- New `WriteExcel` with typed rows (`List<object[]>` / `ExcelSheetModel.Rows`): numbers, dates and booleans keep
  their type.
- Licence: a licence the host set is never overwritten (1.6 always forced NonCommercial). If none is set, the
  NonCommercial fallback still applies unless `UseNonCommercialLicenseWhenUnset = false`.

### Base1024Encoding
- Fixed: the symbol table had 1,025 entries with 9 duplicates, so 512 of the numbers 0-65,535 did not round-trip.
  Only the duplicate slots changed: values that round-tripped before encode the same.
- Fixed: decoded strings ended with an extra `'\0'`; strings encoded by 1.6.x decode correctly.
- The last separator character is now used; negative numbers and unknown symbols throw.

### Build and packaging
- Builds from any folder (the icon path was absolute) and no longer packs on every build.
- Targets `netstandard2.1` and `net8.0`; explicit `System.Text.Json` reference for netstandard2.1.
- SourceLink, symbol package (snupkg), deterministic CI builds, warnings as errors in Release.
- xUnit test project (224 tests) and GitHub Actions CI (Windows and Linux; publish to nuget.org on a `v*` tag).

## 1.6.7
- Fix FromSafeBase64 loop
