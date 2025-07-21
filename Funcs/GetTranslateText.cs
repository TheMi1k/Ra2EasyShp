using Ra2EasyShp.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ra2EasyShp.Funcs
{
    internal static class GetTranslateText
    {
        internal static string Get(string key)
        {
            try
            {
                if (GData.UIData.LanguageDic != null && GData.UIData.LanguageDic.TryGetValue(key, out string value))
                {
                    return value;
                }

                return "Text Not Found";
            }
            catch
            {
                return "Text Not Found";
            }
        }
    }
}
