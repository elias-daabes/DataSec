﻿using DataSec.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DataSec.ViewModels
{
    public class AccountViewModel
    {
        public Account account { get; set; }
        public List<Account> accountsList { get; set; }
    }
}