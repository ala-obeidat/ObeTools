using System;
using System.Threading;

using ObeTools;

// Usage: ObeTools.LogWriter <logDirectory> <logName> <marker> <count> <useMutex true|false> <startUtcTicks>
// Writes <count> info entries "<marker>-<i>" and exits with 1 when any write failed.
var directory = args[0];
var logName = args[1];
var marker = args[2];
var count = int.Parse(args[3]);
Logger.UseCrossProcessMutex = bool.Parse(args[4]);
Logger.WriteTimeoutMs = 10_000;

// Start together with the other process.
var start = new DateTime(long.Parse(args[5]), DateTimeKind.Utc);
while (DateTime.UtcNow < start)
{
    Thread.Yield();
}

int failed = 0;
for (int i = 0; i < count; i++)
{
    if (!Logger.TryLogInfo(logName, directory, $"{marker}-{i}", (object)null, "Main"))
    {
        failed++;
    }
}
Console.WriteLine($"failed={failed}");
return failed == 0 ? 0 : 1;
