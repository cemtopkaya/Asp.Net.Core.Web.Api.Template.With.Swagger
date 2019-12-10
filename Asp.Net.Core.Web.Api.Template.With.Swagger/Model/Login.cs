﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Asp.Net.Core.Web.Api.Template.With.Swagger.Model
{
    public class Login
    {
        public Login() {}
        public Login(string userName, string pass) {
            this.UserName = userName;
            this.Password = pass;
        }

        public string UserName { get; set; }

        public string Password { get; set; }
    }
}
