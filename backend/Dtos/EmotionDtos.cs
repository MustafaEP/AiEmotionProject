namespace backend.Dtos
{
    public class EmotionDtos
    {
        public class EmotionPostPayload
        {
            public string[] data { get; set; } = Array.Empty<string>();
        }

        public class EventIdResponse
        {
            public string event_id { get; set; } = string.Empty;
        }
    }
}
