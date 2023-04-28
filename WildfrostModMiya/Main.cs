using HarmonyLib;
using Il2Cpp;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.Injection;
using Il2CppSystem.Collections;
using Il2CppSystem.IO;
using Il2CppSystem.Reflection;
using MelonLoader;
using UnhollowerBaseLib;
using UnhollowerRuntimeLib;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using UnityEngine.Pool;
using UnityEngine.ResourceManagement.AsyncOperations;
using UniverseLib;
using WildfrostModMiya;
using ClassInjector = Il2CppInterop.Runtime.Injection.ClassInjector;
using Color = System.Drawing.Color;
using Console = Il2Cpp.Console;
using IEnumerator = System.Collections.IEnumerator;
using Il2CppType = Il2CppInterop.Runtime.Il2CppType;
using Object = Il2CppSystem.Object;

[assembly: MelonInfo(typeof(WildFrostAPIMod), "WildFrost API", "1.1", "Kopie_Miya")]
[assembly: MelonGame("Deadpan Games", "Wildfrost")]

namespace WildfrostModMiya;

public partial class WildFrostAPIMod : MelonMod
{
    

    public static string ModsFolder = typeof(WildFrostAPIMod).Assembly.Location.Replace("WildfrostModMiya.dll", "");
    public static WildFrostAPIMod Instance;


    public override void OnSceneWasLoaded(int buildIndex, string sceneName)
    {
        base.OnSceneWasLoaded(buildIndex, sceneName);
    }

    [HarmonyPatch(typeof(PreloadAddressableGroup), nameof(PreloadAddressableGroup.Start))]
    class StartPatch
    {
        [HarmonyPostfix]
        static void Postfix(PreloadAddressableGroup __instance)
        {
            Instance.LoggerInstance.Msg("Preload assets run! "+CardDataAdditions.Count);
            ShouldInjectCards = true;
          

        }
    }

    public static Il2CppSystem.Collections.Generic.Dictionary<string, string> LocalizationAPI=new  Il2CppSystem.Collections.Generic.Dictionary<string, string>()
    {
        ["LOCALTest"]="Translated test"
    };


    public static bool ShouldInjectCards;
    internal static  List<CardData> CardDataAdditions=new List<CardData>();
    internal static  List<StatusEffectData> StatusEffectDataAdditions=new List<StatusEffectData>();
    internal static  List<CardUpgradeData> CardUpgradeDataAdditions=new List<CardUpgradeData>();


 
    public void AddDebugStuff()
    {
        CardAdder.OnAskForAddingCards+= delegate(int i)
        {
            CardAdder.CreateCardData("API","DebugCard")
                .SetTitle("Debug Card")
                .SetIsUnit()
                //.AddToPool(CardAdder.VanillaRewardPools.BasicItemPool) debug cards shouldn't be in pools
                .SetCanPlay(CardAdder.CanPlay.CanPlayOnEnemy | CardAdder.CanPlay.CanPlayOnBoard)
                .SetSprites("CardPortraits\\testPortrait","CardPortraits\\testBackground")
                .SetStats(4,1,3)
                .SetBloodProfile(CardAdder.VanillaBloodProfiles.BloodProfilePinkWisp)
                .SetIdleAnimationProfile(CardAdder.VanillaCardAnimationProfiles.GoopAnimationProfile)
                .SetStartWithEffects("API.DebugEffect".StatusEffectStack(1))
                .RegisterCardInApi();

        };
        StatusEffectAdder.OnAskForAddingStatusEffects+= delegate(int i)
        {
            StatusEffectAdder.CreateStatusEffectData<StatusEffectApplyXWhenHit>("API", "DebugEffect").ModifyFields(
                delegate(StatusEffectApplyXWhenHit data)
                {
                    data.effectToApply = CardAdder.VanillaStatusEffects.Demonize.StatusEffectData();
                    data=data.SetText("Apply {0} to front enemy when hit");
                        data.applyToFlags =  StatusEffectApplyX.ApplyToFlags.FrontEnemy;
                    data.textInsert = "<{a}><keyword=demonize>";
                    return data;
                }).RegisterStatusEffectInApi<StatusEffectApplyXWhenHit>();
            
        };
        CardUpgradeAdder.OnAskForAddingCardUpgrades+= delegate(int i)
        {
            var data = CardUpgradeAdder.CreateCardUpgradeData("API", "DebugCardUpgrade")
                .SetAttackEffects(CardAdder.VanillaStatusEffects.Haze.StatusEffectStack(1)).SetText("Gain one <keyword=haze>.")
                .SetTitle("Haze charm").AddToPool(CardAdder.VanillaRewardPools.GeneralCharmPool,
                    CardAdder.VanillaRewardPools.BasicCharmPool, CardAdder.VanillaRewardPools.MagicCharmPool,
                    CardAdder.VanillaRewardPools.ClunkCharmPool)
                .SetUpgradeType(CardUpgradeData.Type.Charm)
                .SetImage("CardPortraits\\CharmTemplate");
          
               data.RegisterCardUpgradeData();
        };
    }
    private bool MatchCardName(Object o,string name)
    {
        var card = o.Cast<CardData>();
        return card.name.Equals(name, StringComparison.OrdinalIgnoreCase) || card.title.Equals(name, StringComparison.OrdinalIgnoreCase);
    }

