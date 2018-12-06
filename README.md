# SpeckyStandard

Simplified injection:

1. Introduce a Speck

[Speck]
public class Log
{
    public void Print(string message)
    {
        Console.WriteLine(message);
    }
}

2. Introduce another Speck then intitialize with existing Specks.

[Speck]
public class Worker
{
    [SpeckAuto]
    readonly Log log;
}

3. Start operations when intilization is complete.

public class Worker
{
    [SpeckAuto]
    readonly Log log;

    [SpeckPost]
    public void Start()
    {
        Log.Print("Working...");
        //...
    }
}

---------------------------------------------------------------------------------------------------

1. Add configurations.

[SpeckConfiguration("Test")]
class TestConfiguration
{
    readonly string Reason = "Testing";
    readonly List<string> Names = new List<string>()
    {
        "Mathew", "Mark", "Luke", "John"
    };
}

2. Inject configurations.

[SpeckConfigurationAuto]
public List<string> Names { get; }

3. Choose configuration on startup or via file / url.

SpeckAutoStrapper.Start(configuration: "Test");

--------------------------------------------------------------------------------------------------


All this and more plus more to come.

Specky is open source and free to use and distribute.  

'I love, worship, and praise God with all my heart, might, and strength!' - (Michael Brian Puckett, II)