using System.Diagnostics;
using LiteDB;

bool safe = true;
Dictionary<string, double> processesSum = new Dictionary<string, double>(); 

while (safe)
{

    using (var db = new LiteDatabase(@"C:\Temp\Timeshift.db"))
    {
        var col = db.GetCollection<TSProcess>($"processes_{DateTime.Now.ToString("yyyyMMdd")}");
        Process[] processes = Process.GetProcesses();

        foreach (Process process in processes)
        {
            try
            {
                TSProcess p = new TSProcess()
                {
                    Id = ObjectId.NewObjectId(),
                    ProcessName = process.ProcessName,
                    PagedSystemMemorySize64 = process.PagedSystemMemorySize64 / 1024,
                    UserProcessorTime = process.UserProcessorTime.TotalSeconds,
                    FileName = process?.MainModule?.FileName,
                    TimeStamp = DateTime.Now.ToString("yyyy-MM-dd")
                };

                // Summarize time for each of the same process
                if (processesSum.ContainsKey(p.ProcessName))
                {
                    processesSum[p.ProcessName] += p.UserProcessorTime;
                }
                else
                {
                    processesSum.Add(p.ProcessName, p.UserProcessorTime);
                }

                Console.WriteLine($"{process.ProcessName},{process.PagedSystemMemorySize64 / 1024},{process.UserProcessorTime},{process?.MainModule?.FileName}");
                var existing = col.Find(x => x.FileName == p.FileName).FirstOrDefault();
                if (existing == null)
                {
                    col.Insert(p);
                }
                else
                {
                    if (processesSum.ContainsKey(p.ProcessName))
                        p.UserProcessorTime = processesSum[p.ProcessName];
                    
                    col.Update(existing.Id, p);
                }
            }
            catch (System.Exception ex)
            {
                WriteColoredLine($"{ex.Message}: {process.ProcessName}", ConsoleColor.Red);
            }
        }

        var processCollection = db.GetCollection<TSProcess>("process");
        var result = processCollection.Query()
        .ToEnumerable()
        .Select(p =>
        new
        {
            ProcessName = p.ProcessName,
            PagedSystemMemorySize64 = p.PagedSystemMemorySize64 / 1024,
            UserProcessorTime = p.UserProcessorTime,
            FileName = p.FileName,
            TimeStamp = p.TimeStamp
        });

        var totals = result.GroupBy(g => g.FileName).Select(x => new
        {
            Name = x.Key,
            Total = x.Sum(s => s.UserProcessorTime)
        }).OrderBy(o => o.Total);

        foreach (var item in totals)
        {
            WriteColoredLine($"Process: {item.Name} {item.Total}", ConsoleColor.Green);
        }

    }

    safe = false;

    Thread.Sleep(1000);
}


static void WriteColoredLine(string message, ConsoleColor color)
{
    Console.ForegroundColor = color;
    Console.WriteLine(message);
    Console.ResetColor();
}