using System.Collections;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using BepInEx.Unity.IL2CPP.Utils;
using HarmonyLib;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.Injection;
using Il2CppSystem.IO;
using UnityEngine;
using WildfrostModMiya.Hook.Dobby;
using Object = UnityEngine.Object;
using Random = Dead.Random;

namespace WildfrostModMiya;

[BepInPlugin("WildFrost.Miya.WildfrostAPI", "WildfrostAPI", "0.2.2.0")]
public class WildFrostAPIMod : BasePlugin
{
    public static string ModsFolder = typeof(WildFrostAPIMod).Assembly.Location.Replace("WildfrostModMiya.dll", "");
    public static WildFrostAPIMod Instance;

    internal static Dictionary<string, List<Object>>
        GroupAdditions = new()
        {
            { "CardData", new List<Object>() },
            { "StatusEffectData", new List<Object>() },
            { "CardUpgradeData", new List<Object>() }
        };

    internal static Dictionary<string, Action<int>>
        EventsCallers = new()
        {
            { "CardData", delegate { CardAdder.LaunchEvent(); } },
            { "StatusEffectData", delegate { StatusEffectAdder.LaunchEvent(); } },
            { "CardUpgradeData", delegate { CardUpgradeAdder.LaunchEvent(); } }
        };

    /// <summary>
    ///     Method to register custom addressable loader hooks, only use if you know what youre doing
    ///     will be removed when api has support for all data types
    /// </summary>
    /// <param name="typeName"></param>
    /// <param name="list"></param>
    /// <param name="eventToRegisterStuff"></param>
    /// <returns>
    ///     returns false if what you are trying to do is not allowed, ie: trying to add support for already supported
    ///     types
    /// </returns>
    public static bool RegisterCustomAddressableLoaderHook(string typeName, ref List<Object> list,
        Action<int> eventToRegisterStuff)
    {
        if (GroupAdditions.ContainsKey(typeName)) return false;
        GroupAdditions.Add(typeName, list);
        EventsCallers.Add(typeName, eventToRegisterStuff);
        return true;
    }

    private static void GroupConstructorPatch(IntPtr t, IntPtr items)
    {
        var group = new AddressableLoader.Group(t);
        group.list = new Il2CppSystem.Collections.Generic.List<Object>();
        group.lookup = new Il2CppSystem.Collections.Generic.Dictionary<string, Object>();
        var data =
            new Il2CppSystem.Collections.Generic.IEnumerable<Object>(items);
        foreach (var @object in data.ToArray())
        {
            group.list.Add(@object);
            group.lookup[AddressableLoader.Group.GetName(@object)] = @object;
        }

        var typeName = data.ToArray()[0].GetIl2CppType().ToString();
        if (EventsCallers.ContainsKey(typeName)) EventsCallers[typeName].Invoke(0);

        Instance.Log.LogInfo(typeName + " is the typename");
        if (GroupAdditions.ContainsKey(typeName))
            foreach (var @object in GroupAdditions[typeName].ToArray())
            {
                Instance.Log.LogInfo("Added a " + typeName + " " + @object.name);
                group.list.Add(@object);
                group.lookup[AddressableLoader.Group.GetName(@object)] = @object;
            }
    }

    private static readonly GroupConstructorDelegate Test = GroupConstructorPatch;

    private delegate void GroupConstructorDelegate(IntPtr t, IntPtr items);
    
    
    

