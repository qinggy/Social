﻿using Social.Infrastructure.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Social.Application.Dto
{
    public class FilterUpdateDto
    {
        public int Id { get; set; }
        public FilterUpdateDto()
        {
            Conditions = new List<FilterConditionCreateDto>();
        }
        //   public int ConversationId { get; set; }
        [Required]
        [MaxLength(200)]
        public string Name { get; set; }
        public int Index { get; set; }
        public bool IfPublic { get; set; }
        public FilterType Type { get; set; }
        public string LogicalExpression { get; set; }

        // public virtual Conversation Conversation { get; set; }
        public virtual IList<FilterConditionCreateDto> Conditions { get; set; }
    }
}