    private IEnumerator DIRTY_ConsoleStuff()
    {
        yield return new WaitUntil((Func<bool>)(() => SceneManager.IsLoaded("Console")));
        var go=UnityEngine.Object.FindObjectOfType<Console>();
        go?.Toggle();
        
    }

    private void DIRTY_DebugGui()
    {
        if (GUILayout.Button("Try open console?"))
        {
            if(! SceneManager.IsLoaded("Console"))
            CoroutineManager.Start(SceneManager.Load("Console", SceneType.Persistent));
            MelonCoroutines.Start(DIRTY_ConsoleStuff());
        }

        
        if (GUILayout.Button("Try win battle?")) Battle.instance.PlayerWin();
        if (GUILayout.Button("Give me debug card!") )
        {
            CardData card = null;
            foreach (var cardinList in AddressableLoader.groups["CardData"].list)
                if (MatchCardName(cardinList,"API.DebugCard"))
                {
                    card = cardinList.Cast<CardData>();
                    break;
                }

            if (card != null)
            {
                LoggerInstance.Msg("Gave out card " + card.title);
                var clone = card.Clone();
                clone.original = card;
                Campaign.instance.characters._items[0].data.inventory.deck.Add(clone);
                clone.id = (ulong)UnityEngine.Object.FindObjectsOfType<CardData>().Count * 10;
                Campaign.instance.characters._items[0].data.inventory.Save();
            }
            else
            {
                LoggerInstance.Msg("No such card!");
            }
        }

    }
    public override void OnGUI()
    {
        #if DEBUG
      DIRTY_DebugGui();
#endif
        base.OnGUI();
    }

    internal List<CardAnimationProfile> VanillaAnimationProfiles;

    private void CreateVanillaAnimationProfiles()
    {
        VanillaAnimationProfiles = new List<CardAnimationProfile>();
        var list = UnityEngine.Object.FindObjectsOfTypeIncludingAssets(Il2CppType.Of<CardAnimationProfile>());
        foreach (var profile in list)
        {
            VanillaAnimationProfiles.Add(profile.Cast<CardAnimationProfile>());
        }
    }

    internal List<BloodProfile> VanillaBloodProfiles;

    private void CreateVanillaBloodProfiles()
    {
        VanillaBloodProfiles = new List<BloodProfile>();
        var list = UnityEngine.Object.FindObjectsOfTypeIncludingAssets(Il2CppType.Of<BloodProfile>());
        foreach (var profile in list)
        {
            VanillaBloodProfiles.Add(profile.Cast<BloodProfile>());
        }
    }
    
    internal List<TargetMode> VanillaTargetModes;

