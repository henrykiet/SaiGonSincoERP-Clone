using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Sinco.Server.Models
{
    public static class FormConfigHelper
    {
        public static string UpdateInitialDataInFormConfig(
            string formConfigJson,
            List<List<Dictionary<string, object>>> tables)
        {
            var config = JObject.Parse(formConfigJson);

            // ✅ Table 0 → gán cho form chính
            var form = config["tabs"]?[0]?["form"];
            if (form != null)
            {
                var init = form["initialData"];
                if (init == null || init.Type == JTokenType.Null || (init.Type == JTokenType.Object && !init.HasValues))
                {
                    form["initialData"] = tables.Count > 0 && tables[0].Count > 0
                        ? JObject.FromObject(tables[0][0])
                        : new JObject();
                }
            }

            // ✅ Table 1+ → gán cho detail[i-1]
            var detailArray = config["tabs"]?[0]?["detail"] as JArray;
            if (detailArray != null)
            {
                for (int i = 0; i < detailArray.Count; i++)
                {
                    var detail = detailArray[i];
                    var init = detail["initialData"];

                    if (init == null || init.Type == JTokenType.Null || (init.Type == JTokenType.Array && !init.HasValues))
                    {
                        if (tables.Count > i + 1)
                            detail["initialData"] = JArray.FromObject(tables[i + 1]);
                        else
                            detail["initialData"] = new JArray();
                    }
                }
            }

            return config.ToString(Formatting.None);
        }
    }

}
