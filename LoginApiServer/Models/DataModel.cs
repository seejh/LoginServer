using System.ComponentModel.DataAnnotations.Schema;

namespace LoginApiServer.Models
{
    /*---------------------------------------------------------------------
     Custom Login Models
     ---------------------------------------------------------------------*/
    [Table("Account")]
    public class AccountDb
    {
        public int AccountDbId { get; set; }
        public string AccountName { get; set; }
        public string AccountPw { get; set; }
    }

    public class ServerInfo
    {
        public string Name { get; set; }
        public string IpAddress { get; set; }
        public int Port { get; set; }
        public int BusyScore { get; set;}
    }

    public class LoginAccountReq
    {
        public string AccountName { get; set; }
        public string AccountPw { get; set; }
    }

    public class LoginAccountRes
    {
        public bool LoginOk { get; set; }
        public string AccountName { get; set; }
        public string Token { get; set; }
        public List<ServerInfo> ServerList { get; set; } = new List<ServerInfo>();
    }
}
