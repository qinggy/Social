﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Social.Application.Dto
{
    public class FacebookMessagesDto
    {
        [Required]
        [MaxLength(2000)]
        public string message { get; set; }
    }
}
