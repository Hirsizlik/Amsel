using Amsel.Data;
using System.Diagnostics;
using System.Runtime.InteropServices;
using HackF5.UnitySpy;
using HackF5.UnitySpy.Detail;
using HackF5.UnitySpy.Offsets;
using HackF5.UnitySpy.ProcessFacade;
using System.Collections.Immutable;
using System.Collections.Frozen;

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
        foreach (ManagedStructInstance cardInstance in cardEntries.Cast<ManagedStructInstance>())
        {
            int owned = cardInstance.GetValue<int>("value");
            if (owned > 0)
            {
                uint groupId = cardInstance.GetValue<uint>("key");
                cards.Add(groupId, new CardOwned(groupId, owned));
            }
        }
        return cards;
    }

    public string GetCardDatabasePath()
    {
        string connectionString = assemblyImage["WrapperController"]
            ["<Instance>k__BackingField"]
            ["<CardDatabase>k__BackingField"]
            ["<CardDataProvider>k__BackingField"]
            ["_baseCardDataProvider"]
            ["_dbConnection"]
            ["_connectionString"];

        return ExtractDbPath(connectionString);
    }

    public string GetClientLocalizationDatabasePath()
    {
        string connectionString = assemblyImage["WrapperController"]
            ["<Instance>k__BackingField"]
            ["<SceneLoader>k__BackingField"]
            ["_locManager"]
            ["_nestedProviders"]
            ["_items"]
            [0]
            ["_connectionString"];

        return ExtractDbPath(connectionString);
    }

    private string ExtractDbPath(string connectionString)
    {
        string dbPath = connectionString["Data Source=".Length..].Split(';')[0];
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

    public Dictionary<string, SetMetadata> GetSetMetadata()
    {
        object[] collationsByMapping = assemblyImage["WrapperController"]
            ["<Instance>k__BackingField"]
            ["<SceneLoader>k__BackingField"]
            ["_setMetadataProvider"]
            ["_collationsByMapping"]
            ["_entries"];
        Dictionary<string, SetMetadata> result = [];
        foreach (ManagedStructInstance m in collationsByMapping.Cast<ManagedStructInstance>())
        {
            int collationId = m.GetValue<int>("key");
            if (collationId == 0)
                continue;
            ManagedClassInstance v = m.GetValue<ManagedClassInstance>("value");
            ManagedClassInstance set = v["Set"];
            string code = set["SetCode"];
            Availability availability = (Availability)set["Availability"];
            DateTime releaseDate = DateTime.FromBinary((long)set["ReleaseDate"]["_dateData"]);
            SetMetadata sm = new(collationId, code, releaseDate, true, availability, null, null);
            result[code] = sm;
            foreach (ManagedClassInstance? related in set["RelatedSets"]["_items"])
            {
                if (related == null)
                    continue;
                string subCode = related["SetCode"];
                string subName = related["SetName"];
                Availability subAvailability = (Availability)related["Availability"];
                result[subCode] = new SetMetadata(0, subCode, releaseDate, false, subAvailability, subName, code);
            }
        }
        return result;
    }

    private static IList<T> GatherValuesFromSlots<T>(dynamic o)
    {
        List<T> result = [];
        if (o is null || o["_slots"] is null)
            return result;
        foreach (ManagedStructInstance i in o["_slots"])
        {
            if (typeof(T) == typeof(uint) && i["value"] == 0)
                continue;
            result.Add((T)i["value"]);
        }
        return result;
    }

    public ImmutableArray<FormatData> GetFormatData()
    {
        object[] rawFormats = assemblyImage["WrapperController"]
            ["<Instance>k__BackingField"]
            ["FormatManager"]
            ["_formats"]
            ["_items"];
        List<FormatData> result = [];
        foreach (ManagedClassInstance rawFormat in rawFormats.Cast<ManagedClassInstance>())
        {
            if (rawFormat == null)
                continue;

            // skip non evergreen formats (there are a lot due to Midweek Magic and Draft formats)
            bool evergreen = rawFormat["IsEvergreen"];
            if (!evergreen)
                continue;
            string name = rawFormat["_formatName"];
            // skip Bo3 and Direct Game formats, should be mostly the same as Bo1
            if (name.StartsWith("Traditional") || name.StartsWith("DirectGame"))
                continue;
            List<string> legalSets = GatherValuesFromSlots<string>(rawFormat["_legalSets"]);
            List<uint> legalTitles = GatherValuesFromSlots<uint>(rawFormat["_legalTitleIds"]);
            List<uint> bannedTitles = GatherValuesFromSlots<uint>(rawFormat["_bannedTitleIds"]);
            List<uint> bannedAsCommander = GatherValuesFromSlots<uint>(rawFormat["_bannedAsCommanders"]);
            List<KeyValuePair<uint, Quota>> restrictedTitles = [];
            if (rawFormat["_restrictedTitleIds"] is not null && rawFormat["_restrictedTitleIds"]["_entries"] is not null)
            {
                foreach (ManagedStructInstance restrictedTitle in rawFormat["_restrictedTitleIds"]["_entries"])
                {
                    uint titleId = (uint)restrictedTitle["key"];
                    uint max = (uint)restrictedTitle["value"]["Max"];
                    restrictedTitles.Add(new(titleId, new Quota(max)));
                }
            }
            result.Add(new FormatData
            (
                NameKey: name,
                LegalSets: legalSets.ToFrozenSet(),
                BannedTitleIds: bannedTitles.ToFrozenSet(),
                RestrictedTitleIds: restrictedTitles.ToFrozenDictionary(),
                BannedAsCommanderTitles: bannedAsCommander.ToFrozenSet(),
                LegalTitleIds: legalTitles.ToFrozenSet()
            ));
        }
        return result.ToImmutableArray();
    }
}
