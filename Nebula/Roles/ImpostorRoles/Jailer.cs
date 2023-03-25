﻿using Nebula.Patches;
using Nebula.Roles.RoleSystem;

namespace Nebula.Roles.ImpostorRoles;

public class Jailer : Role
{
    public class JailerEvent : Events.LocalEvent
    {
        PlayerControl target;
        ExtraRole targetRole;
        public JailerEvent(PlayerControl target, ExtraRole targetRole) : base(0.2f) { this.targetRole = targetRole; this.target = target; }
        public override void OnActivate()
        {
            RPCEventInvoker.AddExtraRole(target, targetRole,0);
        }
    }

    public class InheritedJailer : ExtraRole
    {
        public override void Assignment(Patches.AssignMap assignMap){}

        public override void ButtonInitialize(HudManager __instance) => ImpAdminSystem.ButtonInitialize(__instance, Roles.Jailer.canMoveWithLookingMapOption.getBool(), Roles.Jailer.ignoreCommSabotageOption.getBool(), Roles.Jailer.canIdentifyImpostorsOption.getBool());

        public override void CleanUp() => ImpAdminSystem.CleanUp();

        public override void OnShowMapTaskOverlay(MapTaskOverlay mapTaskOverlay, Action<Vector2, bool> iconGenerator) => ImpAdminSystem.OnShowMapTaskOverlay(mapTaskOverlay, iconGenerator, Roles.Jailer.canMoveWithLookingMapOption.getBool(), Roles.Jailer.ignoreCommSabotageOption.getBool(), Roles.Jailer.canIdentifyImpostorsOption.getBool());

        public override void OnMapClose(MapBehaviour mapBehaviour) => ImpAdminSystem.OnMapClose(mapBehaviour);


        public InheritedJailer() : base("InheritedJailer", "inheritedJailer", Palette.ImpostorRed, 0)
        {
            IsHideRole = true;
        }
    }

    /* オプション */
    private Module.CustomOption? ignoreCommSabotageOption;
    private Module.CustomOption? canMoveWithLookingMapOption;
    private Module.CustomOption? canIdentifyImpostorsOption;
    private Module.CustomOption? inheritAbilityOption;

    private SpriteLoader madmateButtonSprite = new SpriteLoader("Nebula.Resources.MadmateButton.png", 115f);

    public override void LoadOptionData()
    {
        canMoveWithLookingMapOption = CreateOption(Color.white, "canMoveWithLookingMap", true);
        canIdentifyImpostorsOption = CreateOption(Color.white, "canIdentifyImpostors", true);
        ignoreCommSabotageOption = CreateOption(Color.white, "ignoreCommSabotage", true);
        inheritAbilityOption = CreateOption(Color.white, "canInheritAbility", false);
    }

    public override void ButtonInitialize(HudManager __instance) => ImpAdminSystem.ButtonInitialize(__instance,canMoveWithLookingMapOption.getBool(),ignoreCommSabotageOption.getBool(),canIdentifyImpostorsOption.getBool());

    /* ボタン */

    //public override void ButtonInitialize(HudManager __instance) => ImpAdminSystem.ButtonInitialize(__instance,canMoveWithLookingMapOption.getBool(),ignoreCommSabotageOption.getBool(),canIdentifyImpostorsOption.getBool());

    public override void CleanUp() => ImpAdminSystem.CleanUp();

    public override void OnRoleRelationSetting()
    {
        RelatedRoles.Add(Roles.Disturber);
        RelatedRoles.Add(Roles.Doctor);
        RelatedRoles.Add(Roles.NiceTrapper);
        RelatedRoles.Add(Roles.EvilTrapper);
        RelatedRoles.Add(Roles.Arsonist);
        RelatedRoles.Add(Roles.Opportunist);
    }

    public override void OnShowMapTaskOverlay(MapTaskOverlay mapTaskOverlay, Action<Vector2, bool> iconGenerator) => ImpAdminSystem.OnShowMapTaskOverlay(mapTaskOverlay,iconGenerator,canMoveWithLookingMapOption.getBool(),ignoreCommSabotageOption.getBool(),canIdentifyImpostorsOption.getBool());
  
    public override void OnMapClose(MapBehaviour mapBehaviour)=>ImpAdminSystem.OnMapClose(mapBehaviour);

