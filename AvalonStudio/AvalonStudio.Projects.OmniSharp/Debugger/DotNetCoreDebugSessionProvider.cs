﻿namespace AvalonStudio.Debugging.DotNetCore
{
    using AvalonStudio.CommandLineTools;
    using AvalonStudio.Extensibility;
    using AvalonStudio.GlobalSettings;
    using AvalonStudio.Platforms;
    using AvalonStudio.Projects;
    using AvalonStudio.Projects.OmniSharp;
    using Mono.Debugging.Client;
    using Mono.Debugging.Win32;
    using System.IO;
    using AvalonStudio.Utils;
    using System;
    using System.Threading.Tasks;
    using AvalonStudio.Projects.OmniSharp.DotnetCli;

    public class DotNetCoreDebugger : IDebugger2
    {
        public string BinDirectory => Path.GetDirectoryName(Settings.GetSettings<DotNetToolchainSettings>().DotNetPath);

        public void Activation()
        {
        }

        public void BeforeActivation()
        {
            IoC.RegisterConstant<DotNetCoreDebugger>(this);
        }

        public DebuggerSession CreateSession(IProject project)
        {            
            string dbgShimName = "dbgshim";

            if (Platform.PlatformIdentifier != Platforms.PlatformID.Win32NT)
            {
                dbgShimName = "lib" + dbgShimName;
            }

            var dbgShimPath = Path.Combine(DotNetCliService.Instance.Info.RootPath, "shared", "Microsoft.NETCore.App", DotNetCliService.Instance.Info.Version.ToString(), dbgShimName + Platform.DLLExtension);

            var result = new CoreClrDebuggerSession(System.IO.Path.GetInvalidPathChars(), dbgShimPath)
            {
                CustomSymbolReaderFactory = new PdbSymbolReaderFactory()
            };

            return result;
        }

        public DebuggerSessionOptions GetDebuggerSessionOptions(IProject project)
        {
            var evaluationOptions = EvaluationOptions.DefaultOptions.Clone();

            evaluationOptions.EllipsizeStrings = false;
            evaluationOptions.GroupPrivateMembers = false;
            evaluationOptions.EvaluationTimeout = 1000;

            return new DebuggerSessionOptions() { EvaluationOptions = evaluationOptions };
        }

        public DebuggerStartInfo GetDebuggerStartInfo(IProject project)
        {
            var startInfo = new DebuggerStartInfo()
            {
                Command = "dotnet" + Platforms.Platform.ExecutableExtension,
                Arguments = project.Executable,
                WorkingDirectory = System.IO.Path.GetDirectoryName(project.Executable),
                UseExternalConsole = false,
                CloseExternalConsoleOnExit = true
            };

            return startInfo;
        }

        public object GetSettingsControl(IProject project)
        {
            return null;
        }

        public Task InstallAsync(IConsole console)
        {
            return Task.FromResult(0);
        }
    }
}