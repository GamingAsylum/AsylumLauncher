using AsylumLauncher.Models;
using Avalonia;
using Avalonia.ReactiveUI;
using System;
using System.Diagnostics.CodeAnalysis;

namespace AsylumLauncher
{
    internal sealed class Program
    {
        // To prevent trimming from removing types that are only accessed by reflection, we need to specify them explicitly.
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(MissionFileVersion))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(News))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(AsylumLauncherVersion))]
        [STAThread]
        public static void Main(string[] args) => BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .WithInterFont()
                .LogToTrace()
                .UseReactiveUI();
    }
}
