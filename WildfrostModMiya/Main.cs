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
using UnityEngine.Pool;
using UnityEngine.ResourceManagement.AsyncOperations;
using UniverseLib;
using WildfrostModMiya;
using ClassInjector = Il2CppInterop.Runtime.Injection.ClassInjector;
using Color = System.Drawing.Color;
using IEnumerator = System.Collections.IEnumerator;
using Il2CppType = Il2CppInterop.Runtime.Il2CppType;
using Object = Il2CppSystem.Object;

[assembly: MelonInfo(typeof(WildFrostAPIMod), "WildFrost API", "1", "Kopie_Miya")]
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


    public void AddDebugCards()
    {
        CardAdder.OnAskForAddingCards+= delegate(int i)
        {
            CardAdder.CreateCardData("API","DebugCard")
                .SetTitle("Debug Card")
                .SetIsItem()
                //.AddToPool(CardAdder.VanillaRewardPools.BasicItemPool) debug cards shouldn't be in pools
                .SetCanPlay(CardAdder.CanPlay.CanPlayOnEnemy | CardAdder.CanPlay.CanPlayOnBoard)
                .SetSprites("CardPortraits\\testPortrait","CardPortraits\\testBackground")
                .SetDamage(2)
                .SetBloodProfile(CardAdder.VanillaBloodProfiles.BloodProfilePinkWisp)
                .SetIdleAnimationProfile(CardAdder.VanillaCardAnimationProfiles.GoopAnimationProfile)
                .SetTraits(CardAdder.VanillaTraits.Barrage.TraitStack(1))
                .SetStartWithEffects(CardAdder.VanillaStatusEffects.IncreaseEffects.StatusEffectStack(1))
                .SetAttackEffects(CardAdder.VanillaStatusEffects.Demonize.StatusEffectStack(1))
                .RegisterCardInApi();

        };
    }
    private bool MatchCardName(Object o,string name)
    {
        var card = o.Cast<CardData>();
        return card.name.Equals(name, StringComparison.OrdinalIgnoreCase) || card.title.Equals(name, StringComparison.OrdinalIgnoreCase) ||
               card.forceTitle.Equals(name, StringComparison.OrdinalIgnoreCase);
    }
    private string CardName;

    private void DIRTY_DebugGui()
    {
        CardName = GUILayout.TextField(CardName);
        
        if (GUILayout.Button("Give me!") && !string.IsNullOrEmpty(CardName))
        {
            CardData card = null;
            foreach (var cardinList in AddressableLoader.groups["CardData"].list)
                if (MatchCardName(cardinList,CardName))
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
    
    public override void OnUpdate()
    {
        base.OnUpdate();
        if (
            ShouldInjectCards&& AddressableLoader.groups.ContainsKey("CardData"))
        {
            CoroutineManager.Start(AddressableLoader.LoadGroup("StatusEffectData"));
            if (!AddressableLoader.IsGroupLoaded("StatusEffectData")) return;

            CoroutineManager.Start(AddressableLoader.LoadGroup("TraitData"));
            if (!AddressableLoader.IsGroupLoaded("TraitData")) return;

            
            CreateVanillaAnimationProfiles();
            if (VanillaAnimationProfiles.Count == 0) return;
            
            CreateVanillaBloodProfiles();
            if (VanillaBloodProfiles.Count == 0) return;

            CreateVanillaTargetModes();
            if (VanillaTargetModes.Count == 0) return;
            
            CardAdder.LaunchEvent();
            for (int i = 0; i < CardDataAdditions.Count; i++)
            {
                var c = CardDataAdditions[i];
                AddressableLoader.groups["CardData"].lookup[c.name]=c;
                AddressableLoader.groups["CardData"].list.Add(c);
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
        
        AddDebugCards();
        CardAdder.OnAskForAddingCards +=JSONApi.AddJSONCards;
        LoggerInstance.Msg(Color.Blue, "WildFrost API Loaded!");
        //MelonCoroutines.Start(LoadAssetsTestRoutine());
        base.OnInitializeMelon();
    }
}