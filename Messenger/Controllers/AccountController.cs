﻿using System;
using AutoMapper;
using Messenger.Filters;
using Messenger.Models.Account;
using Microsoft.AspNetCore.Mvc;
using Repository.Models;
using Repository.Repositories.AuthRepositories;
using Repository.Services;

namespace Messenger.Controllers
{
    public class AccountController : Controller
    {
        private Repository.Models.Account  _user => RouteData.Values["User"] as Repository.Models.Account;
        private readonly IMapper _mapper;
        private readonly IAuthRepository _authRepository;
        private readonly ISendEmail _emailService;

        public AccountController(IMapper mapper,
                                 IAuthRepository authRepository,
                                 ISendEmail sendEmail)
        {
            _mapper = mapper;
            _authRepository = authRepository;
            _emailService = sendEmail;
        }
        public IActionResult SignUp()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SignUp(RegisterViewModel model)
        {
            bool checkUser = _authRepository.CheckEmail(model.Email);
            bool number = _authRepository.CheckPhone(model.Phone);
            
            if (checkUser)
            {
                ModelState.AddModelError("Email", "Bu E-mail artiq movcuddur");
            }
            if (number)
            {
                ModelState.AddModelError("Phone", "Bu Nömrə artıq mövcuddur");
            }
            if (ModelState.IsValid)
            {
                var user = _mapper.Map<RegisterViewModel, Account>(model);
                user.Token = Guid.NewGuid().ToString();
                user.Status = true;
                user.IsEmailVerified = false;

                //email verification code
                user.EmailActivationCode = Guid.NewGuid().ToString();

                _authRepository.Register(user);

                //send verification link email
                string userFullname = user.Name + " " + user.Surname;

                string link = HttpContext.Request.Scheme + "://" + Request.Host + "/account/verifyemail/" + user.EmailActivationCode;

                _emailService.VerificationEmail(user.Email, link, user.EmailActivationCode, userFullname);

                Response.Cookies.Append("token", user.Token, new Microsoft.AspNetCore.Http.CookieOptions
                {
                    HttpOnly = true,
                    Expires = DateTime.Now.AddYears(1)
                });

                return RedirectToAction("chat1", "pages");
            }

            return View(model);
        }

        public IActionResult SignIn()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SignIn(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = _authRepository.Login(model.Email, model.Password);
                if (user != null)
                {

                    user.Token = Guid.NewGuid().ToString();
                    _authRepository.UpdateToken(user.Id, user.Token);
                    Response.Cookies.Delete("token");
                    Response.Cookies.Append("token", user.Token, new Microsoft.AspNetCore.Http.CookieOptions
                    {
                        Expires = model.RememberMe ? DateTime.Now.AddYears(1) : DateTime.Now.AddDays(1),
                        HttpOnly = true
                    });

                    if (user.IsEmailVerified == false)
                    {
                        return RedirectToAction("unverified", "account");
                    }

                    return RedirectToAction("chat1", "pages");
                }

                ModelState.AddModelError("Password", "E-poct veya Sifre yanlisdir...");

            }
            return View(model);
        }

        public IActionResult Logout()
        {
            Request.Cookies.TryGetValue("token", out string token);
            var user = _authRepository.CheckByToken(token);
            if (user.Token!= null)
            {
                _authRepository.UpdateToken(user.Id, null);
            }
            Response.Cookies.Delete("token");
            //return PartialView("chat1", "pages");
            return RedirectToAction("signin","account");
        }

        public IActionResult ResetPassword()
        {
            return View();
        }

        //dont using
        [HttpPost]
        public IActionResult ResetPassword(ResetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                Account account = _authRepository.GetByEmail(model.Email);
                if (account == null)
                {
                    ModelState.AddModelError("Email", "There's no Messenger App Account with the info you provided");
                    return View();
                }

                _emailService.ResetPassword(account);
                Response.Cookies.Delete("resetpassword");
                Response.Cookies.Append("resetpassword", account.ForgetToken, new Microsoft.AspNetCore.Http.CookieOptions
                {
                    Expires = DateTime.Now.AddDays(1),
                    HttpOnly = true
                });
                return RedirectToAction("resetpassconfirm", "account");
            }

            return Content("error");
        }

        [TypeFilter(typeof(ResetPassFilter))]
        public IActionResult ResetPassConfirm()
        {
            return View();
        }

        [TypeFilter(typeof(ResetPassFilter))]
        [HttpPost]
        public IActionResult ResetPassConfirm(ResetPasswordConfirmViewModel model)
        {
            if (ModelState.IsValid)
            {
                string forgettoken = Request.Cookies["forgettoken"];
                if (string.IsNullOrEmpty(forgettoken)) return BadRequest();
                Account account = _authRepository.GetByForgetToken(forgettoken);
                if (account != null)
                {
                    _authRepository.UpdatePassword(account.Id, model.Password);
                }
                Response.Cookies.Delete("forgettoken");
                return RedirectToAction("signin", "account");
            }

            return View();
        }

        //Email Verification Link Click View
        [TypeFilter(typeof(Auth))]
        [HttpGet]
        public IActionResult VerifyEmail()
        {
            string Url = Request.Path.Value;
            if (Url.Length < 22)
            {
                return NotFound();
            }
            string linkId = Url.Substring(21);


            if (_user.IsEmailVerified && _user.EmailActivationCode == "verified")
            {
                ViewBag.VerifiedAccount = true;
                return View();
            }

            ViewBag.VerifiedAccount = false;

            ViewBag.IsVerified = false;

            if (_user.EmailActivationCode == null || _user.EmailActivationCode == "expired")
            {
                ViewBag.Message = "Account Verification Link Has Expired !";
                ViewBag.IsVerified = false;

                return View();
            }

            if (_user.EmailActivationCode.ToString() == linkId)
            {
                ViewBag.Message = "Account Successfully Verified";
                ViewBag.IsVerified = true;

                _authRepository.VerifyUserEmail(_user.Id);

                return View();
            }

            ViewBag.Message = "Account Verification Link Has Expired !";
            ViewBag.IsVerified = false;

            return View();
        }

        [TypeFilter(typeof(Auth))]
        public IActionResult UnVerified()
        {
            if (_user.IsEmailVerified)
            {
                return NotFound();
            }

            return View();
        }

        public bool CheckEmailAddress(string userEmail) //checkemailaddress
        {

            if (string.IsNullOrEmpty(userEmail)) return false;

            if (_authRepository.CheckEmail(userEmail))
            {
                Account account = _authRepository.GetByEmail(userEmail);
                if (account == null) return false;
                _emailService.ResetPassword(account);
                return true;
            };

            return false;
           
        }
        public IActionResult CheckForgetCode(string userEmail, string inputResetPass)
        {
            if (string.IsNullOrEmpty(userEmail) || string.IsNullOrEmpty(inputResetPass))
            {
                return Ok(new { status = false });
            }

            Account account = _authRepository.GetByEmail(userEmail);
            if (account == null || string.IsNullOrEmpty(account.ForgetToken)) return Ok(new { status = false });

            if (_authRepository.CheckPasswordResetCode(account.Id, inputResetPass))
            {
                return Ok(new { status = true, forgetToken = account.ForgetToken });
            }

            //return NotFound();
            return Ok(new { status = false});
        }

    }
}