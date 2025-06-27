using System;
using LiveFeedback.Models;
using LiveFeedback.Services;
using ReactiveUI;

namespace LiveFeedback.ViewModels;

public class PositionSelectorViewModel : ReactiveObject
{
    private readonly AppState _appState;
    private readonly LocalConfig _localConfig;

    private string _blBtnColor;

    private string _brBtnColor;

    private string _tlBtnColor;

    private string _trBtnColor;

    public PositionSelectorViewModel(AppState appState, LocalConfig localConfig)
    {
        _appState = appState;
        _localConfig = localConfig;
        _tlBtnColor = BtnColor("tl");
        _trBtnColor = BtnColor("tr");
        _brBtnColor = BtnColor("br");
        _blBtnColor = BtnColor("bl");
    }

    public string TrBtnColor
    {
        get => _trBtnColor;
        set => this.RaiseAndSetIfChanged(ref _trBtnColor, value);
    }

    public string TlBtnColor
    {
        get => _tlBtnColor;
        set => this.RaiseAndSetIfChanged(ref _tlBtnColor, value);
    }

    public string BlBtnColor
    {
        get => _blBtnColor;
        set => this.RaiseAndSetIfChanged(ref _blBtnColor, value);
    }

    public string BrBtnColor
    {
        get => _brBtnColor;
        set => this.RaiseAndSetIfChanged(ref _brBtnColor, value);
    }

    public void SelectCornerCommand(string corner)
    {
        DeactivateAnyButton();
        switch (corner)
        {
            case "tl":
                _appState.OverlayPosition = OverlayPosition.TopLeft;
                TlBtnColor = "DodgerBlue";
                break;
            case "tr":
                _appState.OverlayPosition = OverlayPosition.TopRight;
                TrBtnColor = "DodgerBlue";
                break;
            case "bl":
                _appState.OverlayPosition = OverlayPosition.BottomLeft;
                BlBtnColor = "DodgerBlue";
                break;
            case "br":
                _appState.OverlayPosition = OverlayPosition.BottomRight;
                BrBtnColor = "DodgerBlue";
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        _localConfig.OverlayPosition = _appState.OverlayPosition;
        _localConfig.SaveChanges();
    }

    private void DeactivateAnyButton()
    {
        TlBtnColor = "Transparent";
        TrBtnColor = "Transparent";
        BlBtnColor = "Transparent";
        BrBtnColor = "Transparent";
    }

    private string BtnColor(string btn)
    {
        return _appState.OverlayPosition switch
        {
            OverlayPosition.TopLeft => btn == "tl" ? "DodgerBlue" : "Transparent",
            OverlayPosition.TopRight => btn == "tr" ? "DodgerBlue" : "Transparent",
            OverlayPosition.BottomRight => btn == "br" ? "DodgerBlue" : "Transparent",
            OverlayPosition.BottomLeft => btn == "bl" ? "DodgerBlue" : "Transparent",
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}