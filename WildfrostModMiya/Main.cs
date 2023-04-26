using HarmonyLib;
using Il2Cpp;
using Il2CppSystem.Collections;
using Il2CppSystem.IO;
using Il2CppSystem.Reflection;
using MelonLoader;
using UnhollowerBaseLib;
using UnhollowerRuntimeLib;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Pool;
using WildfrostModMiya;
using Color = System.Drawing.Color;
using IEnumerator = System.Collections.IEnumerator;
using Object = Il2CppSystem.Object;

[assembly: MelonInfo(typeof(WildFrostAPIMod), "WildFrost API", "1", "Kopie_Miya")]
[assembly: MelonGame("Deadpan Games", "Wildfrost")]

namespace WildfrostModMiya;

public partial class WildFrostAPIMod : MelonMod
{
    public static string ModsFolder = typeof(WildFrostAPIMod).Assembly.Location.Replace("WildfrostModMiya.dll", "");
    public static string CardPortraitsFolder = ModsFolder + "CardPortraits\\";
    public static string JsonCardsFolder = ModsFolder + "JsonCards\\";
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

    public static bool ShouldInjectCards;
    internal static  List<CardData> CardDataAdditions=new List<CardData>();

    public void AddDebugCards()
    {
        CardAdder.OnAskForAddingCards+= delegate(int i)
        {
            CardAdder.CreateCardData("API","DebugCard").SetName("Custom Name").RegisterCardInApi();

        };
    }
    private bool MatchCardName(Object o)
    {
        var card = o.Cast<CardData>();
        LoggerInstance.Warning(card.title);
        return card.name.Equals(CardName, StringComparison.OrdinalIgnoreCase) || card.title.Equals(CardName, StringComparison.OrdinalIgnoreCase) ||
               card.forceTitle.Equals(CardName, StringComparison.OrdinalIgnoreCase);
    }
    private string CardName;

    private void DIRTY_DebugGui()
    {
        CardName = GUILayout.TextField(CardName);
        if (GUILayout.Button("Give me!") && !string.IsNullOrEmpty(CardName))
        {
            CardData card = null;
            foreach (var cardinList in AddressableLoader.groups["CardData"].list)
                if (MatchCardName(cardinList))
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
      DIRTY_DebugGui();
        base.OnGUI();
    }

    public override void OnUpdate()
    {
        base.OnUpdate();
        if (ShouldInjectCards&& AddressableLoader.groups.ContainsKey("CardData"))
        {
            CardAdder.LaunchEvent();
            for (int i = 0; i < CardDataAdditions.Count; i++)
            {
                var c = CardDataAdditions[i];
                Instance.LoggerInstance.Msg($"Before Card {c.name} is injected by api!");
                AddressableLoader.groups["CardData"].list.Add(c);
                Instance.LoggerInstance.Msg($"Card {c.name} is injected by api!");

            }

            ShouldInjectCards = false;
        }
    }

    public override void OnInitializeMelon()
    {
        Instance = this;
        
    
        AddDebugCards();
        LoggerInstance.Msg(Color.Blue, "WildFrost API Loaded!");
        //MelonCoroutines.Start(LoadAssetsTestRoutine());
        base.OnInitializeMelon();
    }
}