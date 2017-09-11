﻿using AutoMapper;
using AutoMapper.QueryableExtensions;
using Framework.Core;
using Social.Application.Dto;
using Social.Domain.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Social.Application.AppServices
{
    public interface IMessageAttachmentAppService
    {
        Task GetRawDataJob();
        MessageAttachmentUrlDto GetAttachmentDto(int id);
    }
    public class MessageAttachmentAppService : ServiceBase,IMessageAttachmentAppService
    {
        IRepository<MessageAttachment> _messageAttachmentRepo;

        public MessageAttachmentAppService(IRepository<MessageAttachment> messageAttachmentRepo)
        {
            _messageAttachmentRepo = messageAttachmentRepo;
        }

        public async Task<List<MessageAttachmentRawDto>> GetMessageAttachments()
        {
            var messageAttachments = _messageAttachmentRepo.FindAll()
               .Where(t => t.Id == 2118)
               .ToList();
            //var messageAttachments = _messageAttachmentRepo.FindAll()
            //    .Where(t => (t.MimeType.Contains("image") || t.MimeType.Contains("audio") || t.MimeType.Contains("video"))
            //    && t.RawData == null && t.Message.SendTime > DateTime.UtcNow.AddDays(-4))
            //    .ToList();
            var messageAttachmentRawDtos = new List<MessageAttachmentRawDto>();
            foreach(var messageAttachment in messageAttachments)
            {
                var messageAttachmentRawDto = new MessageAttachmentRawDto();
                messageAttachmentRawDto.Id = messageAttachment.Id;
                messageAttachmentRawDto.Url = messageAttachment.Url;
                messageAttachmentRawDtos.Add(messageAttachmentRawDto);
            }
            return messageAttachmentRawDtos;
        }

        public async Task<List<MessageAttachmentRawDto>> AddRawData(List<MessageAttachmentRawDto> messageAttachmentRawDtos)
        {
            foreach(var messageAttachmentRawDto in messageAttachmentRawDtos)
            {
                string UrlImg = messageAttachmentRawDto.Url;
                if (UrlCheck(UrlImg))
                {
                    WebClient webClient = new WebClient();
                    webClient.Credentials = CredentialCache.DefaultCredentials;
                    byte[] byteData = webClient.DownloadData(UrlImg);
                    messageAttachmentRawDto.RawDate = byteData;
                }
            }
            return messageAttachmentRawDtos;
        }
        private bool UrlCheck(string strUrl)
        {
            if (!strUrl.Contains("http://") && !strUrl.Contains("https://"))
            {
                strUrl = "http://" + strUrl;
            }
            try
            {
                HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(strUrl);
                myRequest.Method = "HEAD";
                myRequest.Timeout = 10000;  //超时时间10秒
                HttpWebResponse res = (HttpWebResponse)myRequest.GetResponse();
                return (res.StatusCode == HttpStatusCode.OK);
            }
            catch
            {
                return false;
            }
        }
        public async Task UpdateMessageAttachment(List<MessageAttachmentRawDto> messageAttachmentRawDtos)
        {
            foreach (var messageAttachmentRawDto in messageAttachmentRawDtos)
            {
                var messageAttachment = _messageAttachmentRepo.Find(messageAttachmentRawDto.Id);
                messageAttachment.RawData = messageAttachmentRawDto.RawDate;
                _messageAttachmentRepo.Update(messageAttachment);
                await CurrentUnitOfWork.SaveChangesAsync();
            }
        }

        public async Task GetRawDataJob()
        {
            List<MessageAttachmentRawDto> messageAttachmentRawDtos = await GetMessageAttachments();
            messageAttachmentRawDtos = await AddRawData(messageAttachmentRawDtos);
            await UpdateMessageAttachment(messageAttachmentRawDtos);
        }

        public MessageAttachmentUrlDto GetAttachmentDto(int id)
        {
            var messageAttachment = _messageAttachmentRepo.Find(id);
            return Mapper.Map<MessageAttachmentUrlDto>(messageAttachment);
        }
    }
}
