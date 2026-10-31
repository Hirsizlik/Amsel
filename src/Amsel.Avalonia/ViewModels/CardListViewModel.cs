using Amsel.ArenaConnect;
using Amsel.Avalonia.Models;
using Amsel.Data;
using Avalonia.Markup.Xaml.MarkupExtensions;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Amsel.Avalonia.ViewModels;

public partial class CardListViewModel : ViewModelBase
{
    private readonly ArenaLoader loader;
    private readonly MtgArenaAssetsFactory arenaAssetsFactory;

    private IMtgArenaAssets? arenaAssets;
    private TextSearch textSearch = TextSearch.Empty();

    [ObservableProperty]
    public partial DecoratedSetInformation SetWithSymbol { get; set; } = DecoratedSetInformation.Empty;

    [ObservableProperty]
    public partial string Search { get; set; } = "";

    [ObservableProperty]
    public partial DecoratedSetInformation[]? Sets { get; set; }

    [ObservableProperty]
    public partial DecoratedCardStats[]? Cards { get; set; }

    [ObservableProperty]
    public partial bool SearchError { get; set; } = false;

    public CardListViewModel(ArenaLoader loader, MtgArenaAssetsFactory arenaAssetsFactory)
    {
        this.loader = loader;
        this.arenaAssetsFactory = arenaAssetsFactory;
    }

    private DecoratedCardStats[] FilterCards()
    {
        ArgumentNullException.ThrowIfNull(arenaAssets);
        var cardsEnumarable = loader.Cards.AsEnumerable();

        if (SetWithSymbol != DecoratedSetInformation.Empty)
        {
            var code = SetWithSymbol.Set.Code;
            cardsEnumarable = cardsEnumarable
                .Where(c => c.Info.ExpansionCode == code || c.Info.DigitalReleaseSet == code);
        }

        cardsEnumarable = cardsEnumarable.Where(textSearch.FilterCard);

        return cardsEnumarable.Select(c => new DecoratedCardStats(
                c,
                loader.SetInformation.First(s => s.Key == c.Info.ExpansionCode).Value,
                loader.SetInformation.FirstOrDefault(s => s.Key == c.Info.DigitalReleaseSet).Value,
                arenaAssets,
                loader.OwnedPerTitle,
                loader.FormatInfo))
            .ToArray();
    }

    public Task LoadAsync()
    {
        arenaAssets = arenaAssetsFactory.Create();
        Sets = loader.SetInformation
            .Select(s => new DecoratedSetInformation(s.Value, arenaAssets))
            .OrderBy(s => s.Set.Code)
            .Prepend(DecoratedSetInformation.Empty)
            .ToArray();
        Cards = FilterCards();
        return Task.CompletedTask;
    }

    partial void OnSetWithSymbolChanged(DecoratedSetInformation value)
    {
        Cards = FilterCards();
    }

    public void OnSearch()
    {
        if (TextSearch.TryParse(Search, loader.FormatInfo, out var result))
        {
            SearchError = false;
            textSearch = result;
        } else
        {
            SearchError = true;
            textSearch = TextSearch.Empty();
        }
        Cards = FilterCards();
    }

    public void OnSearchClear()
    {
        Search = "";
        OnSearch();
    }
}
