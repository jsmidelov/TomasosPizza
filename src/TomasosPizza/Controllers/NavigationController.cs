﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using TomasosPizza.Models;
using TomasosPizza.ViewModels;

namespace TomasosPizza.Controllers
{
    public class NavigationController : Controller
    {
        private TomasosContext _context;

        public NavigationController(TomasosContext context)
        {
            _context = context;
        }

        public IActionResult MenuView()
        {
            MenuViewModel model = new MenuViewModel();
            
            foreach (var product in _context.Matratt)
            {
                model.Menu.Add(product);
            }
            if (HttpContext.Session.GetString("Order") != null)
            {
                string str = HttpContext.Session.GetString("Order");
                model.Order = JsonConvert.DeserializeObject<List<BestallningMatratt>>(str);
            }
            
            return View(model);
        }

        public IActionResult OrderView()
        {
            // get the order data
            // List<BestallningMatratt> order
            Bestallning model = new Bestallning();
            var str = HttpContext.Session.GetString("Order");
            var order = JsonConvert.DeserializeObject<List<BestallningMatratt>>(str);
            model.BestallningMatratt = order;
            
            // if no customer is logged in, ask user to log in
            if (HttpContext.Session.GetString("User") == null) 
                return RedirectToAction("LogInView", "Navigation");
            
            // get the logged in customer
            var id = int.Parse(HttpContext.Session.GetString("User"));
            var user = _context.Kund.FirstOrDefault(u => u.KundId == id);
            model.Kund = user;
            // calculate price in method to make forward-compatible with discounts
            model.Totalbelopp = CalculatePrice(order, user);
            model.BestallningDatum = DateTime.Now;
            model.Levererad = false;
            return View(model);
        }

        private int CalculatePrice(List<BestallningMatratt> order, Kund user)
        {
            return order.Sum(x => x.Matratt.Pris);
        }

        public IActionResult ThankYou()
        {
            return View();
        }
        public IActionResult LogInView()
        {
            return View();
        }
        public IActionResult RegisterView()
        {
            return View();
        }

        public IActionResult UserEdit()
        {
            int userId = int.Parse(HttpContext.Session.GetString("User"));
            Kund user = _context.Kund.First(u => u.KundId == userId);
            return View(user);
        }
    }
}