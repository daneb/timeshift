using System.Diagnostics;

bool safe = true;

while (safe)
{

    Process[] processes = Process.GetProcesses();

    foreach (Process process in processes)
    {
        try
        {
            TSProcess p = new TSProcess() {
                ProcessName = process.ProcessName,
                PagedSystemMemorySize64 = process.PagedSystemMemorySize64 / 1024,
                UserProcessorTime = process.UserProcessorTime.TotalSeconds,
                FileName = process?.MainModule?.FileName
            };

            Console.WriteLine($"{process.ProcessName},{process.PagedSystemMemorySize64 / 1024},{process.UserProcessorTime},{process?.MainModule?.FileName}");
        }
        catch (System.Exception ex)
        {
            WriteColoredLine($"{ex.Message}: {process.ProcessName}", ConsoleColor.Red);
        }
    }

    Thread.Sleep(1000);


}


static void WriteColoredLine(string message, ConsoleColor color)
{
    Console.ForegroundColor = color;
    Console.WriteLine(message);
    Console.ResetColor();
}