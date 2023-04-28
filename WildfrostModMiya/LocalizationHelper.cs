using UnityEngine.Localization;

namespace WildfrostModMiya;

public static class LocalizationHelper
{
    
    public static LocalizedString CreateLocalString()
    {
        LocalizedString @string = new LocalizedString();
        return @string;
    }

}