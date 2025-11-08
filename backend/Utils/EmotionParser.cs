using Newtonsoft.Json.Linq;

namespace backend.Utils
{
    public static class EmotionParser
    {
        public static (string label, double score) ParseLabelScore(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                throw new Exception("Boş yanıt.");

            var s = raw.TrimStart();

            if (s.StartsWith("{") || s.StartsWith("["))
                return ParseFromJson(s);

            var lines = raw
                .Split(new[] { "\r\n", "\n" }, StringSplitOptions.None)
                .Select(l => l.Trim())
                .ToArray();

            string? dataLine = null;
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Equals("event: complete", StringComparison.OrdinalIgnoreCase))
                {
                    for (int j = i + 1; j < lines.Length; j++)
                    {
                        if (lines[j].StartsWith("data:"))
                        {
                            dataLine = lines[j];
                            break;
                        }
                    }
                    if (dataLine != null) break;
                }
            }
            dataLine ??= lines.FirstOrDefault(l => l.StartsWith("data:"));
            if (string.IsNullOrWhiteSpace(dataLine))
                throw new Exception("SSE içinde 'data:' satırı bulunamadı.");

            var jsonPart = dataLine.Substring("data:".Length).Trim();
            return ParseFromJson(jsonPart);
        }

        private static (string label, double score) ParseFromJson(string json)
        {
            var token = JToken.Parse(json);

            if (token is JArray arr && arr.Count > 0)
            {
                var item = arr[0];
                var label = item["label"]?.ToString() ?? item["label_raw"]?.ToString() ?? "unknown";
                var score = item["score"]?.Value<double>() ?? 0.0;
                return (label, score);
            }

            if (token is JObject obj)
            {
                if (obj["label"] != null && obj["score"] != null)
                    return (obj["label"]!.ToString(), obj["score"]!.Value<double>());

                var data = obj["data"];
                if (data is JArray outer && outer.Count > 0)
                {
                    var inner = outer[0];
                    if (inner is JArray innerArr && innerArr.Count > 0)
                    {
                        var item = innerArr[0];
                        var label = item["label"]?.ToString() ?? item["label_raw"]?.ToString() ?? "unknown";
                        var score = item["score"]?.Value<double>() ?? 0.0;
                        return (label, score);
                    }
                }
            }

            throw new Exception("Beklenmeyen JSON/SSE formatı.");
        }
    }
}
