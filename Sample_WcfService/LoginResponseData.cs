using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace Sample_WcfService
{
    [DataContract]
    public class LoginResponseData
    {
        [DataMember(Order = 0)]
        public string token { get; set; }
        [DataMember(Order = 1)]
        public bool authenticated { get; set; }
        [DataMember(Order = 2)]
        public string EmpId { get; set; }
        [DataMember(Order = 3)]
        public string Name { get; set; }

        [DataMember(Order = 8)]
        public DateTime timestamp { get; set; }
        [DataMember(Order = 9)]
        public string Email { get; set; }
    }

}