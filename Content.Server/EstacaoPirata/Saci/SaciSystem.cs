using System.Threading;
using Content.Server.Body.Components;
using Content.Server.Body.Systems;
using Content.Server.Coordinates.Helpers;
using Content.Server.DoAfter;
using Content.Server.Doors.Components;
using Content.Server.Magic.Events;
using Content.Server.Weapons.Ranged.Systems;
using Content.Shared.Actions;
using Content.Shared.Actions.ActionTypes;
using Content.Shared.Body.Components;
using Content.Shared.Doors.Components;
using Content.Shared.Doors.Systems;
using Content.Shared.Interaction.Events;
using Content.Shared.Maps;
using Content.Shared.Physics;
using Content.Shared.Spawners.Components;
using Content.Shared.Storage;
using Robust.Shared.Audio;
using Robust.Shared.Map;
using Robust.Shared.Physics.Components;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Content.Server.EstacaoPirata.Saci;
using Content.Server.Magic;
using Robust.Shared.GameObjects;
using Content.Shared.Revenant;
using Content.Shared.Throwing;
using Content.Server.Storage.EntitySystems;
using Content.Server.Ghost;
using Content.Shared.Damage;
using Content.Shared.Tag;
using Content.Shared.Item;
using Content.Server.Light.Components;
using Content.Server.Maps;
using Content.Server.Storage.Components;
using Robust.Shared.Physics;
using System.Linq;
using Robust.Shared.Utility;

namespace Content.Server.EstacaoPirata.Saci;

public sealed class SaciSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actionsSystem = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IMapManager _mapManager = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly ThrowingSystem _throwing = default!;
    [Dependency] private readonly EntityStorageSystem _entityStorage = default!;
    [Dependency] private readonly GhostSystem _ghost = default!;
    [Dependency] private readonly DamageableSystem _damage = default!;
    [Dependency] private readonly TileSystem _tile = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly TagSystem _tag = default!;

        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<SaciPowersComponent, ComponentStartup>(SaciPowersStartup);
            SubscribeLocalEvent<SaciPowersComponent, ComponentShutdown>(SaciPowersShutdown);

            SubscribeLocalEvent<SaciPowersComponent, RevenantDefileActionEvent>(OnFuracaoAction);
    }
        private void SaciPowersStartup (EntityUid uid, SaciPowersComponent component, ComponentStartup args)
        {
            foreach (var id in component.WorldTargetActions)
            {
                var action = new WorldTargetAction(_prototypeManager.Index<WorldTargetActionPrototype>(id));
                _actionsSystem.AddAction(uid, action, null);
            }

            foreach (var id in component.EntityTargetActions)
            {
                var action = new EntityTargetAction(_prototypeManager.Index<EntityTargetActionPrototype>(id));
                _actionsSystem.AddAction(uid, action, null);
            }

            foreach (var id in component.SelfTargetActions)
            {
                var action = new InstantAction(_prototypeManager.Index<InstantActionPrototype>(id));
                _actionsSystem.AddAction(uid, action, null);
            }
        }

        private void SaciPowersShutdown(EntityUid uid, SaciPowersComponent component, ComponentShutdown args)
        {
            foreach (var id in component.WorldTargetActions)
            {
                var action = new WorldTargetAction(_prototypeManager.Index<WorldTargetActionPrototype>(id));
                _actionsSystem.RemoveAction(uid, action, null);
            }

            foreach (var id in component.EntityTargetActions)
            {
                var action = new EntityTargetAction(_prototypeManager.Index<EntityTargetActionPrototype>(id));
                _actionsSystem.RemoveAction(uid, action, null);
            }

            foreach (var id in component.SelfTargetActions)
            {
                var action = new InstantAction(_prototypeManager.Index<InstantActionPrototype>(id));
                _actionsSystem.RemoveAction(uid, action, null);
            }
        }
        private void OnFuracaoAction(EntityUid uid, SaciPowersComponent component, RevenantDefileActionEvent args)
    {
        if (args.Handled)
            return;

        args.Handled = true;

        //var coords = Transform(uid).Coordinates;
        //var gridId = coords.GetGridUid(EntityManager);
        var xform = Transform(uid);
        if (!_mapManager.TryGetGrid(xform.GridUid, out var map))
            return;
        var tiles = map.GetTilesIntersecting(Box2.CenteredAround(xform.WorldPosition,
            (component.DefileRadius*2, component.DefileRadius))).ToArray();

        _random.Shuffle(tiles);

        for (var i = 0; i < component.DefileTilePryAmount; i++)
        {
            if (!tiles.TryGetValue(i, out var value))
                continue;
            _tile.PryTile(value);
        }

        var lookup = _lookup.GetEntitiesInRange(uid, component.DefileRadius, LookupFlags.Approximate | LookupFlags.Static);
        var tags = GetEntityQuery<TagComponent>();
        var entityStorage = GetEntityQuery<EntityStorageComponent>();
        var items = GetEntityQuery<ItemComponent>();
        var lights = GetEntityQuery<PoweredLightComponent>();

        foreach (var ent in lookup)
        {
            //break windows
            if (tags.HasComponent(ent) && _tag.HasAnyTag(ent, "Window"))
            {
                //hardcoded damage specifiers til i die.
                var dspec = new DamageSpecifier();
                dspec.DamageDict.Add("Structural", 15);
                _damage.TryChangeDamage(ent, dspec, origin: uid);
            }

            if (!_random.Prob(component.DefileEffectChance))
                continue;

            //randomly opens some lockers and such.
            if (entityStorage.TryGetComponent(ent, out var entstorecomp))
                _entityStorage.OpenStorage(ent, entstorecomp);

            //chucks shit
            if (items.HasComponent(ent) &&
                TryComp<PhysicsComponent>(ent, out var phys) && phys.BodyType != BodyType.Static)
                _throwing.TryThrow(ent, _random.NextAngle().ToWorldVec());

            //flicker lights
            if (lights.HasComponent(ent))
                _ghost.DoGhostBooEvent(ent);
        }
    }
 }

