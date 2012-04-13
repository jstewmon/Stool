using log4net.Config;

namespace Stool.Example
{
    public static class Bootstrap
    {
        public static void Run()
        {
            XmlConfigurator.Configure();
            new MyApp();
        }
    }
}