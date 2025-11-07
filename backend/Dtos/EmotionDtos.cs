namespace backend.NewFolder
{
    public class EmotionDtos
    {
        public class EmotionPostPayload
        {
            public string[] data { get; set; }
        }

        public class EventIdResponse
        {
            public string event_id { get; set; }
        }
    }
}
