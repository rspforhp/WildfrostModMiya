using HarmonyLib;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.Injection;
using Il2CppSystem.Collections;
using Il2CppSystem.IO;
using Il2CppSystem.Reflection;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using UnityEngine.Pool;
using UnityEngine.ResourceManagement.AsyncOperations;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using BepInEx.Unity.IL2CPP.Utils;
using Cpp2IL.Core.Attributes;
using WildfrostModMiya;
using ClassInjector = Il2CppInterop.Runtime.Injection.ClassInjector;
using Color = System.Drawing.Color;
using IEnumerator = System.Collections.IEnumerator;
using Il2CppType = Il2CppInterop.Runtime.Il2CppType;
using Object = Il2CppSystem.Object;
using Random = System.Random;


namespace WildfrostModMiya;

[BepInPlugin("WildFrost.Miya.WildfrostAPI", "WildfrostAPI", "0.1.3.1")]
public class WildFrostAPIMod : BasePlugin
{
    public static string ModsFolder = typeof(WildFrostAPIMod).Assembly.Location.Replace("WildfrostModMiya.dll", "");
    public static WildFrostAPIMod Instance;


    [HarmonyPatch(typeof(PreloadAddressableGroup), nameof(PreloadAddressableGroup.Start))]
    class StartPatch
    {
        [HarmonyPostfix]
        static void Postfix(PreloadAddressableGroup __instance)
        {
            Instance.Log.LogInfo("Preload assets run! " + CardDataAdditions.Count);
            ShouldInjectCards = true;
        }
    }


    public static bool ShouldInjectCards;
    internal static List<CardData> CardDataAdditions = new List<CardData>();
    internal static List<StatusEffectData> StatusEffectDataAdditions = new List<StatusEffectData>();
    internal static List<CardUpgradeData> CardUpgradeDataAdditions = new List<CardUpgradeData>();


