// SPDX-FileCopyrightText: 2025 AftrLite
//
// SPDX-License-Identifier: MIT

using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Stellar.Shared.Dropshadow;

/// <summary>
/// Component applying a sprited drop shadow to entities. Specify the sprite and state in YML.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class DropshadowComponent : Component
{
    /// <summary>
    /// The sprite to use for the drop shadow
    /// </summary>
    [DataField(required: true)]
    public SpriteSpecifier Sprite = default!;

    /// <summary>
    /// The distance you need the dropshadow to be offset from the entity it's applied to. Default is offset for playable species base.
    /// </summary>
    [DataField]
    public Vector2 Offset = new Vector2(0, -0.062f);

    /// <summary>
    /// Wether or not to give this entity a drop shadow when it's anchored.
    /// </summary>
    [DataField]
    public bool AnchorShadow = false;
}

[Serializable, NetSerializable]
public enum DropshadowVisuals
{
    Visible,
    Prone
}

[Serializable, NetSerializable]
public enum DropshadowLayers
{
    Shadow
}
