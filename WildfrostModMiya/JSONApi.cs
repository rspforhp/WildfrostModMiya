using Object = Il2CppSystem.Object;
using TinyJson;

namespace WildfrostModMiya;

public static class JSONApi
{
    public class JSONCardUpdateData
    {
        public string name;
    }

    public class JSONStatusEffectData
    {
        public string name;
        public int count;
    }

    public class JSONTraitData
    {
        public string name;
        public int count;
    }

    // public static string CardPortraitsFolder =WildFrostAPIMod.ModsFolder + "JsonCards\\";
    internal static void AddJSONCards(int unused)
    {
        var debugJsonCard = new JSONCardData();
        {
            debugJsonCard.portraitPath = "CardPortraits\\FALLBACKMAINSPRITE";
            debugJsonCard.backgroundPath = "CardPortraits\\FALLBACKBACKGROUNDSPRITE";
            debugJsonCard.name = "API.TemplateCard";
            debugJsonCard.title = "Template Card";
            debugJsonCard.hp = 1;
            debugJsonCard.hasHealth = true;
            debugJsonCard.damage = 1;
            debugJsonCard.hasAttack = true;
            debugJsonCard.counter = 1;
            debugJsonCard.upgrades = new JSONCardUpdateData[1] { new() { name = "Card Upgrade name" } };
            debugJsonCard.attackEffects = new JSONStatusEffectData[1]
                { new() { name = "Status effect name", count = 1 } };
            debugJsonCard.startWithEffects = new JSONStatusEffectData[1]
                { new() { name = "Status effect name", count = 1 } };
            debugJsonCard.traits = new JSONTraitData[1] { new() { name = "Trait name", count = 1 } };
            debugJsonCard.customData = new Dictionary<string, object> { ["SomeData"] = true };
            debugJsonCard.pools = new string[1] { "BasicItemPool" };
            debugJsonCard.bloodProfile = "Blood Profile Normal";
            debugJsonCard.idleAnimation = "SwayAnimationProfile";
            debugJsonCard.CanPlayOnBoard = true;
            debugJsonCard.CanPlayOnEnemy = true;
            debugJsonCard.CanPlayOnFriendly = true;
            debugJsonCard.CanPlayOnHand = true;
            debugJsonCard.IsItem = true;
            debugJsonCard.CardType ="Item";
            debugJsonCard.IsPet = false;
        }
        var debugJson = debugJsonCard.ToJson();
        File.WriteAllText(WildFrostAPIMod.ModsFolder + "Template.json", debugJson);
        foreach (var jsonFile in Directory.EnumerateFiles(WildFrostAPIMod.ModsFolder, "*.json",
                     SearchOption.AllDirectories))
        {
            if (jsonFile.EndsWith("Template.json")|| jsonFile.EndsWith("manifest.json")) continue;
            var text = File.ReadAllText(jsonFile);
            JSONCardData test=text.FromJson<JSONCardData>();
            WildFrostAPIMod.Instance.Log.LogInfo(test.name);

            var attackEffects = new Il2CppSystem.Collections.Generic.List<CardData.StatusEffectStacks>();
            foreach (var effectData in test.attackEffects)
            {
                var data = new CardData.StatusEffectStacks
                {
                    count = effectData.count,
                    data =effectData.name.StatusEffectData()
                };
                WildFrostAPIMod.Instance.Log.LogInfo(effectData.name + " " + effectData.count +
                                                            " added to json attack effects");
                attackEffects.Add(data);
            }

            var startWithEffects = new Il2CppSystem.Collections.Generic.List<CardData.StatusEffectStacks>();
            foreach (var effectData in test.startWithEffects)
            {
                var data = new CardData.StatusEffectStacks
                {
                    count = effectData.count,
                    data = effectData.name.StatusEffectData()
                };
                startWithEffects.Add(data);
            }

            var cardUpgrades = new Il2CppSystem.Collections.Generic.List<CardUpgradeData>();
            foreach (var effectData in test.upgrades)
            {
                var data = AddressableLoader.groups["CardUpgradeData"].lookup[effectData.name]
                    .Cast<CardUpgradeData>();
                cardUpgrades.Add(data);
            }

            var cardTraits = new Il2CppSystem.Collections.Generic.List<CardData.TraitStacks>();
            foreach (var effectData in test.traits)
            {
                var data = new CardData.TraitStacks
                {
                    count = effectData.count,
                    data =effectData.name.TraitData()
                };
                cardTraits.Add(data);
            }

            var card = CardAdder.CreateCardData("", test.name)
                    .SetStats(test.hasHealth ? test.hp : null, test.hasAttack ? test.damage : null, test.counter)
                    .SetSprites(test.portraitPath, test.backgroundPath)
                    .SetTitle(test.title)
                    .SetAttackEffects(attackEffects.ToArray())
                    .SetStartWithEffects(startWithEffects.ToArray())
                    .SetUpgrades(cardUpgrades)
                    .SetTraits(cardTraits.ToArray())
                    .AddToPool(test.pools)
                    .SetBloodProfile(test.bloodProfile)
                    .SetIdleAnimationProfile(test.idleAnimation)
                ;
            if(test.customData!=null)
               foreach (var pair in test.customData) card.SetCustomData(pair.Key, (Object)pair.Value);

            card.canPlayOnBoard = test.CanPlayOnBoard;
            card.canPlayOnEnemy = test.CanPlayOnEnemy;
            card.canPlayOnFriendly = test.CanPlayOnFriendly;
            card.canPlayOnHand = test.CanPlayOnHand;
            if (test.IsItem) card.SetIsItem();
            else card.SetIsUnit();
            card.SetCardType(test.CardType);
            if (test.IsPet) card.AddToPets();
            card.RegisterCardInApi();
        }
    }

    public class JSONCardData
    {
        public string portraitPath;
        public string backgroundPath;
        public string name;
        public string title;
        public int hp;
        public bool hasHealth;
        public int damage;
        public bool hasAttack;
        public int counter;
        public JSONCardUpdateData[] upgrades;
        public JSONStatusEffectData[] attackEffects;
        public JSONStatusEffectData[] startWithEffects;
        public JSONTraitData[] traits;
        public Dictionary<string, object> customData;
        public string[] pools;
        public string bloodProfile;
        public string idleAnimation;
        public string CardType;
        public bool IsItem;

        public bool CanPlayOnBoard;
        public bool CanPlayOnEnemy;
        public bool CanPlayOnFriendly;
        public bool CanPlayOnHand;
        public bool IsPet;
    }
}