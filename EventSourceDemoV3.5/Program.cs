// Copyright (c) Microsoft Corporation.  All rights reserved
// This program uses code hyperlinks available as part of the HyperAddin Visual Studio plug-in.
// It is available from http://www.codeplex.com/hyperAddin 
using System;
using System.Diagnostics.Tracing;
using System.Diagnostics;

class Program
{
    /// <summary>
    /// A simple program that creates a new ETW (Event Tracing for Windows) event curEventSource
    /// (code:AdvancedEventSource) and generates a few (strongly typed) events associated with it
    /// </summary>
    static void Main(string[] args)
    {
        Console.WriteLine("This appication tests ETW events.");
        Console.WriteLine(@"perfMonitor monitorPrint eventSourceDemo.exe");
        Console.WriteLine("To run with logging, use PerfView (bing 'PerfView Download' for download)");
        Console.WriteLine(@"PerfView /OnlyProviders=*MinimalEventSource,*MyCompany run eventSourceDemo.exe");
        Console.WriteLine();
        Console.WriteLine("To run listener tests:");
        Console.WriteLine("    eventSourceDemo.exe /listener");

        // If you wish to catch errors during construction you need to do it explicitly   By default
        // Exceptions are not thrown.  Thus in debug code at least you should be checking for failure.
        // If you have a good place to log the error, you should do that.  
        if (MinimalEventSource.Log.ConstructionException != null)
            throw MinimalEventSource.Log.ConstructionException;

        // MinimalProivider shows you code that gets you loging events quickly
        MinimalEventSource.Log.Load(10, "MyFile");         // These are called all over the place

        MinimalEventSource.Log.Load(11, "AnotherFile");
        MinimalEventSource.Log.SendEnums(MyColor.Blue, MyFlags.Flag2 | MyFlags.Flag3);
        MinimalEventSource.Log.Message("This is a message.");
        MinimalEventSource.Log.SetOther(true, 123456789);

        // Show how fast we can log, and also demonstrate that no object are allocated (because
        var sw = Stopwatch.StartNew();
        for (int i = 0; i < 10000; i++)
            MinimalEventSource.Log.HighFreq(i);
        sw.Stop();
        Console.WriteLine("Each event took {0:f3} usec to log.", sw.Elapsed.TotalSeconds * 100);

        // Examples of more advanced usage. 
        AdvancedUsageDemo.AdvancedETWUsage();

        if (args.Length > 0 && args[0] == "/listener")
            EventListenerDemo.UserDefinedEventListeners();

        Console.WriteLine("Hit return to exit");
        Console.ReadLine();
    }
}

/// <summary>
/// MinimualEventSource shows how to get started writing the least number of lines of code.  4 lines of
/// code are needed, to declare 2 events.  See code:AdvancedEventSource for a more advanced complete example 
/// </summary>
sealed class MinimalEventSource : EventSource
{
    public static MinimalEventSource Log = new MinimalEventSource();
    public void Load(long ImageBase, string Name) { WriteEvent(1, ImageBase, Name); }
    public void Unload(long ImageBase) { WriteEvent(2, ImageBase); }
    public void SetGuid(Guid MyGuid) { WriteEvent(3, MyGuid); }
    public void SendEnums(MyColor color, MyFlags flags) { WriteEvent(4, (int)color, (int)flags); }    // Cast enums to int for efficient logging.  
    public void Message(string Message) { WriteEvent(5, Message); }
    public void SetOther(bool flag, int myInt) { WriteEvent(6, flag, myInt); }
    public void HighFreq(int value) { if (IsEnabled()) WriteEvent(7, value); }
}

enum MyColor
{
    Red,
    Blue,
    Green,
}

[Flags]
enum MyFlags
{
    Flag1 = 1,
    Flag2 = 2,
    Flag3 = 4,
}