    public void AddDebugStuff()
    {
        CardAdder.OnAskForAddingCards += delegate
        {
            CardAdder.CreateCardData("API", "DebugCard")
                .SetTitle("Debug Card")
                .SetIsUnit()
                //.AddToPool(CardAdder.VanillaRewardPools.BasicItemPool) debug cards shouldn't be in pools
                .SetCanPlay(CardAdder.CanPlay.CanPlayOnEnemy | CardAdder.CanPlay.CanPlayOnBoard)
                .SetSprites("CardPortraits\\testPortrait", "CardPortraits\\testBackground")
                .SetStats(4, 1, 3)
                .SetBloodProfile(CardAdder.VanillaBloodProfiles.BloodProfilePinkWisp)
                .SetIdleAnimationProfile(CardAdder.VanillaCardAnimationProfiles.GoopAnimationProfile)
                .SetStartWithEffects("API.DebugEffect".StatusEffectStack(1))
                .RegisterCardInApi();
        };
        StatusEffectAdder.OnAskForAddingStatusEffects += delegate
        {
            StatusEffectAdder.CreateStatusEffectData<StatusEffectApplyXWhenHit>("API", "DebugEffect").ModifyFields(
                delegate(StatusEffectApplyXWhenHit data)
                {
                    data.effectToApply = CardAdder.VanillaStatusEffects.Demonize.StatusEffectData();
                    data = data.SetText("Apply {0} to front enemy when hit");
                    data.applyToFlags = StatusEffectApplyX.ApplyToFlags.FrontEnemy;
                    data.textInsert = "<{a}><keyword=demonize>";
                    return data;
                }).RegisterStatusEffectInApi();
        };
        CardUpgradeAdder.OnAskForAddingCardUpgrades += delegate
        {
            var data = CardUpgradeAdder.CreateCardUpgradeData("API", "DebugCardUpgrade")
                .SetAttackEffects(CardAdder.VanillaStatusEffects.Haze.StatusEffectStack(1))
                .SetText("Gain one <keyword=haze>.")
                .SetTitle("Haze charm").AddToPool(CardAdder.VanillaRewardPools.GeneralCharmPool,
                    CardAdder.VanillaRewardPools.BasicCharmPool, CardAdder.VanillaRewardPools.MagicCharmPool,
                    CardAdder.VanillaRewardPools.ClunkCharmPool)
                .SetUpgradeType(CardUpgradeData.Type.Charm)
                .SetImage("CardPortraits\\CharmTemplate");

            data.RegisterCardUpgradeData();
        };
    }

    private static IEnumerator DIRTY_ConsoleStuff()
    {
        yield return new WaitUntil((Func<bool>)(() => SceneManager.IsLoaded("Console")));
        var go = Object.FindObjectOfType<Console>();
        go?.Toggle();
    }

    private static int seed = -1;

    [HarmonyPatch(typeof(CampaignData), nameof(CampaignData.Init))]
    private class Patch
    {
        [HarmonyPostfix]
        private static void Postfix(CampaignData __instance)
        {
            if (Instance.configSeedManipulation.Value && seed != -1) __instance.Seed = seed;
        }
    }

    private static void DIRTY_DebugGui()
    {
        if (Instance.configOpenConsoleButton.Value)
            if (GUILayout.Button("Try open console?"))
            {
                if (!SceneManager.IsLoaded("Console"))
                    CoroutineManager.Start(SceneManager.Load("Console", SceneType.Persistent));
                Instance._GameObject.StartCoroutine(DIRTY_ConsoleStuff());
            }

        if (Instance.configSeedManipulation.Value)
        {
            var oldSeed = seed;
            seed = int.Parse(GUILayout.TextField(seed.ToString()));
            if (GUILayout.Button("Randomize Seed")) seed = Random.Seed();

            if (oldSeed != seed)
            {
                var o = Object.FindObjectOfType<SelectLeader>();
                if (o != null)
                {
                    o.SetSeed(seed);
                    o.Reroll();
                }
            }
        }

        if (Instance.configWinBattleButton.Value)
            if (GUILayout.Button("Try win battle?"))
                Battle.instance.PlayerWin();
        if (Instance.configGiveDebugCardButton.Value)
            if (GUILayout.Button("Give me debug card!"))
            {
                CardData card = null;
                foreach (var cardinList in AddressableLoader.groups["CardData"].list)
                    if (cardinList.Cast<CardData>().name == "API.DebugCard")
                    {
                        card = cardinList.Cast<CardData>();
                        break;
                    }

                if (card != null)
                {
                    Instance.Log.LogInfo("Gave out card " + card.title);
                    var clone = card.Clone();
                    clone.original = card;
                    Campaign.instance.characters._items[0].data.inventory.deck.Add(clone);
                    clone.id = (ulong)Object.FindObjectsOfType<CardData>().Count * 10;
                    Campaign.instance.characters._items[0].data.inventory.Save();
                }
                else
                {
                    Instance.Log.LogInfo("No such card!");
                }
            }
    }

