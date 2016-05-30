﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.AspNetCore.Http.Features.Authentication;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace MVC.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
        public async Task<IActionResult> Secure()
        {
            ViewBag.IdentityToken = await HttpContext.Authentication.GetIdentityTokenAsync();
            ViewBag.AccessToken = await HttpContext.Authentication.GetAccessTokenAsync();
            return View();
        }

        [Authorize]
        public async Task<IActionResult> CallApi()
        {
            var accessToken = await HttpContext.Authentication.GetAccessTokenAsync();
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await client.GetStringAsync("http://localhost:41575/claims");
            ViewBag.Json = JArray.Parse(response).ToString();

            return View();
        }
    }

    internal static class AuthenticateContextExtensions
    {
        public static async Task<string> GetIdentityTokenAsync(this AuthenticationManager manager)
        {
            var context = new AuthenticateContext("Cookies");
            await manager.AuthenticateAsync(context);

            return context.Properties[".Token.id_token"];
        }

        public static async Task<string> GetAccessTokenAsync(this AuthenticationManager manager)
        {
            var context = new AuthenticateContext("Cookies");
            await manager.AuthenticateAsync(context);

            return context.Properties[".Token.access_token"];
        }
    }
}
