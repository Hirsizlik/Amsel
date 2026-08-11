using Amsel.Data;
using System.Diagnostics;
using System.Runtime.InteropServices;
using HackF5.UnitySpy;
using HackF5.UnitySpy.Detail;
using HackF5.UnitySpy.Offsets;
using HackF5.UnitySpy.ProcessFacade;

namespace Amsel.ArenaConnect;

public class MtgArenaConnect : IMtgArenaConnect
{
    private readonly Process mtgaProcess;
    private readonly IAssemblyImage assemblyImage;
    private readonly string? gameExecutableFilePath; // always null on Windows, only needed for Wine/Proton
    public MtgArenaConnect()
    {
        mtgaProcess = GetProcess();
        (assemblyImage, gameExecutableFilePath) = CreateAssemblyImage(mtgaProcess);
    }

    private static NotSupportedException PlatformNotSupported()
    {
        return new NotSupportedException("Platform not supported");
    }

    private static Process GetProcess()
    {
        Process[] processes = Process.GetProcesses();
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return processes.First(p => p.ProcessName == "MTGA");
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            foreach (Process process in processes.Where(p => p.ProcessName == "MTGA.exe"))
            {
                string maps = File.ReadAllText($"/proc/{process.Id}/maps");
                if (!string.IsNullOrWhiteSpace(maps))
                {
                    return process;
                }
            }
            throw new Exception("Process not found");
        }
        throw PlatformNotSupported();
    }

    private static (IAssemblyImage, string?) CreateAssemblyImage(Process mtgaProcess)
    {
        ProcessFacade processFacade;
        MonoLibraryOffsets monoLibraryOffsets;
        string? gameExecutableFilePath = null;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            string memPseudoFilePath = $"/proc/{mtgaProcess.Id}/mem";
            ProcessFacadeLinuxDirect processFacadeLinux = new(mtgaProcess.Id, memPseudoFilePath);
            gameExecutableFilePath = processFacadeLinux.GetModulePath(mtgaProcess.ProcessName);
            processFacade = processFacadeLinux;
            monoLibraryOffsets = MonoLibraryOffsets.GetOffsets(gameExecutableFilePath);
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            ProcessFacadeWindows processFacadeWindows = new(mtgaProcess);
            monoLibraryOffsets = MonoLibraryOffsets.GetOffsets(processFacadeWindows.GetMainModuleFileName());
            processFacade = processFacadeWindows;
        }
        else
        {
            throw PlatformNotSupported();
        }

        var f = new UnityProcessFacade(processFacade, monoLibraryOffsets);
        return (AssemblyImageFactory.Create(f, "Core"), gameExecutableFilePath);
    }

    public Dictionary<uint, CardOwned> GetCardsOwnedFromInventory()
    {
        Dictionary<uint, CardOwned> cards = [];
        object[] cardEntries = assemblyImage["WrapperController"]
            ["<Instance>k__BackingField"]
            ["<InventoryManager>k__BackingField"]
            ["InventoryServiceWrapper"]
            ["<Cards>k__BackingField"]
            ["_entries"];
        for (int i = 0; i < cardEntries.Length; i++)
        {
            if (cardEntries[i] is ManagedStructInstance cardInstance)
            {
                int owned = cardInstance.GetValue<int>("value");
                if (owned > 0)
                {
                    uint groupId = cardInstance.GetValue<uint>("key");
                    cards.Add(groupId, new CardOwned(groupId, owned));
                }
            }
        }
        return cards;
    }

    public string GetDatabasePath()
    {
        string connectionString = assemblyImage["WrapperController"]
            ["<Instance>k__BackingField"]
            ["<CardDatabase>k__BackingField"]
            ["<CardDataProvider>k__BackingField"]
            ["_baseCardDataProvider"]
            ["_dbConnection"]
            ["_connectionString"];
        string[] connectionParts = connectionString["Data Source=".Length..].Split(';');

        string dbPath = connectionParts[0];
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return dbPath;
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            // Wine / Proton path must be mapped
            string dbRelPath = dbPath[dbPath.IndexOf("MTGA/")..].Replace('\\', '/');
            string mtgaPrefix = gameExecutableFilePath?[..gameExecutableFilePath.IndexOf("MTGA/")]
                ?? throw new Exception("Executable has no file path");
            return mtgaPrefix + dbRelPath;
        }
        else
        {
            throw PlatformNotSupported();
        }
    }

}
