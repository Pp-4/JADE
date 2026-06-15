namespace JADE.models;

public record Config
{
    public string InputFile { get; init; } = "products.txt";
    public string SaveFile { get; init; } = "data.json";
    public string DataDir { get; init; } = "data/";
    public string ImgDir { get; init; } = "img/";
    public string BrowserDataDir { get; init; } = "playwright-user-data/";
    public string BackendAddress { get; init; } = "";
    public string BackendUsername { get; init; } = "";
    public string BackendPassword { get; init; } = "";
    public uint AddingImagesTimeout { get; init; } = 10000;
    public uint ImageLimit { get; init; } = 3;
    public string UserAgent { get; init; } = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/114.0.0.0 Safari/537.36";
    public string Language = "EN";
}