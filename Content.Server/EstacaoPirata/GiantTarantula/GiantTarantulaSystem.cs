using Content.Server.Actions;
using Content.Server.Atmos.EntitySystems;
using Content.Server.Nutrition.Components;
using Content.Server.Coordinates.Helpers;
using Content.Server.Popups;
using Content.Shared.Actions;
using Content.Shared.Atmos;
using Robust.Server.GameObjects;
using Robust.Shared.Player;
using Robust.Shared.Random;

namespace Content.Server.GiantTarantula
{
    public sealed class GiantTarantulaSystem : EntitySystem
    {
        [Dependency] private readonly IRobustRandom _random = default!;
        [Dependency] private readonly PopupSystem _popup = default!;
        [Dependency] private readonly ActionsSystem _action = default!;
        [Dependency] private readonly AtmosphereSystem _atmos = default!;
        [Dependency] private readonly TransformSystem _xform = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<GiantTarantulaComponent, ComponentStartup>(OnStartup);

            SubscribeLocalEvent<GiantTarantulaComponent, GiantTarantulaRaiseArmyActionEvent>(OnRaiseArmy);
            SubscribeLocalEvent<GiantTarantulaComponent, GiantTarantulaDomainActionEvent>(OnDomain);
        }

        private void OnStartup(EntityUid uid, GiantTarantulaComponent component, ComponentStartup args)
        {
            _action.AddAction(uid, component.ActionRaiseArmy, null);
            _action.AddAction(uid, component.ActionDomain, null);
        }

        /// <summary>
        /// Summons an allied rat servant at the King, costing a small amount of hunger
        /// </summary>
        private void OnRaiseArmy(EntityUid uid, GiantTarantulaComponent component, GiantTarantulaRaiseArmyActionEvent args)
        {
            if (args.Handled)
                return;

            if (!TryComp<HungerComponent>(uid, out var hunger))
                return;

            //make sure the hunger doesn't go into the negatives
            if (hunger.CurrentHunger < component.HungerPerArmyUse)
            {
                _popup.PopupEntity(Loc.GetString("giant-tarantula-too-hungry"), uid, Filter.Entities(uid));
                return;
            }
            args.Handled = true;
            hunger.CurrentHunger -= component.HungerPerArmyUse;
            Spawn(component.ArmyMobSpawnId, Transform(uid).Coordinates); //spawn the little mouse boi
        }

        /// <summary>
        /// uses hunger to release a specific amount of miasma into the air. This heals the rat king
        /// and his servants through a specific metabolism.
        /// </summary>
        private void OnDomain(EntityUid uid, GiantTarantulaComponent component, GiantTarantulaDomainActionEvent args)
        {
            if (args.Handled)
                return;

            if (!TryComp<HungerComponent>(uid, out var hunger))
                return;

            //make sure the hunger doesn't go into the negatives
            if (hunger.CurrentHunger < component.HungerPerDomainUse)
            {
                _popup.PopupEntity(Loc.GetString("giant-tarantula-too-hungry"), uid, Filter.Entities(uid));
                return;
            }
            args.Handled = true;
            hunger.CurrentHunger -= component.HungerPerDomainUse;

            if (_random.Prob(0.5f))
            {
                Spawn("Web2", Transform(uid).Coordinates.SnapToGrid(EntityManager));
            } else {
                Spawn("Web", Transform(uid).Coordinates.SnapToGrid(EntityManager));
            }
        }
    }

    public sealed class GiantTarantulaRaiseArmyActionEvent : InstantActionEvent { };
    public sealed class GiantTarantulaDomainActionEvent : InstantActionEvent { };
};
