using System.Collections.Generic;
using System.Threading.Tasks;
using System;

using Microsoft.Playwright;

namespace JADE.Backend;

public partial class BackendNavigation
{
    public async Task LogIn()
    {

        string? userName = config["Backend:Username"];
        string? userPass = config["Backend:Password"];
        string? backend = config["backend"];

        if (backend is null || userName is null || userPass is null)
            throw new KeyNotFoundException("Missing creditentials!");
        await page.GotoAsync(backend);
        if (page.Url.StartsWith(backend))
            return;
        Console.WriteLine("Loging into backend");
        await page.GetByRole(AriaRole.Textbox, new() { Name = "Identyfikator:" }).ClickAsync();
        await page.GetByRole(AriaRole.Textbox, new() { Name = "Identyfikator:" }).FillAsync(userName);
        await page.GetByRole(AriaRole.Textbox, new() { Name = "Hasło:" }).ClickAsync();
        await page.GetByRole(AriaRole.Textbox, new() { Name = "Hasło:" }).FillAsync(userPass);
        await page.GetByText("Zapamiętaj mnie").ClickAsync();
        await page.GetByRole(AriaRole.Button, new() { Name = "Zaloguj" }).ClickAsync();
        await page.GotoAsync(backend);
        await page.WaitForLoadStateAsync();
        if (page.Url.StartsWith(backend))
        {
            await page.GotoAsync(backend);
            await page.WaitForLoadStateAsync();
            if (page.Url.StartsWith(backend))
                throw new Exception("Login failed!");
        }
        Console.WriteLine("Login succesfull");
    }
}