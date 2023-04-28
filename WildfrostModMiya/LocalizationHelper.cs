using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;

namespace WildfrostModMiya;

public static class LocalizationHelper
{
    public static LocalizedString FromId(long id)
    {
        return new LocalizedString(TableReference.TableReferenceFromString("Card Text"),
            new TableEntryReference() { KeyId = id,  ReferenceType = TableEntryReference.Type.Id});
    }
    

    public static long CreateLocalizedString(string key, string localized)
    {
        var test = LocalizationSettings.StringDatabase.GetTable(TableReference.TableReferenceFromString("Card Text"));
        var entry=test.AddEntry(key, localized);
        //WildFrostAPIMod.Instance.LoggerInstance.Msg($"CreateLocalizedString({key}, {localized}) returns {entry.KeyId}");
        return entry.KeyId;
    }

}