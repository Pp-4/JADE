using System.Threading.Tasks;
using System;

using Microsoft.Extensions.Logging;
using Microsoft.Playwright;

namespace JADE.Backend;

public partial class BackendNavigation
{
    public async Task LogIn()
    {
        await page.GotoAsync(config.BackendAddress);
        if (page.Url.StartsWith(config.BackendAddress))
            return;
        logger.LogInformation("Loging into backend");
        var userNameField = page.GetByRole(AriaRole.Textbox, new() { Name = "Login:" });
        await userNameField.ClickAsync();
        await userNameField.FillAsync(config.BackendUsername);
        await page.GetByRole(AriaRole.Button, new() { Name = "Dalej " }).ClickAsync();
        var userPassField = page.GetByRole(AriaRole.Textbox, new() { Name = "Hasło:" });
        await userPassField.ClickAsync();
        await userPassField.FillAsync(config.BackendPassword);
        await page.GetByText("Zapamiętaj mnie").ClickAsync();
        await page.GetByRole(AriaRole.Button, new() { Name = "Zaloguj" }).ClickAsync();
        bool passPl = await page.GetByRole(AriaRole.Button, new() { Name = "Kontynuuj" }).IsVisibleAsync();
        bool passEn = await page.GetByRole(AriaRole.Button, new() { Name = "Continue" }).IsVisibleAsync();
        if (passPl)
            await page.GetByRole(AriaRole.Button, new() { Name = "Kontynuuj" }).ClickAsync();
        if (passEn)
            await page.GetByRole(AriaRole.Button, new() { Name = "Continue" }).ClickAsync();
        await page.GotoAsync(config.BackendAddress);
        await page.WaitForLoadStateAsync();
        if (!page.Url.StartsWith(config.BackendAddress))
        {
            await page.GotoAsync(config.BackendAddress);
            await page.WaitForLoadStateAsync();
            if (!page.Url.StartsWith(config.BackendAddress))
                throw new Exception("Login failed!");
        }
        logger.LogInformation("Login succesfull");
    }
}