    public override void OnDied()
    {
        if (!inheritAbilityOption.getBool()) return;

        var cand = (Game.GameData.data.AllPlayers.Values.Where((d) => d.IsAlive && d.role.side == Side.Impostor && d.role != Roles.Jailer)).ToArray();
        if (cand.Length == 0) return;

        RPCEventInvoker.AddExtraRole(Helpers.playerById(cand[NebulaPlugin.rnd.Next(cand.Length)].id), Roles.InheritedJailer, 0);
    }

    public Jailer()
        : base("Jailer", "jailer", Palette.ImpostorRed, RoleCategory.Impostor, Side.Impostor, Side.Impostor,
             Impostor.impostorSideSet, Impostor.impostorSideSet,
             Impostor.impostorEndSet,
             true, VentPermission.CanUseUnlimittedVent, true, true, true)
    {
    }
}


/*
using Nebula.Patches;

namespace Nebula.Roles.ImpostorRoles;

public class Jailer : Role
{
    public class JailerEvent : Events.LocalEvent
    {
        PlayerControl target;
        ExtraRole targetRole;
        public JailerEvent(PlayerControl target, ExtraRole targetRole) : base(0.2f) { this.targetRole = targetRole; this.target = target; }
        public override void OnTerminal()
        {
            RPCEventInvoker.AddExtraRole(target, targetRole,0);
        }
    }

    private Module.CustomOption? ignoreCommSabotageOption;
    private Module.CustomOption? canMoveWithLookingMapOption;
    private Module.CustomOption? canIdentifyImpostorsOption;
    public static Module.CustomOption createMadmateCoolDownOption;

    private SpriteLoader madmateButtonSprite = new SpriteLoader("Nebula.Resources.MadmateButton.png", 115f);

    MapCountOverlay? jailerCountOverlay = null;

    public bool IsJailerCountOverlay(MapCountOverlay overlay) => overlay == jailerCountOverlay;

    public override void LoadOptionData()
    {
        canMoveWithLookingMapOption = CreateOption(Color.white, "canMoveWithLookingMap", true);
        canIdentifyImpostorsOption = CreateOption(Color.white, "canIdentifyImpostors", true);
        ignoreCommSabotageOption = CreateOption(Color.white, "ignoreCommSabotage", true);
        createMadmateCoolDownOption = CreateOption(Color.white, "createMadmateCoolDown", 20f, 10f, 30f, 5f);
        createMadmateCoolDownOption.suffix = "second";
    }

    public int jailerDataId { get; private set; }
    public int leftMadmateDataId { get; private set; }

    public override void GlobalInitialize(PlayerControl __instance)
    {
        __instance.GetModData().SetRoleData(jailerDataId, __instance.PlayerId);
        __instance.GetModData().SetRoleData(leftMadmateDataId, 1);
    }

    public override void MyPlayerControlUpdate()
    {
        Game.MyPlayerData data = Game.GameData.data.myData;
        data.currentTarget = Patches.PlayerControlPatch.SetMyTarget(1f, true);
        Patches.PlayerControlPatch.SetPlayerOutline(data.currentTarget, Color.yellow);
    }

    static private CustomButton adminButton;
    static private CustomButton madmateButton;
    public override void ButtonInitialize(HudManager __instance)
    {
        jailerCountOverlay = null;

        if (adminButton != null)
        {
            adminButton.Destroy();
        }
        if (!canMoveWithLookingMapOption.getBool())
        {
            adminButton = new CustomButton(
                () =>
                {
                    RoleSystem.HackSystem.showAdminMap(ignoreCommSabotageOption.getBool(), canIdentifyImpostorsOption.getBool() ? AdminPatch.AdminMode.ImpostorsAndDeadBodies : AdminPatch.AdminMode.Default);
                },
                () => { return !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return PlayerControl.LocalPlayer.CanMove; },
                () => { },
                __instance.UseButton.fastUseSettings[ImageNames.AdminMapButton].Image,
                Expansion.GridArrangeExpansion.GridArrangeParameter.None,
                __instance,
                Module.NebulaInputManager.abilityInput.keyCode,
                "button.label.admin"
            );
            adminButton.MaxTimer = 0f;
            adminButton.Timer = 0f;
        }
        else
        {
            adminButton = null;
        }

        if (madmateButton != null)
        {
            madmateButton.Destroy();
        }
        madmateButton = new CustomButton(
            () =>
            {
                //Madmate生成
                Events.LocalEvent.Activate(new JailerEvent(Game.GameData.data.myData.currentTarget, Roles.SecondaryMadmate));
                RPCEventInvoker.AddAndUpdateRoleData(PlayerControl.LocalPlayer.PlayerId, leftMadmateDataId, -1);
                Game.GameData.data.myData.getGlobalData().role.OnAnyoneRoleChanged(Game.GameData.data.myData.currentTarget.PlayerId);

                Game.GameData.data.myData.currentTarget = null;
            },
            () => { return !PlayerControl.LocalPlayer.Data.IsDead && Game.GameData.data.myData.getGlobalData().GetRoleData(leftMadmateDataId) > 0; },
            () => {
                return Game.GameData.data.myData.currentTarget && PlayerControl.LocalPlayer.CanMove;
            },
            () => { madmateButton.Timer = madmateButton.MaxTimer; },
            madmateButtonSprite.GetSprite(),
            Expansion.GridArrangeExpansion.GridArrangeParameter.LeftSideContent,
            __instance,
            Module.NebulaInputManager.abilityInput.keyCode,
            "button.label.madmate"
        );
        madmateButton.MaxTimer = createMadmateCoolDownOption.getFloat();

    }
    public override void CleanUp()
    {
        if (adminButton != null)
        {
            adminButton.Destroy();
            adminButton = null;
        }

        if (madmateButton != null)
        {
            madmateButton.Destroy();
            madmateButton = null;
        }

        if (jailerCountOverlay)
        {
            GameObject.Destroy(jailerCountOverlay.gameObject);
        }
        jailerCountOverlay = null;

        if (MapBehaviour.Instance) GameObject.Destroy(MapBehaviour.Instance.gameObject);
    }

    public override void OnRoleRelationSetting()
    {
        RelatedRoles.Add(Roles.Disturber);
        RelatedRoles.Add(Roles.Doctor);
        RelatedRoles.Add(Roles.NiceTrapper);
        RelatedRoles.Add(Roles.EvilTrapper);
        RelatedRoles.Add(Roles.Arsonist);
        RelatedRoles.Add(Roles.Opportunist);
    }

    public override void OnShowMapTaskOverlay(MapTaskOverlay mapTaskOverlay, Action<Vector2, bool> iconGenerator)
    {
        if (!canMoveWithLookingMapOption.getBool()) return;

        if (jailerCountOverlay == null)
        {
            jailerCountOverlay = GameObject.Instantiate(MapBehaviour.Instance.countOverlay);
            jailerCountOverlay.transform.SetParent(MapBehaviour.Instance.transform);
            jailerCountOverlay.transform.localPosition = MapBehaviour.Instance.countOverlay.transform.localPosition;
            jailerCountOverlay.transform.localScale = MapBehaviour.Instance.countOverlay.transform.localScale;
            jailerCountOverlay.gameObject.name = "JailerCountOverlay";

            Transform roomNames;
            if (GameOptionsManager.Instance.CurrentGameOptions.MapId == 0)
                roomNames = MapBehaviour.Instance.transform.FindChild("RoomNames (1)");
            else
                roomNames = MapBehaviour.Instance.transform.FindChild("RoomNames");
            Map.MapEditor.MapEditors[GameOptionsManager.Instance.CurrentGameOptions.MapId].MinimapOptimizeForJailer(roomNames, jailerCountOverlay, MapBehaviour.Instance.infectedOverlay);
        }

        jailerCountOverlay.gameObject.SetActive(true);

        Patches.AdminPatch.adminMode = canIdentifyImpostorsOption.getBool() ? AdminPatch.AdminMode.ImpostorsAndDeadBodies : AdminPatch.AdminMode.Default;
        Patches.AdminPatch.isAffectedByCommAdmin = !ignoreCommSabotageOption.getBool();
        Patches.AdminPatch.isStandardAdmin = false;
        Patches.AdminPatch.shouldChangeColor = false;
    }

    /// <summary>
    /// マップを閉じるときに呼び出されます。
    /// </summary>
    [RoleLocalMethod]
    public override void OnMapClose(MapBehaviour mapBehaviour)
    {
        if (jailerCountOverlay != null) jailerCountOverlay.gameObject.SetActive(false);
    }

    public Jailer()
        : base("Jailer", "jailer", Palette.ImpostorRed, RoleCategory.Impostor, Side.Impostor, Side.Impostor,
             Impostor.impostorSideSet, Impostor.impostorSideSet,
             Impostor.impostorEndSet,
             true, VentPermission.CanUseUnlimittedVent, true, true, true)
    {
        adminButton = null;
        madmateButton = null;

        jailerDataId = Game.GameData.RegisterRoleDataId("jailer.identifier");
        leftMadmateDataId = Game.GameData.RegisterRoleDataId("jailer.leftMadmate");
    }
}
*/