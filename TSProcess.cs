using LiteDB;

class TSProcess
{
    public ObjectId Id { get; set; }
    public string ProcessName { get; set; }
    public double PagedSystemMemorySize64 { get; set; }
    public double UserProcessorTime { get; set; }
    public string FileName { get; set; }

    public string TimeStamp { get; set; }
}