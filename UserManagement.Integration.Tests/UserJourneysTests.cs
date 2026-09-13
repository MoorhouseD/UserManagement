using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using UserManagement.Data;

namespace UserManagement.Integration.Tests;

[Collection(PostgreSqlCollection.Name)]
public sealed class UserJourneysTests(PostgreSqlWebApplicationFactory factory)
{
    [Fact]
    public async Task ApplicationStartup_AppliesMigrationsAndSeedsUsers()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var scope = factory.Services.CreateScope();
        var dataContext = scope.ServiceProvider.GetRequiredService<DataContext>();

        (await dataContext.Database.CanConnectAsync(cancellationToken)).Should().BeTrue();
        (await dataContext.Database.GetAppliedMigrationsAsync(cancellationToken)).Should().ContainSingle();
        (await dataContext.Users.CountAsync(cancellationToken)).Should().Be(11);
    }

    [Fact]
    public async Task List_WithActiveFilter_ReturnsOnlyActiveUsers()
    {
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var cancellationToken = TestContext.Current.CancellationToken;
        using var response = await client.GetAsync("/users/list?isActive=true", cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        content.Should().Contain("Peter");
        content.Should().NotContain("Castor");
    }

    [Fact]
    public async Task Details_WhenUserExists_CreatesViewedAuditEvent()
    {
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var cancellationToken = TestContext.Current.CancellationToken;
        using var response = await client.GetAsync("/users/1", cancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using var scope = factory.Services.CreateScope();
        var dataContext = scope.ServiceProvider.GetRequiredService<DataContext>();
        var viewedEvents = await dataContext.UserActionLogs
            .Where(log => log.UserId == 1 && log.Action == "Viewed")
            .CountAsync(cancellationToken);

        viewedEvents.Should().BeGreaterThanOrEqualTo(1);
    }

    [Fact]
    public async Task CreateEditAndDeleteJourney_PersistsChangesAndAuditEvents()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var email = $"integration-{Guid.NewGuid():N}@example.com";
        var createToken = await GetAntiforgeryTokenAsync(client, "/users/create", cancellationToken);
        using var createResponse = await client.PostAsync(
            "/users/create",
            FormData(createToken, ("Forename", "Integration"), ("Surname", "User"), ("DateOfBirth", "1990-01-01"), ("Email", email), ("IsActive", "true")),
            cancellationToken);

        createResponse.StatusCode.Should().Be(HttpStatusCode.Redirect);

        using var scope = factory.Services.CreateScope();
        var dataContext = scope.ServiceProvider.GetRequiredService<DataContext>();
        var user = await dataContext.Users.SingleAsync(user => user.Email == email, cancellationToken);

        var editToken = await GetAntiforgeryTokenAsync(client, $"/users/edit/{user.Id}", cancellationToken);
        using var editResponse = await client.PostAsync(
            $"/users/edit/{user.Id}",
            FormData(editToken, ("Forename", "Updated"), ("Surname", "User"), ("DateOfBirth", "1990-01-01"), ("Email", email), ("IsActive", "false")),
            cancellationToken);

        editResponse.StatusCode.Should().Be(HttpStatusCode.Redirect);

        using var deleteResponse = await client.PostAsync(
            $"/users/delete/{user.Id}",
            FormData(editToken),
            cancellationToken);

        deleteResponse.StatusCode.Should().Be(HttpStatusCode.Redirect);
        (await dataContext.Users.AnyAsync(item => item.Id == user.Id, cancellationToken)).Should().BeFalse();
        (await dataContext.UserActionLogs.CountAsync(item => item.UserId == user.Id, cancellationToken)).Should().Be(3);
    }

    private static async Task<string> GetAntiforgeryTokenAsync(HttpClient client, string path, CancellationToken cancellationToken)
    {
        using var response = await client.GetAsync(path, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        var token = Regex.Match(content, "name=\\\"__RequestVerificationToken\\\" type=\\\"hidden\\\" value=\\\"([^\\\"]+)\\\"");
        token.Success.Should().BeTrue();
        return token.Groups[1].Value;
    }

    private static FormUrlEncodedContent FormData(string token, params (string Name, string Value)[] fields)
    {
        var values = fields.Select(field => new KeyValuePair<string, string>(field.Name, field.Value)).ToList();
        values.Add(new("__RequestVerificationToken", token));
        return new FormUrlEncodedContent(values);
    }
}
