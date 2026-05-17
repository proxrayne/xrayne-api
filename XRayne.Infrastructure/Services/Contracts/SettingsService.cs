using System.Globalization;
using Microsoft.Extensions.Configuration;
using XRayne.Contracts.Configurations;
using XRayne.Contracts.Enums;
using XRayne.Contracts.Models;
using XRayne.Contracts.Utilities;
using XRayne.Repositories.Utilities;

namespace XRayne.Infrastructure.Services;

public sealed class SettingsService(IConfiguration configuration) : ISettingsService
{
    private readonly SemaphoreSlim _writeLock = new(1, 1);
    private bool _pendingRestart = false;
    private PanelSettings _current = PanelSettings.Parse(configuration);

    public PanelSettings Current => _current;
    public bool PendingRestart => _pendingRestart;

    public async Task<SettingsUpdateState> ApplyAsync(PanelSettings next, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(next);

        await _writeLock.WaitAsync(ct);
        try
        {
            var commits = BuildCommits(_current, next);
            if (commits.Count == 0)
            {
                return new SettingsUpdateState();
            }

            await ApplyCommitsAsync(commits, ct);

            var changedFields = commits
                        .Select(x => x.Field)
                        .Distinct(StringComparer.Ordinal)
                        .ToArray();

            var requiredRestart = commits.Any(x => x.Impact == RestartImpact.FullRestart);

            _current = next.Clone();
            _pendingRestart = _pendingRestart || requiredRestart;

            return new SettingsUpdateState
            {
                ChangedFields = changedFields,
                RequiresRestart = requiredRestart,
                PendingRestart = _pendingRestart
            }; ;
        }
        finally
        {
            _writeLock.Release();
        }
    }

    private IReadOnlyList<UpdateCommit> BuildCommits(PanelSettings current, PanelSettings next)
    {
        var commits = new List<UpdateCommit>();

        AddIfChanged(commits, current.BindIp, next.BindIp, () => [
            new UpdateCommit() {
                ConfigKey = "BindIp",
                Field = nameof(next.BindIp),
                Value = next.BindIp,
            },
        ]);

        AddIfChanged(commits, current.PathBase, next.PathBase, () => [
            new UpdateCommit() {
                ConfigKey = "PathBase",
                Field = nameof(next.PathBase),
                Value = next.PathBase,
            },
        ]);

        AddIfChanged(commits, current.Domain, next.Domain, () => [
            new UpdateCommit() {
                ConfigKey = "Domain",
                Field = nameof(next.Domain),
                Value = next.Domain,
            },
        ]);

        AddIfChanged(commits, current.Port, next.Port, () => [
            new UpdateCommit() {
                ConfigKey = "API_PORT",
                Field = nameof(next.Port),
                Value = next.Port,
                Target = UpdateTarget.Env,
                Impact = RestartImpact.FullRestart,
            },
            new UpdateCommit() {
                Field = nameof(next.Port),
                ConfigKey = "Kestrel:Endpoints:Http:Url",
                Value = $"http://+:{next.Port}",
                Impact = RestartImpact.FullRestart,
            },
        ]);

        AddIfChanged(commits, current.SessionLifetimeMinutes, next.SessionLifetimeMinutes, () => [
            new UpdateCommit() {
                Field = nameof(next.SessionLifetimeMinutes),
                ConfigKey = "Jwt:AccessTokenLifetimeMinutes",
                Value = next.Port,
                Impact = RestartImpact.FullRestart,
            },
        ]);

        AddIfChanged(commits, current.CertPrivateKeyPath, next.CertPrivateKeyPath, () => [
            new UpdateCommit() {
                Field = nameof(next.CertPrivateKeyPath),
                ConfigKey = "Cert:PrivateKeyPath",
                Value = next.CertPrivateKeyPath,
                Impact = RestartImpact.FullRestart,
            },
        ]);

        AddIfChanged(commits, current.CertPublicKeyPath, next.CertPublicKeyPath, () => [
            new UpdateCommit() {
                Field =nameof(next.CertPublicKeyPath),
                ConfigKey = "Cert:PublicKeyPath",
                Value = next.CertPublicKeyPath,
                Impact = RestartImpact.FullRestart,
            },
        ]);

        return commits;
    }

    private static void AddIfChanged<T>(List<UpdateCommit> commits, T currentValue, T nextValue, Func<IReadOnlyList<UpdateCommit>> map)
    {
        if (DynamicValueComparer.AreEqual(currentValue, nextValue))
        {
            return;
        }

        commits.AddRange(map());
    }

    private static async Task ApplyCommitsAsync(IReadOnlyCollection<UpdateCommit> commits, CancellationToken ct)
    {
        var jsonCommits = new List<UpdateCommit>();
        var envCommits = new List<UpdateCommit>();

        foreach (var commit in commits)
        {
            if (commit.Target == UpdateTarget.Json)
            {
                jsonCommits.Add(commit);
            }
            else
            {
                envCommits.Add(commit);
            }
        }

        var updateTasks = new List<Task>();

        if (jsonCommits.Count > 0)
        {
            updateTasks.Add(JsonConfig.UpdateAsync(config =>
            {
                foreach (var commit in jsonCommits)
                {
                    JsonConfig.Set(config, commit.ConfigKey, commit.Value);
                }
            }, ct));
        }

        if (envCommits.Count > 0)
        {
            updateTasks.Add(EnvConfig.UpdateAsync(env =>
            {
                foreach (var commit in envCommits)
                {
                    EnvConfig.Set(env, commit.ConfigKey, Convert.ToString(commit.Value, CultureInfo.InvariantCulture) ?? "");
                }
            }, ct));
        }

        await Task.WhenAll(updateTasks);
    }
}