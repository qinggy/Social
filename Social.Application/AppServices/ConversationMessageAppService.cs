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
        FacebookMessageDto GetFacebookDirectMessage(int messageId);
        FacebookPostMessageDto GetFacebookPostMessages(int conversationId);
        FacebookPostCommentMessageDto GetFacebookPostCommentMessage(int messageId);
        IList<TwitterDirectMessageDto> GetTwitterDirectMessages(int conversationId);
        TwitterDirectMessageDto GetTwitterDirectMessage(int messageId);
        IList<TwitterTweetMessageDto> GetTwitterTweetMessages(int conversationId);
        TwitterTweetMessageDto GetTwitterTweetMessage(int messageId);
        TwitterTweetMessageDto ReplyTwitterTweetMessage(int conversationId, int tweetAccountId, string message, bool isCloseConversation = false);
        TwitterDirectMessageDto ReplyTwitterDirectMessage(int conversationId, string message, bool isCloseConversation = false);
        FacebookMessageDto ReplyFacebookMessage(int conversationId, string content, bool isCloseConversation = false);
        FacebookPostCommentMessageDto ReplyFacebookPostOrComment(int conversationId, int postOrCommentId, string content, bool isCloseConversation = false);
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

            var allComments = messages.Where(t => t.Source == MessageSource.FacebookPostComment || t.Source == MessageSource.FacebookPostReplyComment).Select(t => Mapper.Map<FacebookPostCommentMessageDto>(t)).ToList();
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
            result.ForEach(t => { t.ParentId = t.ParentId == null ? -1 : t.ParentId; }); // -1?, front-end need this value by now.

            var messageDtoWithQuote = result.Where(t => !string.IsNullOrWhiteSpace(t.QuoteTweetId)).FirstOrDefault();
            if (messageDtoWithQuote == null)
            {
                return result;
            }

            var messageWithQuote = _messageService.Find(messageDtoWithQuote.Id);
            var socialAccount = _socialAccountService.Find(messageWithQuote.IntegrationAccountId);
            if (socialAccount == null)
            {
                return result;
            }
            var quoteTweetMessage = _twitterService.GetTweetMessage(socialAccount, long.Parse(messageDtoWithQuote.QuoteTweetId));
            if (quoteTweetMessage != null)
            {
                result.Find(t => t.Id == messageDtoWithQuote.Id).QuoteTweet = Mapper.Map<BeQuotedTweetDto>(quoteTweetMessage);
              //  messageDtoWithQuote.QuoteTweet = Mapper.Map<BeQuotedTweetDto>(quoteTweetMessage);
            }

            return result;
        }

        public TwitterTweetMessageDto ReplyTwitterTweetMessage(int conversationId, int tweetAccountId, string messageContent, bool isCloseConversation = false)
        {
            Message message = _messageService.ReplyTwitterTweetMessage(conversationId, tweetAccountId, messageContent, isCloseConversation);
            var dto = Mapper.Map<TwitterTweetMessageDto>(message);
            dto.SendAgentName = _agentService.GetDiaplyName(dto.SendAgentId);
            return dto;
        }

        public TwitterDirectMessageDto ReplyTwitterDirectMessage(int conversationId, string messageContent, bool isCloseConversation = false)
        {
            Message message = _messageService.ReplyTwitterDirectMessage(conversationId, messageContent, isCloseConversation);
            var dto = Mapper.Map<TwitterDirectMessageDto>(message);
            dto.SendAgentName = _agentService.GetDiaplyName(dto.SendAgentId);
            return dto;
        }

        public FacebookMessageDto ReplyFacebookMessage(int conversationId, string content, bool isCloseConversation = false)
        {
            Message message = _messageService.ReplyFacebookMessage(conversationId, content, isCloseConversation);
            var dto = Mapper.Map<FacebookMessageDto>(message);
            dto.SendAgentName = _agentService.GetDiaplyName(dto.SendAgentId);
            return dto;
        }

        public FacebookPostCommentMessageDto ReplyFacebookPostOrComment(int conversationId, int postOrCommentId, string content, bool isCloseConversation = false)
        {
            Message message = _messageService.ReplyFacebookPostOrComment(conversationId, postOrCommentId, content, isCloseConversation);
            var dto = Mapper.Map<FacebookPostCommentMessageDto>(message);
            dto.SendAgentName = _agentService.GetDiaplyName(dto.SendAgentId);
            return dto;
        }

        public FacebookMessageDto GetFacebookDirectMessage(int messageId)
        {
            var message = _messageService.Find(messageId);
            var dto = Mapper.Map<FacebookMessageDto>(message);
            dto.SendAgentName = _agentService.GetDiaplyName(dto.SendAgentId);
            return dto;
        }

        public FacebookPostCommentMessageDto GetFacebookPostCommentMessage(int messageId)
        {
            var message = _messageService.Find(messageId);
            var dto = Mapper.Map<FacebookPostCommentMessageDto>(message);
            dto.SendAgentName = _agentService.GetDiaplyName(dto.SendAgentId);
            return dto;
        }

        public TwitterDirectMessageDto GetTwitterDirectMessage(int messageId)
        {
            var message = _messageService.Find(messageId);
            var dto = Mapper.Map<TwitterDirectMessageDto>(message);
            dto.SendAgentName = _agentService.GetDiaplyName(dto.SendAgentId);
            return dto;
        }

        public TwitterTweetMessageDto GetTwitterTweetMessage(int messageId)
        {
            var message = _messageService.Find(messageId);
            var dto = Mapper.Map<TwitterTweetMessageDto>(message);
            dto.SendAgentName = _agentService.GetDiaplyName(dto.SendAgentId);
            return dto;
        }
    }
}
