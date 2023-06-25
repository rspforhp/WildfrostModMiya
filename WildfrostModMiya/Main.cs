using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
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
using BepInEx.Unity.IL2CPP.Hook;
using BepInEx.Unity.IL2CPP.Utils;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using Cpp2IL.Core.Attributes;
using Il2CppInterop.Common;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using TinyJson;
using UnityEngine.ResourceManagement;
using UnityEngine.ResourceManagement.Exceptions;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;
using WildfrostModMiya;
using ClassInjector = Il2CppInterop.Runtime.Injection.ClassInjector;
using Color = System.Drawing.Color;
using Il2CppType = Il2CppInterop.Runtime.Il2CppType;
using MethodInfo = System.Reflection.MethodInfo;
using Object = Il2CppSystem.Object;
using Random = System.Random;
using BepInEx.IL2CPP;
using Il2CppInterop.Runtime.InteropTypes;
using WildfrostModMiya.Hook.Dobby;
using WildfrostModMiya.Hook.Funchook;
using Assembly = System.Reflection.Assembly;
using BindingFlags = System.Reflection.BindingFlags;
using IDetour = Il2CppInterop.Runtime.Injection.IDetour;
using MethodBase = System.Reflection.MethodBase;

namespace WildfrostModMiya;

[BepInPlugin("WildFrost.Miya.WildfrostAPI", "WildfrostAPI", "0.2.0.0")]
public class WildFrostAPIMod : BasePlugin
{
    public static string ModsFolder = typeof(WildFrostAPIMod).Assembly.Location.Replace("WildfrostModMiya.dll", "");
    public static WildFrostAPIMod Instance;


    internal static Dictionary<string, List<UnityEngine.Object>>
        GroupAdditions = new Dictionary<string, List<UnityEngine.Object>>()
        {
            { "CardData", new List<UnityEngine.Object>() },
            { "StatusEffectData", new List<UnityEngine.Object>() },
            { "CardUpgradeData", new List<UnityEngine.Object>() }
        };

    internal static Dictionary<string, Action<int>>
        EventsCallers = new Dictionary<string, Action<int>>()
        {
            { "CardData", delegate(int i) { CardAdder.LaunchEvent(); } },
            { "StatusEffectData", delegate(int i) { StatusEffectAdder.LaunchEvent(); } },
            { "CardUpgradeData", delegate(int i) { CardUpgradeAdder.LaunchEvent(); } }
        };

/*

    [HarmonyPatch(typeof(Addressables))]
    class LoadAssetsAsyncPatch
    {
        private static MethodInfo Info;

        [HarmonyTargetMethods]
        static IEnumerable<MethodInfo> GetAllMethods()
        {
            var a = Array.ConvertAll(
                    AccessTools.GetDeclaredMethods(typeof(Addressables)).FindAll(m => m.Name == "LoadAssetsAsync")
                        .ToArray(),
                    delegate(MethodInfo info) { return info.MakeGenericMethod(typeof(UnityEngine.Object)); }).ToList()
                .FindAll(delegate(MethodInfo info)
                {
                    if (info.ReturnType ==
                        typeof(UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<
                            Il2CppSystem.Collections.Generic.IList<UnityEngine.Object>>) &&
                        info.GetParameters()[0].ParameterType == typeof(Il2CppSystem.Object) &&
                        info.GetParameters()[1].ParameterType == typeof(Il2CppSystem.Action<UnityEngine.Object>))
                        return true;
                    return false;
                }).AsEnumerable();
            Info = a.ToArray()[0];
            return a;
        }

        [HarmonyPostfix]
        static void Postfix(
            UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<
                Il2CppSystem.Collections.Generic.IList<UnityEngine.Object>> __result, Il2CppSystem.Object __0,
            Il2CppSystem.Action<UnityEngine.Object> __1)
        {
            Instance.Log.LogInfo(__0.ToString());
            //Debug.Log("Load all assets patch");
        }
    }


    [HarmonyPatch(typeof(ResourceManager), nameof(ResourceManager.ProvideResourceGroupCached),new []{typeof(Il2CppSystem.Collections.Generic.IList<IResourceLocation>), typeof(int),typeof(Il2CppSystem.Type),typeof(Il2CppSystem.Action<AsyncOperationHandle>),typeof(bool)})]
    class GetResourceProviderPatch
    {

        public static System.Collections.IEnumerator Test(AsyncOperationHandle<Il2CppSystem.Collections.Generic.IList<AsyncOperationHandle>>   __result)
        {
           __result = __result.MemberwiseClone().Cast<AsyncOperationHandle<Il2CppSystem.Collections.Generic.IList<AsyncOperationHandle>>>();
            var e = __result.WaitForCompletion();
            foreach (var thing in e.Cast<Il2CppSystem.Collections.Generic.List<AsyncOperationHandle>>())
            {
                Debug.Log(thing+" test");
                Debug.Log(thing.WaitForCompletion().Cast<AssetBundleResource>()?.+" test2");
            }
            yield break ;
            
        }
       
        [HarmonyPostfix]
        static void Postfix(Il2CppSystem.Collections.Generic.IList<IResourceLocation> locations, int groupHash, Il2CppSystem.Type desiredType, Il2CppSystem.Action<AsyncOperationHandle> callback, bool releaseDependenciesOnFailure,ref ResourceManager __instance, ref AsyncOperationHandle<Il2CppSystem.Collections.Generic.IList<AsyncOperationHandle>>   __result)
        {
            Debug.Log("test "+__result.LocationName+" ");
            APIGameObject.instance?.StartCoroutine(Test(__result));
      
        }
    }


*/

