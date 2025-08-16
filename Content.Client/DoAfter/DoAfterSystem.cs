using System.Diagnostics.CodeAnalysis;
using Content.Shared.CCVar; // Stellar - Revamped DoAfters
using Content.Shared.DoAfter;
using Content.Shared.Hands.Components;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Client.UserInterface; // Stellar - Revamped DoAfters
using Robust.Shared.Configuration; // Stellar - Revamped DoAfters
using Robust.Shared.Prototypes;

namespace Content.Client.DoAfter;

/// <summary>
/// Handles events that need to happen after a certain amount of time where the event could be cancelled by factors
/// such as moving.
/// </summary>
public sealed class DoAfterSystem : SharedDoAfterSystem
{
    [Dependency] private readonly IConfigurationManager _configManager = default!; // Stellar - Revamped DoAfters
    [Dependency] private readonly IUserInterfaceManager _userInterface = default!; // Stellar - Revamped DoAfters
    [Dependency] private readonly IOverlayManager _overlay = default!;
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly MetaDataSystem _metadata = default!;

    private readonly string _barTexturePath = "/progressbar"; // Stellar - Revamped DoAfters
    private Texture? _barTexture; // Stellar - Revamped DoAfters

    public override void Initialize()
    {
        base.Initialize();
        _barTexture = _userInterface.CurrentTheme.ResolveTextureOrNull($"{_userInterface.CurrentTheme.Path}{_barTexturePath}")?.Texture; // Begin Stellar - Revamped DoAfters
        _overlay.AddOverlay(new DoAfterOverlay(_barTexture!, EntityManager, _prototype, GameTiming, _player));
        _configManager.OnValueChanged(CCVars.InterfaceTheme, ReloadOverlay, true); // End Stellar - Revamped DoAfters
    }

    public override void Shutdown()
    {
        base.Shutdown();
        _overlay.RemoveOverlay<DoAfterOverlay>();
    }

    private void ReloadOverlay(string hudTheme) // Begin Stellar - Revamped DoAfters
    {
        _barTexture = _userInterface.CurrentTheme.ResolveTextureOrNull($"{_userInterface.CurrentTheme.Path}{_barTexturePath}")?.Texture;
        _overlay.RemoveOverlay<DoAfterOverlay>();
        _overlay.AddOverlay(new DoAfterOverlay(_barTexture!, EntityManager, _prototype, GameTiming, _player));
    } // End Stellar - Revamped DoAfters

#pragma warning disable RA0028 // No base call in overriden function
    public override void Update(float frameTime)
#pragma warning restore RA0028 // No base call in overriden function
    {
        // Currently this only predicts do afters initiated by the player.

        // TODO maybe predict do-afters if the local player is the target of some other players do-after? Specifically
        // ones that depend on the target not moving, because the cancellation of those do afters should be readily
        // predictable by clients.

        var playerEntity = _player.LocalEntity;

        if (!TryComp(playerEntity, out ActiveDoAfterComponent? active))
            return;

        if (_metadata.EntityPaused(playerEntity.Value))
            return;

        var time = GameTiming.CurTime;
        var comp = Comp<DoAfterComponent>(playerEntity.Value);
        var xformQuery = GetEntityQuery<TransformComponent>();
        var handsQuery = GetEntityQuery<HandsComponent>();
        Update(playerEntity.Value, active, comp, time, xformQuery, handsQuery);
    }

    /// <summary>
    /// Try to find an active do-after being executed by the local player.
    /// </summary>
    /// <param name="entity">The entity the do after must be targeting (<see cref="DoAfterArgs.Target"/>)</param>
    /// <param name="doAfter">The found do-after.</param>
    /// <param name="event">The event to be raised on the found do-after when it completes.</param>
    /// <param name="progress">The progress of the found do-after, from 0 to 1.</param>
    /// <typeparam name="T">The type of event that must be raised by the found do-after.</typeparam>
    /// <returns>True if a do-after was found.</returns>
    public bool TryFindActiveDoAfter<T>(
        EntityUid entity,
        [NotNullWhen(true)] out Shared.DoAfter.DoAfter? doAfter,
        [NotNullWhen(true)] out T? @event,
        out float progress)
        where T : DoAfterEvent
    {
        var playerEntity = _player.LocalEntity;

        doAfter = null;
        @event = null;
        progress = default;

        if (!TryComp(playerEntity, out ActiveDoAfterComponent? active))
            return false;

        if (_metadata.EntityPaused(playerEntity.Value))
            return false;

        var comp = Comp<DoAfterComponent>(playerEntity.Value);

        var time = GameTiming.CurTime;

        foreach (var candidate in comp.DoAfters.Values)
        {
            if (candidate.Cancelled)
                continue;

            if (candidate.Args.Target != entity)
                continue;

            if (candidate.Args.Event is not T candidateEvent)
                continue;

            @event = candidateEvent;
            doAfter = candidate;
            var elapsed = time - doAfter.StartTime;
            progress = (float) Math.Min(1, elapsed.TotalSeconds / doAfter.Args.Delay.TotalSeconds);

            return true;
        }

        return false;
    }
}
