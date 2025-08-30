// SPDX-FileCopyrightText: 2025 AftrLite
//
// SPDX-License-Identifier: MIT

using Content.Shared.Buckle.Components;
using Content.Shared.Gravity;
using Content.Shared.Standing;
using Robust.Shared.Map;

namespace Content.Stellar.Shared.Dropshadow;

public abstract class SharedDropshadowSystem : EntitySystem
{
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly SharedGravitySystem _gravity = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DropshadowComponent, MapInitEvent>(OnInit);

        SubscribeLocalEvent<DropshadowComponent, BuckledEvent>(OnBuckled);
        SubscribeLocalEvent<DropshadowComponent, UnbuckledEvent>(OnUnbuckled);
        SubscribeLocalEvent<DropshadowComponent, DownedEvent>(OnDowned);
        SubscribeLocalEvent<DropshadowComponent, StoodEvent>(OnStood);
        SubscribeLocalEvent<DropshadowComponent, AnchorStateChangedEvent>(OnAnchorChanged);

        SubscribeLocalEvent<DropshadowComponent, EntParentChangedMessage>(OnParentChanged);
        SubscribeLocalEvent<GravityChangedEvent>(OnGravityChanged);
    }

    private void OnInit(Entity<DropshadowComponent> ent, ref MapInitEvent args)
    {
        if (ent.Comp.AnchorShadow)
            _appearance.SetData(ent, DropshadowVisuals.Visible, Transform(ent).Anchored);
    }

    protected bool IsFloating(Entity<DropshadowComponent> ent, TransformComponent? transform = null)
    {
        if (!Resolve(ent, ref transform))
            return false;

        if (transform.MapID == MapId.Nullspace)
            return false;

        Dirty(ent);
        return !_gravity.IsWeightless(ent, xform: transform);
    }

    private void OnBuckled(Entity<DropshadowComponent> ent, ref BuckledEvent args)
    {
        _appearance.SetData(ent, DropshadowVisuals.Visible, false);
    }

    private void OnUnbuckled(Entity<DropshadowComponent> ent, ref UnbuckledEvent args)
    {
        _appearance.SetData(ent, DropshadowVisuals.Visible, true);
    }

    private void OnDowned(Entity<DropshadowComponent> ent, ref DownedEvent args)
    {
        _appearance.SetData(ent, DropshadowVisuals.Prone, true);
    }

    private void OnStood(Entity<DropshadowComponent> ent, ref StoodEvent args)
    {
        _appearance.SetData(ent, DropshadowVisuals.Prone, false);
    }

    private void OnAnchorChanged(Entity<DropshadowComponent> ent, ref AnchorStateChangedEvent args)
    {
        if (ent.Comp.AnchorShadow)
            _appearance.SetData(ent, DropshadowVisuals.Visible, args.Anchored);
    }

    private void OnParentChanged(Entity<DropshadowComponent> ent, ref EntParentChangedMessage args)
    {
        _appearance.SetData(ent, DropshadowVisuals.Visible, IsFloating(ent, args.Transform));

    }

    private void OnGravityChanged(ref GravityChangedEvent args)
    {
        var query = EntityQueryEnumerator<DropshadowComponent, TransformComponent>();
        while (query.MoveNext(out var ent, out var shadowComp, out var transform))
        {
            if (transform.GridUid != args.ChangedGridIndex)
                continue;

            _appearance.SetData(ent, DropshadowVisuals.Visible, args.HasGravity);

            if (shadowComp.AnchorShadow)
                _appearance.SetData(ent, DropshadowVisuals.Visible, Transform(ent).Anchored);
        }
    }
}
