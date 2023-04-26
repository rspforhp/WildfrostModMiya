using Il2Cpp;
using UnityEngine;

namespace WildfrostModMiya;

public static class CardAdder
{
    public static event Action<int> OnAskForAddingCards;

    internal  static void LaunchEvent()
    {
        OnAskForAddingCards?.Invoke(0);
    }
    public static Sprite ToSprite (this Texture2D t, Vector2? v = null)
    {
        var vector = v ?? new Vector2(.5f, .5f);
        return Sprite.Create(t, new Rect(0, 0, t.width, t.height), vector);
    }

    public static Sprite LoadSpriteFromCardPortraits(string name, Vector2? v = null)
    {
        Texture2D tex = new Texture2D(2, 2);
        tex.LoadImage(File.ReadAllBytes(WildFrostAPIMod.CardPortraitsFolder+(name.EndsWith(".png")?name: name+".png")));
        return tex.ToSprite(v);
    }



    public static CardData RegisterCardInApi(this CardData t)
    {
        t.SetCustomData("AddedByApi",true);
        t.original = t;
        WildFrostAPIMod.CardDataAdditions.Add(t);
        return t;
    }
    public static CardData SetName(this CardData t,string name)
    {
        t.titleFallback = name;
        t.forceTitle = name;
        return t;
    }

    public static CardData CreateCardData(string modName,string cardName)
    {
        var newData = ScriptableObject.CreateInstance<CardData>();
        newData.name = cardName.StartsWith(modName) ? cardName : $"{modName}.{cardName}";
        newData.titleFallback = newData.name;
        newData.forceTitle = newData.name;
        newData.cardType = AddressableLoader.GetGroup<CardType>("CardType").Find(delegate(CardType type)
        {
            return type.name == "Friendly";
        });
        newData.backgroundSprite =LoadSpriteFromCardPortraits("FALLBACKBACKGROUNDSPRITE.png");
        newData.mainSprite =LoadSpriteFromCardPortraits("FALLBACKMAINSPRITE.png");
        return newData;
    }
    
}