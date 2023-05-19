using UnityEngine;
using UnityEngine.Localization;

namespace WildfrostModMiya;

public static class StatusEffectAdder
{
    public static event Action<int> OnAskForAddingStatusEffects;

    public static T ModifyFields<T>(this T t, Func<T,T> modifyFields) where T : StatusEffectData
    {
        t = modifyFields(t);
        return t;
    }
    
    public static T SetText<T>(this T t, string text) where T : StatusEffectData
    {
        t.textKey= LocalizationHelper.FromId(LocalizationHelper.CreateLocalizedString(t.name+".Text",text));
        return t;
    }


    public static T RegisterStatusEffectInApi<T>(this T t) where T : StatusEffectData
    {
        WildFrostAPIMod.StatusEffectDataAdditions.Add(t);
        return t;
    }
    

    public static T CreateStatusEffectData<T>(string modName,string cardName) where T : StatusEffectData
    {
        var newData = ScriptableObject.CreateInstance<T>();
        newData.textKey = new LocalizedString();
        newData.name = cardName.StartsWith(modName) ? cardName : $"{modName}.{cardName}";
        if (modName == "") newData.name = cardName;
        return newData;
    }

    
    
    internal static void LaunchEvent()
    {
        OnAskForAddingStatusEffects?.Invoke(0);
    }
}