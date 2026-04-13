using System.Threading.Tasks;
using System;

using Microsoft.Playwright;

namespace PlaywrightSharp.Backend;

public partial class BackendNavigation
{

    public async Task LogIn()
    {
        Console.WriteLine("Loging into backend");
        string userName = config["Backend:Username"];
        string userPass = config["Backend:Password"];

        await page.GotoAsync(config["backend"]);
        if (page.Url.StartsWith(config["backend"]))
            return;
        await page.GetByRole(AriaRole.Textbox, new() { Name = "Identyfikator:" }).ClickAsync();
        await page.GetByRole(AriaRole.Textbox, new() { Name = "Identyfikator:" }).FillAsync(userName);
        await page.GetByRole(AriaRole.Textbox, new() { Name = "Hasło:" }).ClickAsync();
        await page.GetByRole(AriaRole.Textbox, new() { Name = "Hasło:" }).FillAsync(userPass);
        await page.GetByText("Zapamiętaj mnie").ClickAsync();
        await page.GetByRole(AriaRole.Button, new() { Name = "Zaloguj" }).ClickAsync();
        await page.GotoAsync(config["backend"]);
        await page.WaitForLoadStateAsync();
        if (page.Url.StartsWith(config["backend"]))
            throw new Exception("Loging in failed!");
        Console.WriteLine("Login succesfull");
    }
}