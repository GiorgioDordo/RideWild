using System;
using System.Collections.Generic;

namespace RideWild.Models.DataModels;

public partial class LogError
{
    public int Id { get; set; }

    public string ClassName { get; set; } = null!;

    public string MethodName { get; set; } = null!;

    public string ActionName { get; set; } = null!;

    public int Line { get; set; }

    public string Message { get; set; } = null!;

    public DateTime Time { get; set; }
}
