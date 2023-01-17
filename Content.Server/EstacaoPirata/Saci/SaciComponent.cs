using Content.Shared.Actions.ActionTypes;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;
using Robust.Shared.Utility;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.Set;

namespace Content.Server.EstacaoPirata.Saci;

[RegisterComponent]
    public sealed class SaciPowersComponent : Component
    {
        /// <summary>
        /// Whether this component is active or not.
        /// </summary>
        [ViewVariables]
        [DataField("enabled")]
        public bool Enabled = true;

        [ViewVariables]
        public readonly List<ActionType> Actions = new();

        [DataField("worldTargetActions", customTypeSerializer: typeof(PrototypeIdHashSetSerializer<WorldTargetActionPrototype>))] public HashSet<string> WorldTargetActions  = new();
        [DataField("entityTargetActions", customTypeSerializer: typeof(PrototypeIdHashSetSerializer<EntityTargetActionPrototype>))] public HashSet<string> EntityTargetActions  = new();
        [DataField("selfTargetActions", customTypeSerializer:typeof(PrototypeIdHashSetSerializer<InstantActionPrototype>))] public HashSet<string> SelfTargetActions  = new();

    /// <summary>
    /// The status effects applied after the ability
    /// the first float corresponds to amount of time the entity is stunned.
    /// the second corresponds to the amount of time the entity is made solid.
    /// </summary>
    [DataField("defileDebuffs")]
    public Vector2 DefileDebuffs = (1, 4);

    /// <summary>
    /// The radius around the user that this ability affects
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField("defileRadius")]
    public float DefileRadius = 3.5f;

    /// <summary>
    /// The amount of tiles that are uprooted by the ability
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField("defileTilePryAmount")]
    public int DefileTilePryAmount = 15;

    /// <summary>
    /// The chance that an individual entity will have any of the effects
    /// happen to it.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField("defileEffectChance")]
    public float DefileEffectChance = 0.5f;
    }

