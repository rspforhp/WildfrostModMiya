using HarmonyLib;
using Il2Cpp;
using MelonLoader;
using UnityEngine;
using UnhollowerBaseLib;
using UnhollowerBaseLib.Runtime;
using UnityEngine.Localization;
using UnityEngine.Localization.Tables;
using UnityEngine.ResourceManagement.AsyncOperations;
using WildfrostModMiya;
using Object = UnityEngine.Object; // The namespace of your mod class
// ...
[assembly: MelonInfo(typeof(Mod), "WildFrost Mod", "1", "Kopie_Miya")]
[assembly: MelonGame("Deadpan Games", "Wildfrost")]

namespace WildfrostModMiya
{
    public static class WildFrostAPI
    {
        public static event Func<List<CardBuilder>, List<CardBuilder>> AddCards; 
        public static List<string> AddedCards;

        public class CardBuilder
        {
            public CardBuilder(string fallbackCard="Naked Gnome")
            {
                CardData woodheadCard = null;
                foreach (var cardO in AddressableLoader.groups["CardData"].list)
                {
                    var card = cardO.Cast<CardData>();
                    if (card.title.Equals(fallbackCard))
                    {
                        woodheadCard = card;
                        break;
                    }
                }
                data = woodheadCard;
                //data.original = data;
            }
            public CardData data;

            public CardBuilder SetTitle(string title = "")
            {
                data.name = title;
                data.titleFallback = title;
                data.forceTitle = title;
                return this;
            }
            public CardBuilder SetFlavour(string flavour="")
            {
                data.flavour = flavour;
                data.flavourKey = new LocalizedString(){ };
                return this;
            }
            public CardBuilder SetIsItem(bool value = false,int uses=1)
            {
                if (value) data.playType = Card.PlayType.Play;
                else data.playType = Card.PlayType.Place;
                if (data.IsItem)
                {
                    data.uses =uses;
                    data.canBeHit = false;
                    data.cardType = AddressableLoader.groups["CardType"].lookup["Item"].Cast<CardType>();
                }
                return this;
            }
            public CardBuilder SetStats(int health=-1,int damage=-1,int counter=0)
            {
                data.hp = health == -1 ? 0 : health;
                data.damage = damage == -1 ? 0 : damage;
                data.counter = counter;
                data.hasHealth = health != -1;
                data.hasAttack = damage != -1;
                return this;
            }
        }
        internal static void InvokeEvents(ref List<CardBuilder> cardBuilders)
        {
            if (AddCards != null) cardBuilders = AddCards(cardBuilders);
        }
        //TODO: For some reason it crashes after a save, look at it!
        internal static void AddAllCards(List<CardBuilder> cardBuilders)
        {
            foreach (var builder in cardBuilders)
            {
                var card = builder.data;
                Mod.Instance.LoggerInstance.Warning($"Added card {card.forceTitle}!");
                AddressableLoader.groups["CardData"].list.Add(card);
            }
        }
    }
    public class Mod : MelonMod
    {
        internal static Mod Instance;
        public override void OnInitializeMelon()
        {
            Instance = this;
            LoggerInstance.Msg("Hi from miyas mod!");
            base.OnInitializeMelon();

            
            WildFrostAPI.AddCards+= delegate(List<WildFrostAPI.CardBuilder> list)
            {
                list.Add(new WildFrostAPI.CardBuilder().SetTitle("NewCard").SetFlavour("Maybe it's a bit useful...").SetStats(-1,1,0).SetIsItem(true));
                
                return list;
            };

        }


        public override void OnUpdate()
        {
           
            base.OnUpdate();
        }

        public static bool CardsAdded = false;

        public static CardData debug = new CardData() { forceTitle = "APIADDDED" };

        [HarmonyPatch(typeof(AddressableLoader), nameof(AddressableLoader.IsGroupLoaded))]
        class namePatch
        {
            [HarmonyPostfix]
            static void Postfix(string name,bool __result)
            {
                if (name == "CardData"&& !AddressableLoader.groups["CardData"].list.Contains(debug))
                {
                    AddressableLoader.groups["CardData"].list.Add(debug);
                    List<WildFrostAPI.CardBuilder> builders = new List<WildFrostAPI.CardBuilder>();
                    WildFrostAPI.InvokeEvents(ref builders);
                    WildfrostModMiya.WildFrostAPI.AddAllCards(builders);
                }
            }
        }
        
        [HarmonyPatch(typeof(MetaprogressionSystem), nameof(MetaprogressionSystem.IsUnlocked),
            new[] { typeof(string), typeof(Il2CppSystem.Collections.Generic.List<string>) })]
        class IsUnlocked1
        {
            [HarmonyPostfix]
            static void Postfix(MetaprogressionSystem __instance, ref bool __result)
            {
                if (UnlockAll)

                    __result = UnlockAll;
            }
        }
        [HarmonyPatch(typeof(MetaprogressionSystem), nameof(MetaprogressionSystem.IsUnlocked),
            new[] { typeof(UnlockData), typeof(Il2CppSystem.Collections.Generic.List<string>) })]
        class IsUnlocked2
        {
            [HarmonyPostfix]
            static void Postfix(MetaprogressionSystem __instance, ref bool __result)
            {
                if (UnlockAll)

                    __result = UnlockAll;
            }
        }


        [HarmonyPatch(typeof(MetaprogressionSystem), nameof(MetaprogressionSystem.GetUnlockedList))]
        class Unlockall
        {
            [HarmonyPostfix]
            static void Postfix(MetaprogressionSystem __instance,
                ref Il2CppSystem.Collections.Generic.List<string> __result)
            {
                if (UnlockAll)
                {
                    foreach (var pet in MetaprogressionSystem.data["pets"]
                                 .Cast<Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStringArray>())
                    {
                        __result.Add(pet);
                    }
                }

                ;
            }
        }

        public static bool UnlockAll;

        public bool MatchCardName(Object o)
        {
            var card = o.Cast<CardData>();
            LoggerInstance.Warning(card.title);
            return card.title.Equals(CardName, StringComparison.OrdinalIgnoreCase) ||  card.forceTitle.Equals(CardName, StringComparison.OrdinalIgnoreCase);
        }

        public Rect _rect = new Rect(0, 0, 175, 300);
        public string CardName;

        public override void OnGUI()
        {
            var e = Event.current;
            if (e.type == EventType.MouseDrag && _rect.Contains(e.mousePosition))
            {
                _rect.x += e.delta.x;
                _rect.y += e.delta.y;
            }

            GUI.BeginGroup(_rect, "");
            GUILayout.Label("Enter the card name: ");
            CardName = GUILayout.TextField(CardName);
            if (GUILayout.Button("Give me!") && !string.IsNullOrEmpty(CardName))
            {
                CardData card = null;
                foreach (var cardinList in AddressableLoader.groups["CardData"].list)
                {
                    if (MatchCardName(cardinList))
                    {
                        card = cardinList.Cast<CardData>();
                        break;
                    }
                }

                if (card != null)
                {
                    LoggerInstance.Msg("Gave out card " + card.title);
                    Campaign.instance.characters._items[0].data.inventory.deck.Add(card.Clone());
                    Campaign.instance.characters._items[0].data.inventory.Save();
                }
                else LoggerInstance.Msg("No such card!");
            }

            UnlockAll = GUILayout.Toggle(UnlockAll, "Unlock all?");
           

            GUI.EndGroup();
            base.OnGUI();
        }
    }
}