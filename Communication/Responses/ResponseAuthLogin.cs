namespace Communication.Responses
{
    public class ResponseAuthLogin
    {
        public string Token { get; set; }
        public long TokenExpired { get; set; }
    }
}
