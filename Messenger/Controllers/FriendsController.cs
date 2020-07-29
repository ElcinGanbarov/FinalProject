﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Messenger.Filters;
using Microsoft.AspNetCore.Mvc;
using Repository.Models;
using Repository.Repositories.AccountRepository;
using Repository.Repositories.AuthRepositories;
using Repository.Repositories.SearchRepository;

namespace Messenger.Controllers
{
    [TypeFilter(typeof(Auth))]
    public class FriendsController : Controller
    {
        private readonly IAuthRepository _authRepository;
        private readonly IAccountDetailRepository _accountDetailRepository;
        private readonly IMapper _mapper;
        private readonly IFriendsRepository _friendsRepository;
        private readonly ISearchRepository _searchRepository;
        private Repository.Models.Account _user => RouteData.Values["User"] as Repository.Models.Account;


        public FriendsController(IAuthRepository authRepository,
                                       IMapper mapper,
                                       IAccountDetailRepository accountDetailRepository,
                                       IFriendsRepository friendsRepository,
                                       ISearchRepository searchRepository)
        {
            _authRepository = authRepository;
            _accountDetailRepository = accountDetailRepository;
            _mapper = mapper;
            _friendsRepository = friendsRepository;
            _searchRepository = searchRepository;
        }

        [HttpPost]
        public IActionResult AllFriends(int userId)
        {
            ICollection<Account> friends = _friendsRepository.GetAllFriends(userId);

            return Ok(friends);
        }

        [HttpPost]
        public IActionResult GetFriendInfo(int friendId)
        {
            return Ok(_accountDetailRepository.GetDatasFriend(friendId));

            //return Ok(StatusCode(404));
        }

        [HttpPost]
        public IActionResult FriendSocialLinks(int friendId)
        {
            AccountSocialLink friendsocials = _friendsRepository.GetFriendSocialLinks(friendId);
            if (friendsocials != null)
            {
                return Ok(friendsocials);
            };

            return Ok(StatusCode(404));
        }

        //testing

        public IActionResult testing()
        {
            //Account account = _friendsRepository.GetFriendById(9025);
            //return Ok(account);

            return Ok(_accountDetailRepository.GetDatasOwn(3024));
        }

        [HttpGet]
        public async Task<IActionResult> SearchAccount()
        {
            try
            {
                string term = HttpContext.Request.Query["term"].ToString();
                var results = _searchRepository.SearchAccounts(_user.Id, term);
                return Ok(results);
            }
            catch
            {
                return BadRequest();
            }
        }
    }
}