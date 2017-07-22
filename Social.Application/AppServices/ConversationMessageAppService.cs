﻿using AutoMapper;
using AutoMapper.QueryableExtensions;
using Framework.Core;
using Social.Application.Dto;
using Social.Domain.DomainServices;
using Social.Domain.Entities;
using Social.Infrastructure;
using Social.Infrastructure.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Social.Application.AppServices
{
    public interface IConversationMessageAppService
    {
        IList<FacebookMessageDto> GetFacebookDirectMessages(int conversationId);
        FacebookPostMessageDto GetFacebookPostMessages(int conversationId);
        IList<TwitterDirectMessageDto> GetTwitterDirectMessages(int conversationId);
        IList<TwitterTweetMessageDto> GetTwitterTweetMessages(int conversationId);
        void ReplyTwitterTweetMessage(int conversationId, int tweetAccountId, int parentId, string message);
        void ReplyTwitterDirectMessage(int conversationId, int tweetAccountId, string message);
        void ReplyFacebookMessage(int conversationId, string content);
        void ReplyFacebookPostOrComment(int conversationId, int parentId, string content);
    }

    public class ConversationMessageAppService : AppService, IConversationMessageAppService
    {
        private IConversationService _conversationService;
        private IMessageService _messageService;
        private IAgentService _agentService;
        private ITwitterService _twitterService;
        private ISocialAccountService _socialAccountService;

        public ConversationMessageAppService(
            IConversationService conversationService,
            IAgentService agentService,
            IMessageService messageService,
            ITwitterService twitterService,
            ISocialAccountService socialAccountService
            )
        {
            _conversationService = conversationService;
            _agentService = agentService;
            _messageService = messageService;
            _twitterService = twitterService;
            _socialAccountService = socialAccountService;
        }


        public FacebookPostMessageDto GetFacebookPostMessages(int conversationId)
        {
            var converation = _conversationService.Find
                (conversationId, new[] { ConversationSource.FacebookVisitorPost, ConversationSource.FacebookWallPost });
            if (converation == null)
            {
                return null;
            }

            var messages = _messageService.FindAllByConversationId(conversationId).ToList();
            var postMessage = messages.FirstOrDefault(t => t.Source == MessageSource.FacebookPost);
            if (postMessage == null)
            {
                return null;
            }
            var postDto = Mapper.Map<FacebookPostMessageDto>(postMessage);

            var allComments = messages.Where(t => t.Source == MessageSource.FacebookPostComment).Select(t => Mapper.Map<FacebookPostCommentMessageDto>(t)).ToList();
            _agentService.FillAgentName(allComments.Cast<IHaveSendAgent>());

            postDto.Comments = allComments.Where(t => t.ParentId == postDto.Id).OrderBy(t => t.SendTime).ToList();
            foreach (var comment in postDto.Comments)
            {
                comment.ReplyComments = allComments.Where(t => t.ParentId == comment.Id).OrderBy(t => t.SendTime).ToList();
            }

            return postDto;
        }

        public IList<FacebookMessageDto> GetFacebookDirectMessages(int conversationId)
        {
            List<FacebookMessageDto> result = new List<FacebookMessageDto>();

            var converation = _conversationService.Find(conversationId, ConversationSource.FacebookMessage);
            if (converation == null)
            {
                return result;
            }

            result = _messageService.FindAllByConversationId(conversationId)
                .OrderBy(t => t.SendTime)
                .ProjectTo<FacebookMessageDto>()
                .ToList();

            _agentService.FillAgentName(result.Cast<IHaveSendAgent>());

            return result;
        }

        public IList<TwitterDirectMessageDto> GetTwitterDirectMessages(int conversationId)
        {
            List<TwitterDirectMessageDto> result = new List<TwitterDirectMessageDto>();
            var converation = _conversationService.Find(conversationId, ConversationSource.TwitterDirectMessage);
            if (converation == null)
            {
                return result;
            }

            result = _messageService.FindAllByConversationId(conversationId)
                .OrderBy(t => t.SendTime)
                .ProjectTo<TwitterDirectMessageDto>()
                .ToList();
            _agentService.FillAgentName(result.Cast<IHaveSendAgent>());

            return result;
        }

        public IList<TwitterTweetMessageDto> GetTwitterTweetMessages(int conversationId)
        {
            List<TwitterTweetMessageDto> result = new List<TwitterTweetMessageDto>();
            var converation = _conversationService.Find(conversationId, ConversationSource.TwitterTweet);
            if (converation == null)
            {
                return result;
            }

            result = _messageService.FindAllByConversationId(conversationId)
                .OrderBy(t => t.SendTime)
                .ProjectTo<TwitterTweetMessageDto>()
                .ToList();
            _agentService.FillAgentName(result.Cast<IHaveSendAgent>());

            var quotedMessageDto = result.Where(t => !string.IsNullOrWhiteSpace(t.QuoteTweetId)).FirstOrDefault();
            if (quotedMessageDto == null)
            {
                return result;
            }

            var socialAccount = _socialAccountService.Find(quotedMessageDto.UserId);
            if (socialAccount == null)
            {
                return result;
            }
            var quoteTweetMessage = _twitterService.GetTweetMessage(socialAccount, long.Parse(quotedMessageDto.QuoteTweetId));
            if (quoteTweetMessage != null)
            {
                quotedMessageDto.QuoteTweet = Mapper.Map<BeQuotedTweetDto>(quoteTweetMessage);
            }

            return result;
        }

        public void ReplyTwitterTweetMessage(int conversationId, int tweetAccountId, int parentId, string message)
        {
            _messageService.ReplyTwitterTweetMessage(conversationId, tweetAccountId, parentId, message);
        }

        public void ReplyTwitterDirectMessage(int conversationId, int tweetAccountId, string message)
        {
            _messageService.ReplyTwitterDirectMessage(conversationId, tweetAccountId, message);
        }

        public void ReplyFacebookMessage(int conversationId, string content)
        {
            _messageService.ReplyFacebookMessage(conversationId, content);
        }

        public void ReplyFacebookPostOrComment(int conversationId, int parentId, string content)
        {
            _messageService.ReplyFacebookPostOrComment(conversationId, parentId, content);
        }
    }
}
