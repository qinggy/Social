﻿using Framework.Core;
using Social.Domain.Entities;
using Social.Infrastructure.Facebook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tweetinvi.Models;

namespace Social.Domain.DomainServices
{
    public interface IUserInfoService
    {
        SocialUser GetUser(string OriginalId);
        List<Conversation> GetConversations(int userId);
        FbUser GetFacebookInfo(string OriginalId, Conversation conversation, int userId);
        IUser GetTwitterInfo(string OriginalId, Conversation conversation, int userId);
    }
    public class UserInfoService: DomainService<SocialAccount>, IUserInfoService
    {
        private IRepository<SocialUser> _socialUserRepo;
        private IRepository<Message> _messageRepo;
        private IRepository<Conversation> _conversationRepo;
        private IRepository<SocialAccount> _socialAccountRepo;
        public UserInfoService(IRepository<SocialUser> socialUserRepo, IRepository<Message> messageRepo, IRepository<Conversation> conversationRepo,
            IRepository<SocialAccount> socialAccountRepo)
        {
            _socialUserRepo = socialUserRepo;
            _messageRepo = messageRepo;
            _conversationRepo = conversationRepo;
            _socialAccountRepo = socialAccountRepo;
        }

        public SocialUser GetUser(string OriginalId)
        {
            List<SocialUser> users = _socialUserRepo.FindAll().Where(t => t.OriginalId == OriginalId).ToList();
            if(users.Count() == 1)
            {
                return users[0];
            }
            return null;
        }

        public List<Conversation> GetConversations(int userId)
        {
            List<int> conversationIds = _messageRepo.FindAll().Where(t => t.SenderId == userId || t.ReceiverId == userId).Select(t => t.ConversationId).Distinct().ToList();
            List<Conversation> conversations = new List<Conversation>();
            foreach (var conversationId in conversationIds)
            {
                conversations.Add(_conversationRepo.Find(conversationId));
            }
            return conversations;
        }

        public FbUser GetFacebookInfo(string OriginalId, Conversation conversation, int userId)
        {
            int? accountId = null;
            while (accountId == null)
            {
                accountId = conversation.Messages.First().SenderId == userId ? conversation.Messages.First().ReceiverId : conversation.Messages.First().SenderId;
            }
            string token = _socialAccountRepo.Find(accountId.Value).Token;
            FbUser fbUser =  FbClient.GetFacebookUserInfo(token, OriginalId);
            return fbUser;
        }

        public IUser GetTwitterInfo(string OriginalId, Conversation conversation, int userId)
        {
            var twitterService = DependencyResolver.Resolve<ITwitterService>();
            int? accountId = null;
            while (accountId == null)
            {
                accountId = conversation.Messages.First().SenderId == userId ? conversation.Messages.First().ReceiverId : conversation.Messages.First().SenderId;
            }
           SocialAccount socialAccount = _socialAccountRepo.Find(accountId.Value);
            IUser twitterUser = twitterService.GetUser(socialAccount, long.Parse(OriginalId));
            return twitterUser;
        }
    }
}