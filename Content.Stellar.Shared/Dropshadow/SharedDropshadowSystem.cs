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

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DropshadowComponent, MapInitEvent>(OnMapInit);

        SubscribeLocalEvent<DropshadowComponent, BuckledEvent>(OnBuckled);
        SubscribeLocalEvent<DropshadowComponent, UnbuckledEvent>(OnUnbuckled);
        SubscribeLocalEvent<DropshadowComponent, DownedEvent>(OnDowned);
        SubscribeLocalEvent<DropshadowComponent, StoodEvent>(OnStood);
        SubscribeLocalEvent<DropshadowComponent, AnchorStateChangedEvent>(OnAnchorChanged);
        SubscribeLocalEvent<DropshadowComponent, WeightlessnessChangedEvent>(OnWeightlessnessChanged);
    }

    private void OnMapInit(Entity<DropshadowComponent> ent, ref MapInitEvent args)
    {
        _appearance.SetData(ent, DropshadowVisuals.Anchored, !ent.Comp.AnchorShadow || Transform(ent).Anchored);
    }

    private void OnBuckled(Entity<DropshadowComponent> ent, ref BuckledEvent args)
    {
        _appearance.SetData(ent, DropshadowVisuals.Buckled, true);
    }

    private void OnUnbuckled(Entity<DropshadowComponent> ent, ref UnbuckledEvent args)
    {
        _appearance.SetData(ent, DropshadowVisuals.Buckled, false);
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
        _appearance.SetData(ent, DropshadowVisuals.Anchored, !ent.Comp.AnchorShadow || args.Anchored);
    }

    private void OnWeightlessnessChanged(Entity<DropshadowComponent> ent, ref WeightlessnessChangedEvent args)
    {
        _appearance.SetData(ent, DropshadowVisuals.Weightless, args.Weightless);
    }
}
