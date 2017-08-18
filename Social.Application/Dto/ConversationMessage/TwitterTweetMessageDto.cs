﻿using Social.Infrastructure;
using Social.Infrastructure.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Social.Application.Dto
{
    public class TwitterTweetMessageDto : IHaveSendAgent
    {
        public int Id { get; set; }
        public MessageSource Source { get; set; }
        public int ConversationId { get; set; }
        public string UserAvatar { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string UserScreenName { get; set; }
        public string UserEmail { get; set; }
        public int? ParentId { get; set; }
        public string QuoteTweetId { get; set; }
        public string Content { get; set; }
        public DateTime SendTime { get; set; }
        public int? SendAgentId { get; set; }
        public string SendAgentName { get; set; }

        public BeQuotedTweetDto QuoteTweet { get; set; }
        public List<MessageAttachmentDto> Attachments { get; set; }
    }
}
