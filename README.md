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

    [Speck(typeof(ITestType))] // Inject the dependency as FirstTestType and ITestType and is registered as single instance.
    public class FirstTestType : ITestType
    {
        static int IdCounter;
        public int Id { get; set; } = ++IdCounter;
        public string Name { get; set; } = "Mathew " + IdCounter;
    }
    
    [Speck] // Inject the dependency as SecondTestType and is registered as single instance.
    public class SecondTestType : ITestType
    {
        [AutoSpeck] // Intializes FirstTestType via FirstTestType dependency.
        public FirstTestType FirstTestType { get; set; }

        public int Id => FirstTestType.Id;
        public string Name => "Mark " + Id;
    }
    
    ::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
    
    [Speck] // Inject the dependency as TestTypeViewModel and is registered as single instance.
    public class TestTypeViewModel : NotifyBase
    {
        [AutoSpeck] // Intializes firstTestType via FirstTestType dependency.
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

        [AutoSpeck] // Intializes SecondTestType via SecondTestType dependency.
        public SecondTestType SecondTestType
        {
            get => secondTestType;
            set
            {
                secondTestType = value;
                Notify();
            }
        }

        [AutoSpeck] // Intializes ThirdTestType via ITestType dependency.
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
    
    
    [Speck] // Inject the dependency as MainWindow and is registered as single instance.
    public partial class MainWindow : Window
    {
        [AutoSpeck] // Intializes ThirdTestType via TestTypeViewModel dependency.
        public TestTypeViewModel TestTypeViewModel { get; private set; }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = TestTypeViewModel;
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
    
    
    
