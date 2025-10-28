// #define CHANNEL_SUPPORT
using System;
using System.Diagnostics.Tracing;


class AdvancedUsageDemo
{
    /// <summary>
    /// Demo of more advanced usage, with start and stop opcodes, tasks, keywords, levels, formatted messages.
    /// </summary>
    public static void AdvancedETWUsage()
    {
        AdvancedEventSource.Log.AnotherEvent(1, "", 2);
        // A full eventLogger.  You create this unconditionally and is very cheap if not
        // activated.  See code:AdvancedEventSource about how to turn on logging.  

        // Here are a bunch of events (in a row!)
        AdvancedEventSource.Log.TaskCreate(0x10);

        AdvancedEventSource.Log.ImageStart(0x129, 35, "This is a relatively long name ");
        AdvancedEventSource.Log.TaskCreate(0x10);
        AdvancedEventSource.Log.RunStart(0x11);
        AdvancedEventSource.Log.RunStop(0x12);
        AdvancedEventSource.Log.EmptyEvent();
        AdvancedEventSource.Log.RunStart(0x111);
        AdvancedEventSource.Log.RunStop(0x112);
        AdvancedEventSource.Log.SetGuid(new Guid(0xc836fd3, 0xee92, 0x4301, 0xbc, 0x13, 0xd8, 0x94, 0x3e, 0xc, 0x1e, 0x77));
        AdvancedEventSource.Log.AnotherEvent(100, "(Numbers should be 100, 101)", 101);

        Console.WriteLine("Done writing some events.");
    }
}

/// <summary>
/// AdvancedEventSource is just a demonstration sample of some of the more advanced features of ETW.   
/// 
/// This EventSource also demonstrates the LocalizationResource attribute, which allows names and
/// format strings used by the EventSource to be localized to different languages using .NET
/// resource files.   TO use this feature create a resx file and put is fully qualified name
/// of the resource access class into the LocalizationResources property.
/// </summary>
[EventSource(LocalizationResources = "EventSourceDemo.AdvancedUseResources", Name = "MyCompany")]
internal sealed class AdvancedEventSource : EventSource
{
    // The singleton instance of the curEventSource. 
    static public AdvancedEventSource Log = new AdvancedEventSource();

    /// <summary>
    /// This is an example of an event method. Its arguments are information that is dumped into the log
    /// (as a strongly typed block of information). The code for these methods is always very simmilar.
    /// It first checks if the curEventSource is enabled and if so calls 'WriteEvent' with an event
    /// number (a unique number of this event in this curEventSource), and the arguments. The call is
    /// thread-safe, so there is no concern about logging from different threads.
    /// 
    /// Many eventSources will never do more that this (and thus never need custom attributes).  
    /// </summary>
    public void ImageStart(long ImageBase, long Size, string Name) { if (IsEnabled()) WriteEvent(1, ImageBase, Size, Name); }
    /// <summary>
    /// Events have 'level's associted with them (see code:EventLevel) that indicate the
    /// verbosity of the event.  By default it is code:EventLevel.Always.  If you wish
    /// the event to be something different you can assign a level using custom attributes.   When
    /// specifying the custom attribute you must also specify the event number (first parameter to
    /// WriteEvent) associated with the event.  
    /// </summary>
    /// <param name="Id"></param>
    [Event(2, Level = EventLevel.Error)]
    public void TaskCreate(long TaskId) { if (IsEnabled()) WriteEvent(2, TaskId); }
    /// <summary>
    /// Keywords is a 64 bit set that represents 'areas' of the curEventSource that can be turned on or off at
    /// collection time (as well as filter after the fact). You can specify all the keywords associated
    /// with event as a bit-set. You can define your own keywords by creating constants (see
    /// code:#Keywords) below.
    /// </summary>
    [Event(3, Keywords = Keywords.Loader | Keywords.Other, Task = Tasks.Run, Opcode = EventOpcode.Start)]
    public void RunStart(long TaskId) { if (IsEnabled()) WriteEvent(3, TaskId); }
    /// <summary>
    /// Opcode represent a 'standard' operation that the event is logging.  The most useful and common
    /// opcodes are 'Start' and 'Stop' which mark the begining and end of something (and tools can give you
    /// duration and nesting semantics).  The 'task' represents the general area that the opcode is
    /// operating on (in this case we define one called 'TaskRun').   See code:#Tasks for defined tasks. 
    /// </summary>
    /// <param name="Id"></param>
    [Event(4, Task = Tasks.Run, Opcode = EventOpcode.Stop, Version = 1)]
    public void RunStop(long TaskId) { if (IsEnabled()) WriteEvent(4, TaskId); }
    /// <summary>
    /// This example shows the support for the Guid type as well as the ability to have formatted messages.  
    /// </summary>
    /// <param name="MyGuid"></param>
    [Event(5, Message = "The SetGuid has a guid value of {0}.", Opcode = Opcodes.MyOpcode1)]
    public void SetGuid(Guid MyGuid) { if (IsEnabled()) WriteEvent(5, MyGuid); }

