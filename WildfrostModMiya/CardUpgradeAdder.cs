using Il2CppInterop.Runtime;
using UnityEngine;
using UnityEngine.Localization;
using Object = UnityEngine.Object;

namespace WildfrostModMiya;

public static class CardUpgradeAdder
{
    public static event Action<int> OnAskForAddingCardUpgrades;

    public static CardUpgradeData ModifyFields(this CardUpgradeData t, Func<CardUpgradeData,CardUpgradeData> modifyFields) 
    {
        t = modifyFields(t);
        return t;
    }
    
    public static CardUpgradeData SetText(this CardUpgradeData t, string text) 
    {
        t.textKey= LocalizationHelper.FromId(LocalizationHelper.CreateLocalizedString(t.name+".Text",text));
        return t;
    }

    public static CardUpgradeData SetTitle(this CardUpgradeData t, string title) 
    {
        t.titleKey= LocalizationHelper.FromId(LocalizationHelper.CreateLocalizedString(t.name+".Title",title));
        return t;
    }
    public static CardUpgradeData SetUpgradeType(this CardUpgradeData t, CardUpgradeData.Type type)
    {
        t.type = type;
        return t;
    }
    public static CardUpgradeData SetImage(this CardUpgradeData t, string sprite)
    {
        t.image = CardAdder.LoadSpriteFromCardPortraits(sprite);
        return t;
    }
    public static CardUpgradeData SetAttackEffects(this CardUpgradeData t, params CardData.StatusEffectStacks[] effect)
    {
        t.attackEffects = effect;
        return t;
    }
    public static CardUpgradeData SetStartWithEffects(this CardUpgradeData t, params CardData.StatusEffectStacks[] effect)
    {
        t.startWithEffectsApplied = new Il2CppSystem.Collections.Generic.List<CardData.StatusEffectStacks>();
        foreach (var e in effect)
        {
            t.startWithEffectsApplied.Add(e);
        }
        return t;
    }

    public static CardUpgradeData SetTraits(this CardUpgradeData t,  params CardData.TraitStacks[] traits)
    {
        var list = new Il2CppSystem.Collections.Generic.List<CardData.TraitStacks>();
        foreach (var trait in traits)
        {
            list.Add(trait);
        }
        t.traitsAffected = list;
        return t;
    }
    public static CardUpgradeData RegisterCardUpgradeData(this CardUpgradeData t)
    {
        WildFrostAPIMod.GroupAdditions["CardUpgradeData"].Add(t);
        return t;
    }
    
    public static CardUpgradeData AddToPool(this CardUpgradeData t, params CardAdder.VanillaRewardPools[] rewardPools)
    {
        List<string> names = new();
        foreach (var p in rewardPools)
        {
            names.Add(p.ToString().Replace("VanillaRewardPools.", ""));
        }

        t = t.AddToPool(names.ToArray());
        return t;
    }

    public static CardUpgradeData AddToPool(this CardUpgradeData t, params string[] rewardPools)
    {
        var allPools = UnityEngine.Object.FindObjectsOfTypeIncludingAssets(Il2CppType.Of<RewardPool>());
        foreach (var poolName in rewardPools)
        {
            var pool = allPools.ToList().Find(a => a.name == poolName).Cast<RewardPool>();
            pool?.list?.Add(t);
        }

        return t;
    }
    

    public static  CardUpgradeData CreateCardUpgradeData(string modName,string cardName)
    {
        var newData = ScriptableObject.CreateInstance<CardUpgradeData>();
        newData.textKey = new LocalizedString();
        newData.name = cardName.StartsWith(modName) ? cardName : $"{modName}.{cardName}";
        if (modName == "") newData.name = cardName;
        return newData;
    }

    
    
    internal static void LaunchEvent()
    {
        if(WildFrostAPIMod.GroupAdditions["CardUpgradeData"].Count<=0)
            OnAskForAddingCardUpgrades?.Invoke(0);
    }
}