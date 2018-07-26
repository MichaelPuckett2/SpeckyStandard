# SpeckyStandard
.NET Standard project to assist with DI, MVVM, XAML, and more...

-- WPF EXAMPLE: --
Below is a first commit example of using dependency injection:
In this example we make 3 different types, 1 interface and 2 models of the interface.
See the various ways that injection is annotated on the types.

[SpeckAttribute] is a class level annotation that injects a class for use.
[AutoSpecktAttribute] is a property, field, and parameter level annotiation that injects dependency values automatically.

    :::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::

    public interface ITestType
    {
        int Id { get; }
        string Name { get; }
    }

    // Inject the dependency as FirstTestType and ITestType and is registered as single instance.
    [Speck(typeof(ITestType))] 
    public class FirstTestType : ITestType
    {
        static int IdCounter;
        public int Id { get; set; } = ++IdCounter;
        public string Name { get; set; } = "Mathew " + IdCounter;
    }
    
    // Inject the dependency as SecondTestType and is registered as single instance.
    [Speck] 
    public class SecondTestType : ITestType
    {
        // Intializes FirstTestType via FirstTestType dependency.
        [AutoSpeck] 
        public FirstTestType FirstTestType { get; set; }

        public int Id => FirstTestType.Id;
        public string Name => "Mark " + Id;
    }
    
    ::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
    
    // Inject the dependency as TestTypeViewModel and is registered as single instance.
    [Speck] 
    public class TestTypeViewModel : NotifyBase
    {
        // Intializes firstTestType via FirstTestType dependency.
        [AutoSpeck] 
        private FirstTestType firstTestType;
        
        private SecondTestType secondTestType;
        private ITestType thirdTestType;

        public FirstTestType FirstTestType
        {
            get => firstTestType;
            set
            {
                firstTestType = value;
                Notify();
            }
        }

        // Intializes SecondTestType via SecondTestType dependency.
        [AutoSpeck] 
        public SecondTestType SecondTestType
        {
            get => secondTestType;
            set
            {
                secondTestType = value;
                Notify();
            }
        }

        // Intializes ThirdTestType via ITestType dependency.
        [AutoSpeck] 
        public ITestType ThirdTestType
        {
            get => thirdTestType;
            set
            {
                thirdTestType = value;
                Notify();
            }
        }
    }
    
    ::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
    
    // Inject the dependency as MainWindow and is registered as single instance.
    [Speck]
    public partial class MainWindow : Window
    {
        // Inject the ViewModel automatically into a base type.
        [AutoSpeck(typeof(TestTypeViewModel))]
        public INotifyPropertyChanged ViewModel { get; }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = ViewModel;
        }
    }   
    
    ::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
    
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            SpeckAutoStrapper.Start();
            Current.MainWindow = SpeckContainer.Instance.GetInstance<MainWindow>();
            Current.MainWindow.Show();
        }
    }

    ::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::

    **Data Access Layers**

    The RestPollingAttribute can be used to label a Speck as a data access layer for rest results.
    The default implementation of the RestPolling uses WebClient and Newtonsoft on a seperate thread to keep the RestData updated.
    Add the RestDataAttribute to the expected json result object type and let Specky do the work.
    After Specky has completed initializing via SpeckAutoStrapper.Start you can at anytime start or stop the Data Access Layers like so:
    SpeckControllersManager.StartControllers();

    If the type is inherited by NotifyBase then the RestData will automatically notify when udpated unless specified otherwise.
 
    Here's an example:
    Note the various ways of implementing IpAddressContext. All implementations are the same.
    ::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::

    public class IpAddress
    {
        public string Ip { get; set; }
    }

    [RestPolling("http://ip.jsontest.com/", Interval = 1000)]
    public class IpAddressContext : NotifyBase
    {
        [RestData(CanNotify = true)]
        public IpAddress IpAddress { get; set; }
    }

    // or...

    [RestPolling]
    public class IpAddressContext : NotifyBase
    {
        [RestData("http://ip.jsontest.com/")]
        public IpAddress IpAddress { get; set; }
    }

    // or...

    [RestPolling("http://")]
    public class IpAddressContext : NotifyBase
    {
        [RestData("ip.jsontest.com/")]
        public IpAddress IpAddress { get; set; }
    }
    
    
    
    
