using System;
using System.IO;
using System.Reflection;
using Topshelf;

namespace FiddService
{
    class Program
    {
        static void Main(string[] args)
        {
            HostFactory.New(x =>
            {
                x.Service<FiddPollingService>();
                x.SetServiceName("FiddRSSPollingService");
                x.SetDisplayName("RSS polling service for Fidd");
                x.SetDescription("Polls subscribed RSS feeds from Fidd peridiocally and updates their content.");
                x.SetStartTimeout(TimeSpan.FromMinutes(1));
                x.EnableServiceRecovery(r => r.RestartService(TimeSpan.FromSeconds(10)));
                x.StartAutomatically();
            }).Run();
        }
    }
}