    private sealed class
        MethodInfoStoreGeneric_ProvideResources_Public_AsyncOperationHandle_1_IList_1_TObject_IList_1_IResourceLocation_Boolean_Action_1_TObject_0<
            TObject>
    {
        internal static System.IntPtr Pointer = IL2CPP.il2cpp_method_get_from_reflection(
            IL2CPP.Il2CppObjectBaseToPtrNotNull(
                (Il2CppObjectBase)new Il2CppSystem.Reflection.MethodInfo(IL2CPP.il2cpp_method_get_object(
                    IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<ResourceManager>.NativeClassPtr, 100663406),
                    Il2CppClassPointerStore<ResourceManager>.NativeClassPtr)).MakeGenericMethod(
                    new Il2CppReferenceArray<Il2CppSystem.Type>(new Il2CppSystem.Type[1]
                    {
                        Il2CppSystem.Type.internal_from_handle(
                            IL2CPP.il2cpp_class_get_type(Il2CppClassPointerStore<TObject>.NativeClassPtr))
                    }))));
    }


    private unsafe static void OurAttackCheck(IntPtr t, IntPtr items)
    {
        var group = new AddressableLoader.Group(t);

        group.list = new Il2CppSystem.Collections.Generic.List<UnityEngine.Object>();
        group.lookup = new Il2CppSystem.Collections.Generic.Dictionary<string, UnityEngine.Object>();
        Il2CppSystem.Collections.Generic.IEnumerable<UnityEngine.Object> data =
            new Il2CppSystem.Collections.Generic.IEnumerable<UnityEngine.Object>(items);
        foreach (UnityEngine.Object @object in data.ToArray())
        {
            group.list.Add(@object);
            group.lookup[AddressableLoader.Group.GetName(@object)] = @object;
        }

        var typeName = data.ToArray()[0].GetIl2CppType().ToString();
        if (EventsCallers.ContainsKey(typeName))
        {
            EventsCallers[typeName].Invoke(0);
        }

        Instance.Log.LogInfo(typeName + " is the typename");
        if (GroupAdditions.ContainsKey(typeName))
        {
            foreach (UnityEngine.Object @object in GroupAdditions[typeName].ToArray())
            {
                Instance.Log.LogInfo("Added a " + typeName + " " + @object.name);
                group.list.Add(@object);
                group.lookup[AddressableLoader.Group.GetName(@object)] = @object;
            }
        }
    }

    private static AttackCheckDelegate Test = (AttackCheckDelegate)OurAttackCheck;
    private static AttackCheckDelegate OriginalHammerMethod;

    //Il2CppSystem.Collections.Generic.IEnumerable<UnityEngine.Object> data
    private unsafe delegate void AttackCheckDelegate(IntPtr t, IntPtr items);

 



    /*
    [HarmonyPatch(typeof(AddressableLoader))]
    private static class LoadAssetsAsyncPatch
    {
        private static Type[] TypesArray = new[] {typeof(CardData)};
        [HarmonyTargetMethods]
        private static IEnumerable<MethodInfo> GetAllMethods()
        {
            return new List<MethodInfo>(){ AccessTools.Method(typeof(AddressableLoader), "StoreGroup").MakeGenericMethod(typeof(CardData))}.AsEnumerable(); ;
        }

        [HarmonyPostfix]
        private static void Postfix(ref string name)
        {
            Debug.Log("Load all assets patch "+ name);
        }
    }
*/


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


    private static System.Collections.IEnumerator DIRTY_ConsoleStuff()
    {
        yield return new WaitUntil((Func<bool>)(() => SceneManager.IsLoaded("Console")));
        var go = UnityEngine.Object.FindObjectOfType<Console>();
        go?.Toggle();
    }

    private static int seed = -1;

    [HarmonyPatch(typeof(CampaignData), nameof(CampaignData.Init))]
    class Patch
    {
        [HarmonyPostfix]
        static void Postfix(CampaignData __instance)
        {
            if (Instance.configSeedManipulation.Value && seed != -1)
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
            var oldSeed = seed;
            seed = int.Parse(GUILayout.TextField(seed.ToString()));
            if (GUILayout.Button("Randomize Seed"))
            {
                seed = Dead.Random.Seed();
            }

            if (oldSeed != seed)
            {
                var o = UnityEngine.Object.FindObjectOfType<SelectLeader>();
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
                    clone.id = (ulong)UnityEngine.Object.FindObjectsOfType<CardData>().Count * 10;
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
            if (_VanillaAnimationProfiles == null)
            {
                CreateVanillaAnimationProfiles();
            }

            return _VanillaAnimationProfiles;
        }
    }

    internal static List<CardAnimationProfile> _VanillaAnimationProfiles;

    private static void CreateVanillaAnimationProfiles()
    {
        _VanillaAnimationProfiles = new List<CardAnimationProfile>();
        var list = UnityEngine.Object.FindObjectsOfTypeIncludingAssets(Il2CppType.Of<CardAnimationProfile>());
        foreach (var profile in list)
        {
            _VanillaAnimationProfiles.Add(profile.Cast<CardAnimationProfile>());
        }
    }

    internal static List<BloodProfile> VanillaBloodProfiles
    {
        get
        {
            if (_VanillaBloodProfiles == null)
            {
                CreateVanillaBloodProfiles();
            }

            return _VanillaBloodProfiles;
        }
    }

    internal static List<BloodProfile> _VanillaBloodProfiles;

    private static void CreateVanillaBloodProfiles()
    {
        _VanillaBloodProfiles = new List<BloodProfile>();
        var list = UnityEngine.Object.FindObjectsOfTypeIncludingAssets(Il2CppType.Of<BloodProfile>());
        foreach (var profile in list)
        {
            _VanillaBloodProfiles.Add(profile.Cast<BloodProfile>());
        }
    }

    internal static List<TargetMode> VanillaTargetModes
    {
        get
        {
            if (_VanillaTargetModes == null)
            {
                CreateVanillaTargetModes();
            }

            return _VanillaTargetModes;
        }
    }

    internal static List<TargetMode> _VanillaTargetModes;


    private static void CreateVanillaTargetModes()
    {
        _VanillaTargetModes = new List<TargetMode>();
        var list = UnityEngine.Object.FindObjectsOfTypeIncludingAssets(Il2CppType.Of<TargetMode>());
        foreach (var profile in list)
        {
            _VanillaTargetModes.Add(profile.Cast<TargetMode>());
        }
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


    public unsafe override void Load()
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


       

        IntPtr test =
            *(IntPtr*)(IntPtr)IL2CPP.GetIl2CppMethodByToken(
                Il2CppClassPointerStore<AddressableLoader.Group>.NativeClassPtr, 100664621);
        new DobbyDetour(test, Test).Apply();
        //      OriginalHammerMethod = detour.GenerateTrampoline((MethodBase) typeof (AttackCheckDelegate).GetMethod("Invoke"));


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