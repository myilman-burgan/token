using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace amorphie.token.core.Models.User
{
    public class SecurityImageRequestDto
    {
        public Guid Id{get;set;}
        public string Image{get;set;}
        public string EnTitle{get;set;}
        public string TrTitle{get;set;}
        public DateTime CreatedAt{get;set;}
        public Guid CreatedBy{get;set;}
        public Guid CreatedByBehalfOf{get;set;}
        public DateTime ModifiedAt{get;set;}
        public Guid ModifiedBy{get;set;}
        public Guid ModifiedByBehalfOf{get;set;}
    }
}