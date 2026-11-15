using Amsel.ArenaConnect;
using Amsel.Avalonia.Models;
using Amsel.Avalonia.Views;
using Amsel.Avalonia.Views.UserControls;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Amsel.Avalonia.ViewModels;

public partial class SetStatisticViewModel : ViewModelBase
{
    private readonly ArenaLoader loader;
    private readonly MtgArenaAssetsFactory arenaAssetsFactory;

    private IMtgArenaAssets? arenaAssets;

    [ObservableProperty]
    public partial DecoratedSetInformation[]? Sets { get; set; }

    [ObservableProperty]
    public partial DecoratedSetInformation? SelectedSet { get; set; }

    public SetStatisticViewModel(ArenaLoader loader, MtgArenaAssetsFactory arenaAssetsFactory)
    {
        this.loader = loader;
        this.arenaAssetsFactory = arenaAssetsFactory;
    }

    public Task LoadAsync()
    {
        arenaAssets = arenaAssetsFactory.Create();
        ReloadSetsFiltered(null);

        return Task.CompletedTask;
    }

    public void ReloadSetsFiltered(OptionsChangedEventArgs? options)
    {
        ArgumentNullException.ThrowIfNull(arenaAssets);

        options ??= new OptionsChangedEventArgs(null!, new Filter(false, false, false, true),
            Sort.ReleaseDate,
            Direction.Descending);

        var result = loader.SetInformation
            .Select(s => new DecoratedSetInformation(s.Value, arenaAssets, loader.FormatInfo));

        Func<DecoratedSetInformation, IComparable?> getValue = s => options.Sort switch
        {
            Sort.Name => s.Set.Name,
            Sort.SetCode => s.Set.Code,
            Sort.ReleaseDate => s.Set.Metadata?.ReleaseDate,
            Sort.Completion1x => s.Set.Statistic.PercentOwnedUnique,
            Sort.Completion4x => s.Set.Statistic.PercentOwned,
            _ => throw new InvalidOperationException("Invalid Sort value")
        };

        result = result.Where(s =>
        {
            if (!options.Filter.WithoutMetadata && !s.HasMetadata)
                return false;

            if (!options.Filter.WithoutName && s.HasNoMetadataNoName)
                return false;

            if (!options.Filter.WithoutProgress && s.Set.Statistic.TotalOwned == 0)
                return false;

            if (!options.Filter.WithSubsets && s.IsSubSet)
                return false;
            return true;
        });

        result = options.Direction == Direction.Ascending
            ? result.OrderBy(getValue)
            : result.OrderByDescending(getValue);

        Sets = result.ToArray();
    }
}