    public void AnotherEvent(int myInt, string myString, int mySecondInt) { if (IsEnabled()) WriteEvent(6, myInt, myString, mySecondInt); }
    public void EmptyEvent() { if (IsEnabled()) WriteEvent(7); }
    public void Message(string Message) { if (IsEnabled()) WriteEvent(8, Message); }

#if CHANNEL_SUPPORT
    // [Event(9, Level = EventLevel.Error, Channel = (EventChannel)Channels.AdvancedChannel)]
    public void AnEventWithAChannel(int myInt, string myString, int mySecondInt) { if (IsEnabled()) WriteEvent(9, myInt, myString, mySecondInt); }
#endif

    // #Keywords (notice they are bitsets (flags)).  Events can belong to any of these sets.
    // When you start the provider you can specify this bitmask that indicates which groups 
    // of events you wish to have.  
    public class Keywords
    {
        public const EventKeywords Loader = (EventKeywords)0x0001;
        public const EventKeywords Critical = (EventKeywords)0x0002;
        public const EventKeywords Other = (EventKeywords)0x0004;
    }

    // A task is a subsystem of your code.   Often they are used in conjuction with the
    // built in 'start' and 'stop' opcodes, which indicate that some particular task is
    // starting or has completed. 
    public class Tasks
    {
        public const EventTask Run = (EventTask)1;
    }

    // Typically you use the built in start and stop opcodes, but if you have a set of
    // tasks that all do simmiar operations, you can give the operations an opcode number
    // so that the reader of the event can treat opcodes from different tasks in the same
    // way.  
    public class Opcodes
    {
        public const EventOpcode MyOpcode1 = (EventOpcode)200;
        public const EventOpcode MyOpcode2 = (EventOpcode)200;
    }

#if CHANNEL_SUPPORT
    // #Channels channels are used to hook into the windows event logging infrastructure.  
    // By marking an event with a channel, you can cause the windows event logging to 
    // send it to different places.  
    public class Channels
    {
        [Channel(Type=ChannelTypes.Analytic, Enabled=true, Isolation="Application")]
        public const EventChannel AdvancedChannel = EventChannel.Reserved + 1;

        [Channel(ImportChannel = "Microsoft-Windows-BaseProvider/Admin")]
        public const EventChannel OtherChannel = EventChannel.Reserved + 2;
    }
#endif

    #region private
    /// <summary>
    /// This method shows an advanced feature where AdvancedEventSource gets a callback whenever 
    /// a ETW command is issued from a controller.  The callback is given whether the curEventSource
    /// should be enabled, the command being performance and a set of key-value pairs of strings
    /// that the controller can optionally provide. 
    /// 
    /// This feature is typically used to implement filtering as show below.   If the contoller
    /// sends and ETW command with a key 'ProcessID' then the eventSource will only turn on
    /// if its own process ID matches the given one.  Thus we have implemented per-process filtering.
    /// 
    /// In general other filtering can be done in a simmilar way.
    /// 
    /// Another thing that you can do is to create new commands.  For example you might want a command
    /// that forces a GC, or flushes caches.   
    /// 
    /// </summary>
    protected override void OnEventCommand(EventCommandEventArgs command)
    {
        if (!IsEnabled())
            return;

        // Implement process filtering.  Disable events if controller asked for just certain PIDs
        string value;
        if (command.Arguments.TryGetValue("ProcessID", out value))
        {
            System.Diagnostics.Process myProcess = System.Diagnostics.Process.GetCurrentProcess();
            int processID;
            if (!(int.TryParse(value, out processID) && myProcess.Id == processID))
            {
                for (int i = 0; ; i++)
                {
                    if (!command.EnableEvent(i))
                        break;
                }
            }
        }
    }
    #endregion
}

