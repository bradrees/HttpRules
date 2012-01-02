using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RulesWPF.Models
{
    public class ResponseModel
    {
        private string responseCodeText;

        public int ResponseCode { get; set; }

        public string FullUrl { get; set; }

        public string ResponseCodeText 
        {
            get 
            {
                return responseCodeText ?? ((System.Net.HttpStatusCode)ResponseCode).ToString();
            }
            set
            {
                responseCodeText = value;
            }
        }

        public string Referer { get; set; }
    }
}
