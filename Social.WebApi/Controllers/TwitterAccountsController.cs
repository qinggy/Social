﻿using Framework.Core;
using Framework.Core.UnitOfWork;
using Social.Application.AppServices;
using Social.Application.Dto;
using Social.Infrastructure;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using Tweetinvi;
using Tweetinvi.Models;

namespace Social.WebApi.Controllers
{
    [RoutePrefix("api/twitter-accounts")]
    public class TwitterAccountsController : ApiController
    {
        private IUnitOfWorkManager _uowManager;
        private ITwitterAccountAppService _appService;

        public TwitterAccountsController(
            IUnitOfWorkManager uowManager,
            ITwitterAccountAppService appService)
        {
            _uowManager = uowManager;
            _appService = appService;
        }

        /// <summary>
        /// Make a integration request, fron-end page should redirect to this api rather than calling this api directly.
        /// </summary>
        /// <param name="redirectUri">After social user grant related permissions to our system, current page will redirect to this url.</param>
        /// <returns></returns>
        [HttpGet]
        [Route("integration-request")]
        public IHttpActionResult IntegrationRequest([Required]string redirectUri)
        {
            int siteId = _uowManager.Current.GetSiteId().Value;
            // Specify the url you want the user to be redirected to\
            string url = Request.RequestUri.Scheme + "://" + Request.RequestUri.Authority + Url.Route("ValidateAuth", new { siteId = siteId, redirectUri = redirectUri });

            IAuthenticationContext authenticationContext = _appService.InitAuthentication(url);
            return Redirect(authenticationContext.AuthorizationURL);
        }

        /// <summary>
        /// This api is used by twitter.
        /// </summary>
        /// <param name="authorization_id"></param>
        /// <param name="redirectUri"></param>
        /// <param name="oauth_verifier"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("validate-twitter-auth", Name = "ValidateAuth")]
        public async Task<IHttpActionResult> ValidateTwitterAuth(string authorization_id, string redirectUri, string oauth_verifier = null)
        {
            await _appService.AddAccountAsync(authorization_id, oauth_verifier);
            return Redirect(redirectUri);
        }

        /// <summary>
        /// Get accounts.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("accounts")]
        public IList<TwitterAccountListDto> GetAccounts()
        {
            return _appService.GetAccounts();
        }

        /// <summary>
        /// Get account by id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("accounts/{id}")]
        [ResponseType(typeof(TwitterAccountDto))]
        public IHttpActionResult GetAccount(int id)
        {
            var page = _appService.GetAccount(id);
            if (page == null)
            {
                return StatusCode(HttpStatusCode.NotFound);
            }

            return Ok(page);
        }

        /// <summary>
        /// update account.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("accounts/{id}")]
        public TwitterAccountDto UpdateAccount(int id, [Required]UpdateTwitterAccountDto dto)
        {
            var account = _appService.UpdateAccount(id, dto);
            return account;
        }

        /// <summary>
        ///  delete account.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("accounts/{id}")]
        public async Task<int> DeleteAccount(int id)
        {
            await _appService.DeleteAccountAsync(id);
            return id;
        }
    }
}