    public void AddDebugStuff()
    {
        CardAdder.OnAskForAddingCards += delegate(int i)
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
        StatusEffectAdder.OnAskForAddingStatusEffects += delegate(int i)
        {
            StatusEffectAdder.CreateStatusEffectData<StatusEffectApplyXWhenHit>("API", "DebugEffect").ModifyFields(
                delegate(StatusEffectApplyXWhenHit data)
                {
                    data.effectToApply = CardAdder.VanillaStatusEffects.Demonize.StatusEffectData();
                    data = data.SetText("Apply {0} to front enemy when hit");
                    data.applyToFlags = StatusEffectApplyX.ApplyToFlags.FrontEnemy;
                    data.textInsert = "<{a}><keyword=demonize>";
                    return data;
                }).RegisterStatusEffectInApi<StatusEffectApplyXWhenHit>();
        };
        CardUpgradeAdder.OnAskForAddingCardUpgrades += delegate(int i)
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

    private static bool MatchCardName(Object o, string name)
    {
        var card = o.Cast<CardData>();
        return card.name.Equals(name, StringComparison.OrdinalIgnoreCase) ||
               card.title.Equals(name, StringComparison.OrdinalIgnoreCase);
    }

    private static IEnumerator DIRTY_ConsoleStuff()
    {
        yield return new WaitUntil((Func<bool>)(() => SceneManager.IsLoaded("Console")));
        var go = UnityEngine.Object.FindObjectOfType<Console>();
        go?.Toggle();
    }

    private static int seed=-1;

    [HarmonyPatch(typeof(CampaignData), nameof(CampaignData.Init))]
    class Patch
    {
        [HarmonyPostfix]
        static void Postfix(CampaignData __instance)
        {
            if (Instance.configSeedManipulation.Value&& seed!=-1)
            {
                __instance.Seed = seed;
            }
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
            seed = int.Parse(GUILayout.TextField(seed.ToString()));
            if (GUILayout.Button("Randomize Seed"))
            {
                seed = Dead.Random.Seed();
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
                    if (MatchCardName(cardinList, "API.DebugCard"))
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
                    clone.id = (ulong)UnityEngine.Object.FindObjectsOfType<CardData>().Count * 10;
                    Campaign.instance.characters._items[0].data.inventory.Save();
                }
                else
                {
                    Instance.Log.LogInfo("No such card!");
                }
            }
    }


    internal static List<CardAnimationProfile> VanillaAnimationProfiles;

    private static void CreateVanillaAnimationProfiles()
    {
        VanillaAnimationProfiles = new List<CardAnimationProfile>();
        var list = UnityEngine.Object.FindObjectsOfTypeIncludingAssets(Il2CppType.Of<CardAnimationProfile>());
        foreach (var profile in list)
        {
            VanillaAnimationProfiles.Add(profile.Cast<CardAnimationProfile>());
        }
    }

    internal static List<BloodProfile> VanillaBloodProfiles;

    private static void CreateVanillaBloodProfiles()
    {
        VanillaBloodProfiles = new List<BloodProfile>();
        var list = UnityEngine.Object.FindObjectsOfTypeIncludingAssets(Il2CppType.Of<BloodProfile>());
        foreach (var profile in list)
        {
            VanillaBloodProfiles.Add(profile.Cast<BloodProfile>());
        }
    }

    internal static List<TargetMode> VanillaTargetModes;

    private static void CreateVanillaTargetModes()
    {
        VanillaTargetModes = new List<TargetMode>();
        var list = UnityEngine.Object.FindObjectsOfTypeIncludingAssets(Il2CppType.Of<TargetMode>());
        foreach (var profile in list)
        {
            VanillaTargetModes.Add(profile.Cast<TargetMode>());
        }
    }


    [HarmonyPatch(typeof(JournalCardManager), nameof(JournalCardManager.LoadCardData))]
    class LoadCardDataPatch
    {
        [HarmonyPostfix]
        static void Postfix(JournalCardManager.Category category, JournalCardManager __instance,
            ref Il2CppSystem.Collections.Generic.List<Il2CppSystem.Collections.Generic.KeyValuePair<string, CardData>>
                __result)
        {
            foreach (var data in CardDataAdditions)
            {
                if (data.cardType == category.cards[0].Asset.Cast<CardData>().cardType)
                {
                    __result.Add(new Il2CppSystem.Collections.Generic.KeyValuePair<string, CardData>(
                        data.title, data
                    ));
                    __instance.discovered.Add(data.name);
                }
            }
        }
    }


    public class APIGameObject : MonoBehaviour
    {
        public void OnGUI()
        {
            DIRTY_DebugGui();
        }

        public void Update()
        {
            if (
                ShouldInjectCards && AddressableLoader.groups.ContainsKey("CardData"))
            {
                if (!AddressableLoader.IsGroupLoaded("StatusEffectData"))
                {
                    CoroutineManager.Start(AddressableLoader.LoadGroup("StatusEffectData"));
                    return;
                }

                if (!AddressableLoader.IsGroupLoaded("CardUpgradeData"))
                {
                    CoroutineManager.Start(AddressableLoader.LoadGroup("CardUpgradeData"));
                    return;
                }

                if (!AddressableLoader.IsGroupLoaded("TraitData"))
                {
                    CoroutineManager.Start(AddressableLoader.LoadGroup("TraitData"));
                    return;
                }


                CreateVanillaAnimationProfiles();
                if (VanillaAnimationProfiles.Count == 0) return;

                CreateVanillaBloodProfiles();
                if (VanillaBloodProfiles.Count == 0) return;

                CreateVanillaTargetModes();
                if (VanillaTargetModes.Count == 0) return;

                if (StatusEffectDataAdditions != null)
                {
                    foreach (var oldCard in StatusEffectDataAdditions)
                    {
                        Destroy(oldCard);
                    }
                }
                StatusEffectDataAdditions = new();

                StatusEffectAdder.LaunchEvent();
                for (int i = 0; i < StatusEffectDataAdditions.Count; i++)
                {
                    var c = StatusEffectDataAdditions[i];
                    if (!AddressableLoader.groups["StatusEffectData"].lookup.ContainsKey(c.name))
                    {
                        AddressableLoader.groups["StatusEffectData"].list.Add(c);
                        AddressableLoader.groups["StatusEffectData"].lookup[c.name] = c;
                    }

                    Instance.Log.LogInfo($"StatusEffect {c.name} is injected by api!");
                }

                if (CardUpgradeDataAdditions != null)
                {
                    foreach (var oldCard in CardUpgradeDataAdditions)
                    {
                        Destroy(oldCard);
                    }
                }
                CardUpgradeDataAdditions = new List<CardUpgradeData>();
                CardUpgradeAdder.LaunchEvent();
                for (int i = 0; i < CardUpgradeDataAdditions.Count; i++)
                {
                    var c = CardUpgradeDataAdditions[i];
                    if (!AddressableLoader.groups["CardUpgradeData"].lookup.ContainsKey(c.name))
                    {
                        AddressableLoader.groups["CardUpgradeData"].list.Add(c);
                        AddressableLoader.groups["CardUpgradeData"].lookup[c.name] = c;
                    }

                    Instance.Log.LogInfo($"CardUpgradeData {c.name} is injected by api!");
                }

                if (CardDataAdditions != null)
                {
                    foreach (var oldCard in CardDataAdditions)
                    {
                        Destroy(oldCard);
                    }
                }
                CardDataAdditions = new List<CardData>();
                CardAdder.LaunchEvent();
                for (int i = 0; i < CardDataAdditions.Count; i++)
                {
                    var c = CardDataAdditions[i];
                    if (!AddressableLoader.groups["CardData"].lookup.ContainsKey(c.name))
                    {
                        AddressableLoader.groups["CardData"].list.Add(c);
                        AddressableLoader.groups["CardData"].lookup.Add(c.name, c);
                    }

                    Instance.Log.LogInfo($"Card {c.name} is injected by api!");
                }


                ShouldInjectCards = false;
            }
        }
    }

    private APIGameObject _GameObject;


    private ConfigEntry<bool> configWinBattleButton;
    private ConfigEntry<bool> configGiveDebugCardButton;
    private ConfigEntry<bool> configOpenConsoleButton;
    private ConfigEntry<bool> configSeedManipulation;

    public override void Load()
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
        Harmony.CreateAndPatchAll(System.Reflection.Assembly.GetExecutingAssembly(), "WildFrost.Miya.WildfrostAPI");
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