    internal static List<CardAnimationProfile> VanillaAnimationProfiles
    {
        get
        {
            if (_VanillaAnimationProfiles == null) CreateVanillaAnimationProfiles();

            return _VanillaAnimationProfiles;
        }
    }

    internal static List<CardAnimationProfile> _VanillaAnimationProfiles;

    private static void CreateVanillaAnimationProfiles()
    {
        _VanillaAnimationProfiles = new List<CardAnimationProfile>();
        var list = Object.FindObjectsOfTypeIncludingAssets(Il2CppType.Of<CardAnimationProfile>());
        foreach (var profile in list) _VanillaAnimationProfiles.Add(profile.Cast<CardAnimationProfile>());
    }

    internal static List<BloodProfile> VanillaBloodProfiles
    {
        get
        {
            if (_VanillaBloodProfiles == null) CreateVanillaBloodProfiles();

            return _VanillaBloodProfiles;
        }
    }

    internal static List<BloodProfile> _VanillaBloodProfiles;

    private static void CreateVanillaBloodProfiles()
    {
        _VanillaBloodProfiles = new List<BloodProfile>();
        var list = Object.FindObjectsOfTypeIncludingAssets(Il2CppType.Of<BloodProfile>());
        foreach (var profile in list) _VanillaBloodProfiles.Add(profile.Cast<BloodProfile>());
    }

    internal static List<TargetMode> VanillaTargetModes
    {
        get
        {
            if (_VanillaTargetModes == null) CreateVanillaTargetModes();

            return _VanillaTargetModes;
        }
    }

    internal static List<TargetMode> _VanillaTargetModes;

    private static void CreateVanillaTargetModes()
    {
        _VanillaTargetModes = new List<TargetMode>();
        var list = Object.FindObjectsOfTypeIncludingAssets(Il2CppType.Of<TargetMode>());
        foreach (var profile in list) _VanillaTargetModes.Add(profile.Cast<TargetMode>());
    }

    public class APIGameObject : MonoBehaviour
    {
        public static APIGameObject instance;

        private void Awake()
        {
            instance = this;
        }

        public void OnGUI()
        {
            DIRTY_DebugGui();
        }
    }

    private APIGameObject _GameObject;
    private ConfigEntry<bool> configWinBattleButton;
    private ConfigEntry<bool> configGiveDebugCardButton;
    private ConfigEntry<bool> configOpenConsoleButton;
    private ConfigEntry<bool> configSeedManipulation;

    public override unsafe void Load()
    {
        configWinBattleButton = Config.Bind("Debug.Toggles",
            "WinBattleButton",
            false,
            "Whether or not to show the \"try win battle?\" button");

        configGiveDebugCardButton = Config.Bind("Debug.Toggles",
            "GiveDebugCardButton",
            false,
            "Whether or not to show the \"give me debug card!\" button");
        configOpenConsoleButton = Config.Bind("Debug.Toggles",
            "OpenConsoleButton",
            false,
            "Whether or not to show the \"OpenConsole!\" button");

        configSeedManipulation = Config.Bind("Utils.Toggles",
            "SeedManipulation",
            true,
            "Whether or not to show the seed manipulation ui");
        Instance = this;
        var test =
            *(IntPtr*)IL2CPP.GetIl2CppMethodByToken(
                Il2CppClassPointerStore<AddressableLoader.Group>.NativeClassPtr, 100664736 );
        new DobbyDetour(test, Test).Apply();
        Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), "WildFrost.Miya.WildfrostAPI");
        ClassInjector.RegisterTypeInIl2Cpp<CardAnimationProfile>();
        ClassInjector.RegisterTypeInIl2Cpp<BloodProfile>();
        ClassInjector.RegisterTypeInIl2Cpp<TargetMode>();
        ClassInjector.RegisterTypeInIl2Cpp<RewardPool>();
        ClassInjector.RegisterTypeInIl2Cpp<APIGameObject>();
        AddDebugStuff();
        CardAdder.OnAskForAddingCards += JSONApi.AddJSONCards;
        _GameObject = AddComponent<APIGameObject>();
        Log.LogInfo("WildFrost API Loaded!");
    }
}