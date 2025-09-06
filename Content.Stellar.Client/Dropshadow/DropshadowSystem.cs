// SPDX-FileCopyrightText: 2025 AftrLite
//
// SPDX-License-Identifier: MIT

using System.Numerics;
using Content.Shared.Humanoid;
using Content.Stellar.Shared.Dropshadow;
using Robust.Client.GameObjects;

namespace Content.Stellar.Client.Dropshadow;

public sealed partial class DropshadowSystem : SharedDropshadowSystem
{
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly SpriteSystem _sprite = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DropshadowComponent, ComponentStartup>(OnDropshadowAdded);
        SubscribeLocalEvent<DropshadowComponent, ComponentShutdown>(OnDropshadowRemoved);

        SubscribeLocalEvent<DropshadowComponent, AppearanceChangeEvent>(OnAppearanceChanged);
    }

    private void OnDropshadowAdded(Entity<DropshadowComponent> ent, ref ComponentStartup args)
    {
        if (!TryComp<SpriteComponent>(ent, out var sprite))
            return;

        var layer = _sprite.AddLayer((ent, sprite), ent.Comp.Sprite, 0);
        _sprite.LayerMapSet((ent, sprite), DropshadowLayers.Shadow, layer);
        _sprite.LayerSetOffset((ent, sprite), DropshadowLayers.Shadow, ent.Comp.Offset);
        sprite.LayerSetShader(DropshadowLayers.Shadow, "unshaded");

        if (ent.Comp.AnchorShadow)
            _sprite.LayerSetVisible((ent, sprite), DropshadowLayers.Shadow, Transform(ent).Anchored);
    }

    private void OnDropshadowRemoved(Entity<DropshadowComponent> ent, ref ComponentShutdown args)
    {
        if (!TryComp<SpriteComponent>(ent, out var sprite))
            return;

        _sprite.RemoveLayer((ent, sprite), DropshadowLayers.Shadow, false);
    }

    private void OnAppearanceChanged(Entity<DropshadowComponent> ent, ref AppearanceChangeEvent args)
    {
        if (args.Sprite == null)
            return;

        _appearance.TryGetData<bool>(ent, DropshadowVisuals.Weightless, out var weightless, args.Component);
        _appearance.TryGetData<bool>(ent, DropshadowVisuals.Anchored, out var anchored, args.Component);
        _appearance.TryGetData<bool>(ent, DropshadowVisuals.Buckled, out var buckled, args.Component);

        var visible = !weightless && anchored && !buckled;

        _sprite.LayerSetVisible((ent, args.Sprite), DropshadowLayers.Shadow, visible);

        if (HasComp<HumanoidAppearanceComponent>(ent) && _appearance.TryGetData<bool>(ent, DropshadowVisuals.Prone, out var prone, args.Component))
        {
            if (!prone)
            {
                _sprite.LayerSetRsiState((ent, args.Sprite), DropshadowLayers.Shadow, "shadow-playable");
                _sprite.LayerSetOffset((ent, args.Sprite), DropshadowLayers.Shadow, ent.Comp.Offset);
            }
            else
            {
                _sprite.LayerSetRsiState((ent, args.Sprite), DropshadowLayers.Shadow, "shadow-playable-prone");
                _sprite.LayerSetOffset((ent, args.Sprite), DropshadowLayers.Shadow, Vector2.Zero);
            }
        }
    }
}