    private void CreateVanillaTargetModes()
    {
        VanillaTargetModes = new List<TargetMode>();
        var list =UnityEngine.Object.FindObjectsOfTypeIncludingAssets(Il2CppType.Of<TargetMode>());
        foreach (var profile in list)
        {
            VanillaTargetModes.Add(profile.Cast<TargetMode>());
        }
    }



    [HarmonyPatch(typeof(JournalCardManager), nameof(JournalCardManager.LoadCardData))]
    class LoadCardDataPatch
    {
        [HarmonyPostfix]
        static void Postfix(JournalCardManager.Category category,JournalCardManager __instance,ref Il2CppSystem.Collections.Generic.List< Il2CppSystem.Collections.Generic.KeyValuePair<string,CardData>> __result)
        {
            foreach (var data in CardDataAdditions)
            {
                if (data.cardType == category.cards[0].Asset.Cast<CardData>().cardType)
                {
                    __result.Add(new  Il2CppSystem.Collections.Generic.KeyValuePair<string,CardData>(
                        data.title,data
                    ));
                    __instance.discovered.Add(data.name);
                }
            }
        }
    }
    
    public override void OnUpdate()
    {
        base.OnUpdate();
        if (
            ShouldInjectCards&& AddressableLoader.groups.ContainsKey("CardData"))
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
            
            StatusEffectDataAdditions = new ();
         
                     StatusEffectAdder.LaunchEvent();
                            for (int i = 0; i < StatusEffectDataAdditions.Count; i++)
                            {
                                var c = StatusEffectDataAdditions[i];
                                if (!AddressableLoader.groups["StatusEffectData"].lookup.ContainsKey(c.name))
                                {
                                    AddressableLoader.groups["StatusEffectData"].list.Add(c);
                                    AddressableLoader.groups["StatusEffectData"].lookup[c.name]=c;

                                }
                                Instance.LoggerInstance.Msg($"StatusEffect {c.name} is injected by api!");
                
                            }

                            CardUpgradeDataAdditions = new List<CardUpgradeData>();
                            CardUpgradeAdder.LaunchEvent();
                            for (int i = 0; i < CardUpgradeDataAdditions.Count; i++)
                            {
                                var c = CardUpgradeDataAdditions[i];
                                if (!AddressableLoader.groups["CardUpgradeData"].lookup.ContainsKey(c.name))
                                {
                                        AddressableLoader.groups["CardUpgradeData"].list.Add(c);
                                        AddressableLoader.groups["CardUpgradeData"].lookup[c.name]=c;
                                }
                                Instance.LoggerInstance.Msg($"CardUpgradeData {c.name} is injected by api!");
                
                            }
            CardDataAdditions = new List<CardData>();
            CardAdder.LaunchEvent();
            for (int i = 0; i < CardDataAdditions.Count; i++)
            {
                var c = CardDataAdditions[i];
                if (!AddressableLoader.groups["CardData"].lookup.ContainsKey(c.name))
                {
                    AddressableLoader.groups["CardData"].list.Add(c);
                    AddressableLoader.groups["CardData"].lookup.Add(c.name,c);
                }
                Instance.LoggerInstance.Msg($"Card {c.name} is injected by api!");

            }
            
            
        

            ShouldInjectCards = false;
        }
    }

   
    public override void OnInitializeMelon()
    {
        Instance = this;
        
        ClassInjector.RegisterTypeInIl2Cpp<CardAnimationProfile>();
        ClassInjector.RegisterTypeInIl2Cpp<BloodProfile>();
        ClassInjector.RegisterTypeInIl2Cpp<TargetMode>();
        ClassInjector.RegisterTypeInIl2Cpp<RewardPool>();
        
        AddDebugStuff();
        CardAdder.OnAskForAddingCards +=JSONApi.AddJSONCards;
        LoggerInstance.Msg(Color.Blue, "WildFrost API Loaded!");
        //MelonCoroutines.Start(LoadAssetsTestRoutine());

        base.OnInitializeMelon();
    }
}