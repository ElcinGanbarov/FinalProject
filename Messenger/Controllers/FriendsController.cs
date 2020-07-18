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

namespace Messenger.Controllers
{
    [TypeFilter(typeof(Auth))]
    public class FriendsController : Controller
    {
        private readonly IAuthRepository _authRepository;
        private readonly IMapper _mapper;
        private readonly IFriendsRepository _friendsRepository;
        private Repository.Models.Account _user => RouteData.Values["User"] as Repository.Models.Account;


        public FriendsController(IAuthRepository authRepository,
                                       IMapper mapper,
                                       IAccountDetailRepository accountDetailRepository,
                                       IFriendsRepository friendsRepository)
        {
            _authRepository = authRepository;
            _mapper = mapper;
            _friendsRepository = friendsRepository;
        }

        [HttpPost]
        public IActionResult AllFriends(int userId)
        {
            ICollection<Account> friends = _friendsRepository.GetAllFriends(userId);

            return Ok(friends);
        }
    }
}