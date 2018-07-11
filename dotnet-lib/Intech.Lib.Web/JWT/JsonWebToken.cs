namespace Intech.Lib.Web.JWT
{
    public class JsonWebToken
    {
        public string AccessToken { get; set; }
        public bool Authenticated { get; internal set; }
        public string Created { get; internal set; }
        public string Expiration { get; internal set; }
        public string Message { get; internal set; }
    }
}
