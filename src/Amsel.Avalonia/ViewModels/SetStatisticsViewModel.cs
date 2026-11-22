using Amsel.ArenaConnect;
using Amsel.Avalonia.Models;
using Amsel.Avalonia.Views.UserControls;
using Amsel.Data;
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
        options ??= new OptionsChangedEventArgs(
            null!,
            new Filter(false, false, false, true),
            Sort.ReleaseDate,
            Direction.Descending);

        var result = loader.SetInformation.Select(s => s.Value);

        IComparable? GetValue(SetInformation s) => options.Sort switch
        {
            Sort.Name => s.Name,
            Sort.SetCode => s.Code,
            Sort.ReleaseDate => s.Metadata?.ReleaseDate,
            Sort.Completion1x => s.Statistic.PercentOwnedUnique,
            Sort.Completion4x => s.Statistic.PercentOwned,
            _ => throw new InvalidOperationException("Invalid Sort value")
        };

        result = result.Where(s =>
        {
            if (!options.Filter.WithoutMetadata && s.Metadata == null)
                return false;

            if (!options.Filter.WithoutName && s.Metadata == null && s.Name == null)
                return false;

            if (!options.Filter.WithoutProgress && s.Statistic.TotalOwned == 0)
                return false;

            if (!options.Filter.WithSubsets && s.Metadata?.IsMajorCardSet != true)
                return false;
            return true;
        });

        result = options.Direction == Direction.Ascending
            ? result.OrderBy(GetValue)
            : result.OrderByDescending(GetValue);

        Sets = result
            .Select(s => new DecoratedSetInformation(s, arenaAssets, loader.FormatInfo))
            .ToArray();
    }
}
