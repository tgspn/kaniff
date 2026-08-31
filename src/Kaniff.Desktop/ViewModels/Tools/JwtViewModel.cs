using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Kaniff.Core.Tools;

namespace Kaniff.Desktop.ViewModels.Tools;

public partial class JwtViewModel : ToolPageViewModel
{
    private readonly JwtTool _tool = new();

    public JwtViewModel() : base("JWT", "Decode a JWT header and payload (no verification).") { }

    [ObservableProperty]
    public partial string Token { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Header { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Payload { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Summary { get; set; } = string.Empty;

    [RelayCommand]
    private void Decode()
    {
        try
        {
            var result = _tool.Decode(Token);
            Header = result.HeaderJson;
            Payload = result.PayloadJson;

            var lines = new List<string>();
            if (result.IssuedAt is { } iat) lines.Add($"Issued at : {iat:u}");
            if (result.NotBefore is { } nbf) lines.Add($"Not before: {nbf:u}");
            if (result.ExpiresAt is { } exp)
                lines.Add($"Expires at: {exp:u} ({(result.IsExpired ? "EXPIRED" : "valid")})");
            Summary = lines.Count > 0 ? string.Join(Environment.NewLine, lines) : "No standard time claims.";
        }
        catch (Exception ex)
        {
            Header = string.Empty;
            Payload = string.Empty;
            Summary = $"⚠ {ex.Message}";
        }
    }
}
